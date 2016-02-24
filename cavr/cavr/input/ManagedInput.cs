using System;
using System.Collections.Generic;

using NLog;

namespace cavr.input
{
    public class ManagedInput<T> where T : Input, new()
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		private Dictionary<string, T> byAlias;
		private Dictionary<string, T> byDeviceName;

		public ManagedInput()
		{
			byAlias = new Dictionary<string, T>();
			byDeviceName = new Dictionary<string, T>();
		}

		public ManagedInput<T> Call() {
			return this;
		}

		public T ByAlias(string name) {
			T obj;
			var found = byAlias.TryGetValue(name, out obj);
			if(!found) {
				var dummy = default(T);
				log.Warn("Could not find {0} aliased as {1}", dummy.GetType().Name, name);
				log.Warn("Returning dummy instead");
				return dummy;
			}

			return obj;
		}

		public T ByDeviceNameOrNull(string name) {
			T obj;
			var found = byDeviceName.TryGetValue(name, out obj);
			if(!found) {
				return null;
			}

			return obj;
		}

		public T ByDeviceName(string name) {
			var result = ByDeviceNameOrNull(name);
			if(result == null) {
				var dummy = default(T);
				log.Warn("Could not find {0} with device name {1}", dummy.GetType().Name, name);
				log.Warn("Returning dummy instead");
				return dummy;
			}

			return result;
		}

		public T ByDeviceNameOrCreate(string name) {
			T obj;
			var found = byDeviceName.TryGetValue(name, out obj);
			if(found) {
				return obj;
			}

            obj = new T();
			obj.Name = name;
			byDeviceName[name] = obj;

			return obj;
		}

		public bool Reset() {
			byAlias.Clear();
			byDeviceName.Clear();
			return true;
		}

		public bool Map(string alias, string deviceName) {
			T device = ByDeviceNameOrCreate(deviceName);
			if(byAlias.ContainsKey(alias)) {
				log.Error("{0} alias {1} is already mapped", device.GetType().Name, alias);
				return false;
			}
			byAlias[alias] = device;
			return true;
		}

		public List<string> GetDeviceNames() {
			var results = new List<string>();
			foreach(var pair in byDeviceName) {
				results.Add(pair.Key);
			}
			return results;
		}
	}
}

