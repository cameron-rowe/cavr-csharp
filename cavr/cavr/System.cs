using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

using NLog;
using NLog.Conditions;
using NLog.Config;
using NLog.Targets;

using cavr.com;
using cavr.config;
using cavr.input;
using cavr.util;

namespace cavr
{
    public delegate void FunctionCallback();

	public static class System
	{
		private static Logger log;

		private static Data data = Data.CreateEmpty();
		private static ThreadLocal<object> contextData = new ThreadLocal<object>();

		public static bool Init(string[] args, InputMap inputMap) {
			InitLogging();

			Communications.Initialize();

			data.terminated = false;

			if(!InputManager.MapInputs(inputMap)) {
				log.Error("Failed to map inputs");
				return false;
			}

			var configPaths = Paths.GetConfigPaths();

			string configPath;
			if(!File.Find("cavrconfig.lua", configPaths, out configPath)) {
				log.Error("Failed to find config file cavrconfig.lua");
				return false;
			}

			var configReader = LuaReader.CreateFromFile(configPath);
			if(configReader == null) {
				log.Error("Failed to read config file");
				return false;
			}

			var globalSpec = ConfigurationSpecification.CreateFromSchema("globals.lua", "globals");
			if(globalSpec == null) {
				log.Error("Failed to open globals schema");
				return false;
			}

			var globalConfig = new Configuration();
			if(!globalSpec.Configure(configReader, string.Empty, globalConfig)) {
				log.Error("Failed to get global configuration");
				return false;
			}

			var machineConfigNames = globalConfig.Get<List<string>>("machines.__keys");
			data.numMachines = machineConfigNames.Count;

			// TODO: FIX THIS
			data.master = args.Length == 0; //args.Where(arg => arg.Contains("--cavr_master")).Count() == 0;
			var hostname = Environment.MachineName;

			if(data.master) {
				var cavrMasterKey = string.Empty;
				foreach(var n in machineConfigNames) {
					var configHostname = globalConfig.Get<string>("machines." + n + ".hostname");
					if(hostname == configHostname) {
						cavrMasterKey = n;
						break;
					}
				}

				if(cavrMasterKey == string.Empty) {
					log.Error("The machine this is running on is not configured");
					return false;
				}

				var cwd = Environment.CurrentDirectory;
				foreach(var n in machineConfigNames) {
					if(n == cavrMasterKey) {
						continue;
					}

					var sshTarget = globalConfig.Get<string>("machines." + n + ".ssh");
					var sb = new StringBuilder();
					sb.AppendFormat("{0} cd {1} && {2}", sshTarget, cwd, AppDomain.CurrentDomain.FriendlyName);
					foreach(var arg in args) {
						sb.AppendFormat(" {0}", arg);
					}

					sb.AppendFormat(" --cavr_master={0}", cavrMasterKey);

					var remotePid = new Process();
					remotePid.StartInfo = new ProcessStartInfo("ssh", sb.ToString());
					var success = remotePid.Start();

					if(!success) {
						log.Error("Failed to start remote process on {0}", n);
						return false;
					}

					data.remoteProcesses.Add(remotePid);
				}
			}

			string pluginsFilePath;
			if(!File.Find("cavrplugins.lua", configPaths, out pluginsFilePath)) {
				log.Error("Failed to find plugins file cavrplugins.lua");
				return false;
			}

			var pluginsFileReader = LuaReader.CreateFromFile(pluginsFilePath);
			if(pluginsFileReader == null) {
				log.Error("Failed to read plugins file {0}", pluginsFilePath);
				return false;
			}

			var pluginsFileSpec = ConfigurationSpecification.CreateFromSchema("plugins_file.lua", "plugins_file");
			if(pluginsFileSpec == null) {
				log.Error("Failed to open plugins schema");
				return false;
			}

			var pluginsFileConfig = new Configuration();
			if(!pluginsFileSpec.Configure(pluginsFileReader, "", pluginsFileConfig)) {
				log.Error("Failed to get plugins file data");
				return false;
			}

			var pluginPaths = pluginsFileConfig.Get<List<string>>("plugins");
			foreach(var n in pluginPaths) {
				var generator = PluginUtils.LoadPlugin(n);
				if(generator == null) {
					log.Error("Failed to load plugin: {0}", n);
					continue;
				}

				data.pluginGenerators[generator.Name] = generator;
			}

			foreach(var n in machineConfigNames) {
				var configHostname = globalConfig.Get<string>("machines." + n + ".hostname");
				if(hostname == configHostname) {
					data.machineName = n;
					break;
				}
			}

			if(data.machineName == string.Empty) {
				log.Error("Machine with hostname {0} is not configured", hostname);
				return false;
			}

			var machinePrefix = "machines." + data.machineName;

			var pluginKeys = globalConfig.Get<List<string>>(machinePrefix + ".plugins.__keys");
			foreach(var n in pluginKeys) {
				var pluginPrefix = machinePrefix + ",plugins.__keys";
				var pluginType = globalConfig.Get<string>(pluginPrefix + ".type");
				PluginGeneratorBase gen;
				if(!data.pluginGenerators.TryGetValue(pluginType, out gen)) {
					log.Error("Could not find plugin of type {0} for {1}", pluginType, pluginPrefix);

					foreach(var pair in data.pluginGenerators) {
						log.Error("  {0}", pair.Key);
					}

					log.Error("Skipping load of {0}", pluginPrefix);
					continue;
				}

				var pluginSpec = gen.Configuration;
				var pluginConfig = new Configuration();
				if(!pluginSpec.Configure(configReader, pluginPrefix, pluginConfig)) {
					log.Error("Failed to configure {0}", pluginPrefix);
					log.Error("Skipping load of {0}", pluginPrefix);
					continue;
				}

				var plugin = gen.Generate();
				pluginConfig.PushPrefix(pluginPrefix + ".");
				if(!plugin.Init(pluginConfig)) {
					log.Error("Failed to initialize plugin {0}", pluginPrefix);
					log.Error("Skipping load of {0}", pluginPrefix);
					continue;
				}

				data.plugins[pluginPrefix] = plugin;
			}

			var syncPort = (int) globalConfig.Get<double>("sync_port");
			var dataPort = (int) globalConfig.Get<double>("data_port");

			if(data.master) {
				data.syncSocket = Communications.CreateReplier(syncPort);
				data.pubsubSocket = Communications.CreatePublisher(dataPort);
			}
			else {
				var masterKey = args.First(arg => arg.Contains("--cavr_master")).Split('=')[1];
				var masterAddress = globalConfig.Get<string>("machines." + masterKey + ".address");

				data.syncSocket = Communications.CreateRequester(masterAddress, syncPort);
				data.pubsubSocket = Communications.CreateSubscriber(masterAddress, dataPort);
			}

			if(data.syncSocket == null || data.pubsubSocket == null) {
				log.Error("Failed to setup network");
				return false;
			}

			if(!InputManager.Initialize(data.syncSocket, data.pubsubSocket, data.master, data.numMachines)) {
				log.Error("Failed to initialize inputs");
				return false;
			}

			return true;
		}

		public static void SetCallback(string name, FunctionCallback f) {
			data.callbacks[name] = f;
		}

		public static FunctionCallback GetCallback(string name) {
			FunctionCallback f;
			if(!data.callbacks.TryGetValue(name, out f)) {
				log.Error("No callback named {0}", name);
				return () => {};
			}

			return f;
		}

		public static void Run() {
			var updateFunction = GetCallback("update");
			var master = data.master;
			var pubsubFunction = GetCallback(master ? "publish_data" : "receive_data");

			FunctionCallback updateThread = () => {
				while(!Terminated()) {
					if(master) {
						pubsubFunction();
					}
					if(!InputManager.Sync()) {
						log.Error("Failed to sync InputManager");
						return;
					}
					if(!master) {
						pubsubFunction();
					}

					updateFunction();
				}
			};

			var t = new Thread(() => updateThread());
			t.Start();
			data.threads.Add(t);

			foreach(var pair in data.plugins) {
				var plugin = pair.Value;
				FunctionCallback threadFunc = () => {
					while(!Terminated()) {
						plugin.Step();
					}
				};

				t = new Thread(() => threadFunc());
				t.Start();
				data.threads.Add(t);
			}

			foreach(var thread in data.threads) {
				thread.Join();
			}
		}

		public static double Dt() {
			return InputManager.Dt();
		}

		public static void SetSyncData(string data) {
			InputManager.SetSyncData(data);
		}

		public static string GetSyncData() {
			return InputManager.GetSyncData();
		}

		public static void Shutdown() {
			data.terminated = true;
		}

		public static bool Terminated() {
			return data.terminated;
		}

		public static object GetContextData() {
			return contextData.Value;
		}

		public static void SetContextData(object data) {
			contextData.Value = data;
		}

		private static void InitLogging() {
			var config = new LoggingConfiguration();
			var consoleTarget = new ColoredConsoleTarget();

			var highlightrule = new ConsoleRowHighlightingRule();
			highlightrule.Condition = ConditionParser.ParseExpression("level == LogLevel.Warn");
			highlightrule.ForegroundColor = ConsoleOutputColor.Yellow;
			consoleTarget.RowHighlightingRules.Add(highlightrule);

			highlightrule = new ConsoleRowHighlightingRule();
			highlightrule.Condition = ConditionParser.ParseExpression("level == LogLevel.Error");
			highlightrule.ForegroundColor = ConsoleOutputColor.Red;
			consoleTarget.RowHighlightingRules.Add(highlightrule);

			config.AddTarget("console", consoleTarget);

			consoleTarget.Layout = @"[${date:format=h\:mm\:ss tt} ${logger:shortName=true} ${level:uppercase=true}] -- ${message}";
			var rule = new LoggingRule("*", LogLevel.Info, consoleTarget);
			config.LoggingRules.Add(rule);

			LogManager.Configuration = config;

			log = LogManager.GetCurrentClassLogger();
		}

		private struct Data
		{
			public Dictionary<string, FunctionCallback> callbacks;
			public bool terminated;
			public bool master;
			public List<Thread> threads;
			public List<Process> remoteProcesses;
			public string machineName;
			public Dictionary<string, PluginGeneratorBase> pluginGenerators;
			public Dictionary<string, Plugin> plugins;
			public int numMachines;
			public Socket syncSocket;
			public Socket pubsubSocket;

			public static Data CreateEmpty() {
				return new Data {
					callbacks = new Dictionary<string, FunctionCallback>(),
					terminated = false,
					master = false,
					threads = new List<Thread>(),
					remoteProcesses = new List<Process>(),
					machineName = string.Empty,
					pluginGenerators = new Dictionary<string, PluginGeneratorBase>(),
					plugins = new Dictionary<string, Plugin>(),
					numMachines = 0,
					syncSocket = null,
					pubsubSocket = null
				};
			}
		}
	}
}

