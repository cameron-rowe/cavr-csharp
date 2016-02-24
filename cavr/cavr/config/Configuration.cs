using System;
using System.Collections.Generic;

namespace cavr.config
{
    public class Configuration
    {
        private Stack<string> accessPrefixes;
        private Dictionary<string, ValueBase> values;

        public Configuration()
        {
            accessPrefixes = new Stack<string>();
            values = new Dictionary<string, ValueBase>();
        }

        public void PushPrefix(string prefix) {
            var oldPrefix = GetPrefix();
            if(oldPrefix == string.Empty) {
                accessPrefixes.Push(prefix);
            }
            else {
                accessPrefixes.Push(oldPrefix + prefix);
            }
        }

        public string GetPrefix() {
            if(accessPrefixes.Count > 0) {
                return accessPrefixes.Peek();
            }

            return string.Empty;
        }

        public void PopPrefix(string prefix = "") {
            accessPrefixes.Pop();
        }

        public bool Add<T>(string name, T val) {
            if(name != string.Empty && name[0] == '.') {
                values[name.Substring(1)] = new Value<T> { value = val };
            }
            else {
                values[name] = new Value<T> { value = val };
            }

            return true;
        }

        public T Get<T>(string name) {
            ValueBase v;
            var found = values.TryGetValue(GetPrefix() + name, out v);
            if(!found) {
                Console.Error.WriteLine("Configuration does not contain: " + GetPrefix() + name);
                return default(T);
            }

            return ((Value<T>) v).value;
        }

        public bool Absorb(Configuration config) {
            foreach(var pair in config.values) {
                if(values.ContainsKey(pair.Key)) {
                    Console.Error.WriteLine("Configuration: {0} already exists.", pair.Key);
                    return false;
                }
            }

            foreach(var pair in config.values) {
                values[pair.Key] = pair.Value;
            }

            config.values.Clear();
            return true;
        }

        internal interface ValueBase
        {
        }

        internal struct Value<T> : ValueBase
        {
            public T value;
        }
    }
}

