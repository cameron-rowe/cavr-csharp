using System;
using System.Collections.Generic;

using NLog;

using cavr.config;

namespace cavr.input
{
	public class InputMap
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		public Dictionary<string, string> buttonMap;
		public Dictionary<string, string> analogMap;
		public Dictionary<string, string> sixdofMap;

		private delegate bool GenFunc(Dictionary<string, string> map, string varName);

		public static InputMap GenerateFromLuaFile(string path) {
			var reader = LuaReader.CreateFromFile(path);
			if(reader == null) {
				log.Error("Could not open InputMap file: {0}", path);
				return null;
			}

			GenFunc readMap = (map, varName) => {
				var result = true;
				var keys = new List<string>();
				if(!reader.GetKeys(varName, ref keys)) {
					log.Error("{0} is not a table in file {1}", varName, path);
					return false;
				}

				foreach(var userName in keys) {
					var deviceName = string.Empty;
					if(!reader.Get(string.Format("{0}.{1}", varName, userName), ref deviceName)) {
						log.Error("{0}.{1} is not a string", varName, userName);
						result = false;
						continue;
					}

					map[userName] = deviceName;
				}

				return result;
			};

			var correct = true;
			var ret = new InputMap();
			correct &= readMap(ret.buttonMap, "button_map");
			correct &= readMap(ret.analogMap, "analog_map");
			correct &= readMap(ret.sixdofMap, "sixdof_map");

			if(!correct) {
				log.Error("Failed to parse InputMap file: {0}", path);
				return null;
			}

			return ret;
		}
	}
}

