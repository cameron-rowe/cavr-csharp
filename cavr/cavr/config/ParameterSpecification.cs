using System;
using System.Collections.Generic;

namespace cavr.config
{
	public class ParameterSpecification
	{
		public ParameterType Type { get; private set; }
		public string Name { get; private set; }
		public bool Required { get; private set; }

		public ParameterSpecification(ParameterType type, string name, bool required) {
			Type = type;
			Name = name;
			Required = required;
		}

		public bool SetDefault(bool val) {
			WarnDefaultMessage<bool>();
			return false;
		}
		public bool SetDefault(double val) {
			WarnDefaultMessage<double>();
			return false;
		}

		public bool SetDefault(string val) {
			WarnDefaultMessage<string>();
			return false;
		}

		public bool SetDefault(Transform val) {
			WarnDefaultMessage<Transform>();
			return false;
		}

		public bool SetDefault(List<string> val) {
			WarnDefaultMessage<List<string>>();
			return false;
		}

		public bool GetDefault(ref bool val) {
			WarnDefaultMessage<bool>();
			return false;
		}

		public bool GetDefault(ref double val)
		{
			WarnDefaultMessage<double>();
			return false;
		}

		public bool GetDefault(ref string val)
		{
			WarnDefaultMessage<string>();
			return false;
		}

		public bool GetDefault(ref Transform val) {
			WarnDefaultMessage<Transform>();
			return false;
		}

		public bool GetDefault(ref List<string> val) {
			WarnDefaultMessage<List<string>>();
			return false;
		}

		private void WarnDefaultMessage<T>() {
			Console.Error.WriteLine("{0} is not a {1}.", Name, typeof(T).Name);
		}
	}

	public class Parameter<T> : ParameterSpecification
	{
		private T defaultValue;

		public Parameter(string name, bool required) : base(ParameterTraits<T>.Type, name, required)
		{
		}

		public virtual bool SetDefault(T val) {
			defaultValue = val;
			return true;
		}

		public virtual bool GetDefault(out T val) {
			val = defaultValue;
			return true;
		}

		public virtual ParameterSpecification Copy() {
			var result = new Parameter<T>(Name, Required);
			result.SetDefault(defaultValue);
			return result;
		}

		public virtual bool Configure(Configuration config, LuaReader reader, string b) {
			var path = b + Name;
			T val;
			//var specified = reader.Get(path, val);
			return false;
		}
	}
}

