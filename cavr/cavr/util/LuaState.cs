using System;
using System.Collections.Generic;

using NLog;

using NLua;
using NLua.Exceptions;

namespace cavr.util
{
    public class LuaState
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        public KopiLua.LuaState State { get; private set; }
        public int StackDepth { get; private set; }

        public LuaState()
        {
            State = new KopiLua.LuaState();
            LuaLib.LuaLOpenLibs(State);
            StackDepth = 0;
        }

        public bool LoadBuffer(string buffer) {
            if(LuaLib.LuaLLoadBuffer(State, buffer, "") != 0) {
                log.Error("Could not execute lua buffer in LoadBuffer");
                log.Error(LuaLib.LuaToString(State, -1));
                return false;
            }

            if(LuaLib.LuaPCall(State, 0, 0, 0) != 0) {
                log.Error("Could not execute lua buffer in LoadBuffer");
                log.Error(LuaLib.LuaToString(State, -1));
                return false;
            }

            return true;
        }

        public bool LoadFile(string path) {
            var index = path.LastIndexOf('/');

            var pathToFile = (index > 0) ? path.Substring(0, index-1) : (index == 0) ? "/" : path;
            var importFunction = string.Format("import = function(s) dofile(\"{0}/\" .. s) end", pathToFile);

            if(!LoadBuffer(importFunction)) {
                log.Error("Failed to load import function");
                return false;
            }

            var hostname = Environment.MachineName;
            var hostnameSetting = string.Format("HOSTNAME = \"{0}\";", hostname);

            if(!LoadBuffer(hostnameSetting)) {
                log.Error("Failed to set hostname");
                return false;
            }

            if(LuaLib.LuaLLoadFile(State, path) != 0) {
                log.Error("Could not load lua file");
                log.Error(LuaLib.LuaToString(State, -1));
                return false;
            }

            if(LuaLib.LuaPCall(State, 0, 0, 0) != 0) {
                log.Error("Could not execute Lua file");
                log.Error(LuaLib.LuaToString(State, -1));
                return false;
            }

            return true;
        }

        public bool PushTable(string name) {
            if(!PushValue(name)) {
                return false;
            }

            if(LuaLib.LuaType(State, -1) != LuaTypes.Table) {
                PopValue(name);
                return false;
            }

            return true;
        }

        public bool PopTable(string name) {
            return PopValue(name);
        }

        public bool PushValue(string name) {
            if(StackDepth == 0) {
                LuaLib.LuaGetGlobal(State, name);
            }
            else {
                LuaLib.LuaPushString(State, name);
                LuaLib.LuaGetTable(State, -2);
            }

            if(!LuaLib.LuaIsNil(State, -1)) {
                StackDepth++;
                return true;
            }

            LuaLib.LuaPop(State, 1);
            if(StackDepth > 0 && name != string.Empty && !name.IsInt()) {
                int val = int.Parse(name);
                LuaLib.LuaPushNumber(State, val);
                LuaLib.LuaGetTable(State, -2);
                if(!LuaLib.LuaIsNil(State, -1)) {
                    StackDepth++;
                    return true;
                }
                else {
                    LuaLib.LuaPop(State, 1);
                }
            }

            return false;
        }

        public bool PopValue(string key) {
            LuaLib.LuaPop(State, 1);
            StackDepth--;
            return true;
        }

        public void Reset() {
            LuaLib.LuaPop(State, StackDepth);
            StackDepth = 0;
        }

        public bool ReadValue<T>(ref T val) {
            if(!LuaLib.LuaIsNumber(State, -1)) {
                log.Error("Value is not a number");
                return false;
            }

            val = (T)Convert.ChangeType(LuaLib.LuaToNumber(State, -1), typeof(T));

            return true;
        }

        public bool ReadValue(ref string val) {
            if(!LuaLib.LuaIsString(State, -1)) {
                log.Error("Value is not a string");
                return false;
            }

            val = LuaLib.LuaToString(State, -1);
            return true;
        }

        public bool ReadValue(ref bool val) {
            if(LuaLib.LuaIsNumber(State, -1)) {
                val = Convert.ToBoolean(LuaLib.LuaToNumber(State, -1));
            }
            else if(LuaLib.LuaIsBoolean(State, -1)) {
                val = LuaLib.LuaToBoolean(State, -1);
            }
            else {
                log.Error("Value is not a number nor a bool");
                return false;
            }

            return true;
        }

        public bool ReadValue(ref config.Transform val) {
            var transform = LuaLib.LuaToUserData(State, -1);

            if(transform != null) {
                log.Info(transform);
                return true;
            }

            log.Error("Failed to convert to transform");
            return true;
        }

        public bool ReadValue(ref List<string> val) {
            LuaLib.LuaPushNil(State);
            while(LuaLib.LuaNext(State, -2) != 0) {
                val.Add(LuaLib.LuaToString(State, -1));
                LuaLib.LuaPop(State, 1);
            }

            return true;
        }

        public bool ReadKeys(ref List<string> keys) {
            LuaLib.LuaPushNil(State);
            while(LuaLib.LuaNext(State, -2) != 0) {
                LuaLib.LuaPushValue(State, -2);
                keys.Add(LuaLib.LuaToString(State, -1));
                LuaLib.LuaPop(State, 1);
                LuaLib.LuaPop(State, 1);
            }

            return true;
        }

        public bool GetValue<T>(string key, ref T val) {
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

    public static class UtilStringExtensions
    {
        public static bool IsInt(this string val) {
            int ingore;
            return int.TryParse(val, out ingore);
        }
    }
}

