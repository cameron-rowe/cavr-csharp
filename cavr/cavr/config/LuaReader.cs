using System;
using System.Collections.Generic;
using System.Linq;

using NLog;

using cavr.util;

namespace cavr.config
{
	public class LuaReader
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		private LuaState luaState;

		private LuaReader()
		{
		}

		public bool Get<T>(string path, ref T val) {
			var pathParts = path.Split('.').ToList();
			if(pathParts.Count == 0) {
				log.Error("Empty Path");
				return false;
			}

			var varName = pathParts.Last();
			pathParts.RemoveAt(pathParts.Count - 1);
			foreach(var part in pathParts) {
				if(part != string.Empty && !luaState.PushTable(part)) {
					luaState.Reset();
					return false;
				}
			}

			var result = luaState.GetValue(varName, ref val);
			luaState.Reset();
			return result;
		}

		public bool GetKeys(string path, ref List<string> keys) {
			var pathParts = path.Split('.');
			foreach(var part in pathParts) {
				if(part != string.Empty && luaState.PushTable(part)) {
					log.Error("Invalid path: {0}", path);
					luaState.Reset();
					return false;
				}
			}

			var result = luaState.ReadKeys(ref keys);
			luaState.Reset();
			return result;
		}

		public static LuaReader CreateFromBuffer(string buffer) {
			var reader = new LuaReader();
			if(!reader.luaState.LoadBuffer(buffer)) {
				return null;
			}

			return reader;
		}

		public static LuaReader CreateFromFile(string path) {
			var reader = new LuaReader();
			if(!reader.luaState.LoadFile(path)) {
				return null;
			}

			return reader;
		}
	}
}

