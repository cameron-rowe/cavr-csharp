using System;
using System.Collections.Generic;

using NLua;
using NLua.Exceptions;

namespace cavr.util
{
	public class LuaState
	{
		public Lua State { get; private set; }
		public int StackDepth { get; private set; }

		public LuaState()
		{
			State = new Lua();
			StackDepth = 0;
		}

		public bool LoadBuffer(string buffer) {
			try {
				State.DoString(buffer);
				return true;
			}
			catch(LuaException e) {
				Console.Error.WriteLine("Could not execute lua buffer in LoadBuffer");
				Console.Error.WriteLine(e.Message);
				return false;
			}
		}

		public bool LoadFile(string path) {
			//var strs = path.Split(new char[] {'/'}, 2, StringSplitOptions.RemoveEmptyEntries);
			//var pathToFile = strs[0];
			//var fileName = strs[1];

			try {
				State.DoFile(path);

				var hostname = Environment.MachineName;
				var hostnameSetting = string.Format("HOSTNAME = \"{0}\";", hostname);

				State.DoString(hostnameSetting);

				return true;

			}
			catch(LuaException e) {
				Console.Error.WriteLine("Exception occured in LoadFile");
				Console.Error.WriteLine(e.Message);
			}

			return false;
		}

		public bool PushTable(string name) {
			return false;
		}

		public bool PopTable(string name) {
			return false;
		}

		public bool PushValue(string key) {
			return false;
		}

		public bool PopValue(string key) {
			return false;
		}

		public void Reset() {
			
		}

		public bool ReadValue<T>(ref T val) where T : struct, IComparable<T> {
			return false;
		}

		public bool ReadValue(ref string val) {
			return false;
		}

		public bool ReadValue(ref bool val) {
			return false;
		}

		public bool ReadValue(ref config.Transform val) {
			return false;
		}

		public bool ReadValue(ref List<string> val) {
			return false;
		}

		public bool ReadKeys(ref List<string> keys) {
			return false;
		}

		public bool GetValue<T>(string key, ref T val) where T : struct, IComparable<T> {
			if(!PushValue(key)) {
				return false;
			}

			if(!ReadValue(ref val)) {
				PopValue(key);
				return false;
			}

			PopValue(key);
			return true;
		}
	}
}

