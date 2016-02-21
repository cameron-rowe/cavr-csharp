using System;
using System.Collections.Generic;

using NLog;

using cavr.config;

namespace cavr.input
{
	public class Specification
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		private Dictionary<string, Switch> switches;
		private Dictionary<string, Analog> analogs;
		private Dictionary<string, SixDOF> sixdofs;

		public Specification()
		{
			switches = new Dictionary<string, Switch>();
			analogs = new Dictionary<string, Analog>();
			sixdofs = new Dictionary<string, SixDOF>();
		}

		public static Specification CreateFromLuaFile(string path) {
			var inputsFileSpec = ConfigurationSpecification.CreateFromSchema("inputs_file.lua", "inputs_file");
			if(inputsFileSpec == null) {
				log.Error("Failed to open inputs file {0}", path);
				return null;
			}

			var reader = LuaReader.CreateFromFile(path);
			if(reader == null) {
				log.Error("Failed to open inputs file {0}", path);
				return null;
			}

			var configuration = new Configuration();
			if(!inputsFileSpec.Configure(reader, string.Empty, configuration)) {
				log.Error("Failed to configure inputs file {0}", path);
				return null;
			}

			var spec = new Specification();
			var switchKeys = configuration.Get<List<string>>("switches.__keys");
			foreach(var k in switchKeys) {
				var prefix = "switches." + k;
				var stateKeys = configuration.Get<List<string>>(prefix + ".states.__keys");
				var stateNames = new List<string>();
				var stateDescriptions = new List<string>();

				foreach(var s in stateKeys) {
					var p = string.Format("{0}.states.{1}", prefix, s);
					stateNames.Add(configuration.Get<string>(p + ".name"));
					stateDescriptions.Add(configuration.Get<string>(p + ".description"));
				}

				var inputSwitch = new Switch((uint) stateNames.Count);
				inputSwitch.Name = configuration.Get<string>(prefix + ".name");
				inputSwitch.Description = configuration.Get<string>(prefix + ".description");
				for(int i = 0; i < stateNames.Count; i++) {
					inputSwitch.SetStateName((uint) i, stateNames[i]);
					inputSwitch.SetStateDescription((uint) i, stateDescriptions[i]);
				}
				inputSwitch.State = 0;
				spec.switches[inputSwitch.Name] = inputSwitch;
			}

			var analogKeys = configuration.Get<List<string>>("analogs.__keys");
			foreach(var k in analogKeys) {
				var p = "analogs." + k;
				var analog = new Analog();
				analog.Name = configuration.Get<string>(p + ".name");
				analog.Description = configuration.Get<string>(p + ".description");
				spec.analogs[analog.Name] = analog;
			}

			var sixdofKeys = configuration.Get<List<string>>("sixdofs.__keys");
			foreach(var k in sixdofKeys) {
				var p = "sixdofs." + k;
				var sixdof = new SixDOF();
				sixdof.Name = configuration.Get<string>(p + ".name");
				sixdof.Description = configuration.Get<string>(p + ".description");
				spec.sixdofs[sixdof.Name] = sixdof;
			}

			return spec;
		}
	}
}

