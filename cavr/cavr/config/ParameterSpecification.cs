using System;
using System.Collections.Generic;

using NLog;

using cavr.util;

namespace cavr.config
{
	public class ParameterSpecification
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

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

		public virtual bool SetDefault<T>(T val) {
			WarnDefaultMessage<T>();
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

		public virtual ParameterSpecification Copy() {
			return new ParameterSpecification(Type, Name, Required);
		}

		public virtual bool Configure(Configuration config, LuaReader reader, string b) {
			return false;
		}

		private void WarnDefaultMessage<T>() {
			log.Warn("{0} is not a {1}.", Name, typeof(T).Name);
		}
	}

	public class Parameter<T> : ParameterSpecification
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		private T defaultValue;

		public Parameter(string name, bool required) : base(ParameterTraits<T>.Type, name, required)
		{
		}

		public bool SetDefault(T val) {
			defaultValue = val;
			return true;
		}

		public virtual bool GetDefault(out T val) {
			val = defaultValue;
			return true;
		}

		public override ParameterSpecification Copy() {
			var result = new Parameter<T>(Name, Required);
			result.SetDefault(defaultValue);
			return result;
		}

		public override bool Configure(Configuration config, LuaReader reader, string b) {
			var path = b + "." + Name;
			T val = default(T);

			var specified = reader.Get(path, ref val);

			if(!specified && Required) {
				log.Error("{0} must be specified", path);
				return false;
			}

			if(!specified) {
				log.Info("{0} was not specified. Using default value of {1}", path, defaultValue);
				val = defaultValue;
			}

			config.Add(path, val);
			return true;
		}
	}

	public class ConfigurationListParameter : ParameterSpecification
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		private ConfigurationSpecification spec;

		public ConfigurationListParameter(string name, bool required, ConfigurationSpecification specification) : base(ParameterType.kConfigurationList, name, required) {
			spec = new ConfigurationSpecification(specification);
		}

		public override ParameterSpecification Copy() {
			return new ConfigurationListParameter(Name, Required, spec);
		}

		public override bool Configure(Configuration config, LuaReader reader, string b) {
			var path = b + "." + Name;
			var keys = new List<string>();
			if(!reader.GetKeys(path, ref keys)) {
				log.Error("{0} must be a table/list", path);
				return false;
			}

			foreach(var key in keys) {
				if(!spec.Configure(reader, string.Format("{0}.{1}", path, key), config)) {
					log.Error("{0}.{1} is not valid", path, key);
					return false;
				}
			}

			config.Add(path + ".__keys", keys);
			return true;
		}
	}

	public class OneOfParameter : ParameterSpecification
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		private Dictionary<string, ConfigurationSpecification> choices;

		public OneOfParameter(string name, bool required, Dictionary<string, ConfigurationSpecification> pChoices) : base(ParameterType.kOneOf, name, required) {
			choices = new Dictionary<string, ConfigurationSpecification>(pChoices);
		}

		public override ParameterSpecification Copy() {
			return new OneOfParameter(Name, Required, choices);
		}

		public override bool Configure(Configuration config, LuaReader reader, string b) {
			var path = b + "." + Name;
			var configured = false;
			var matchedChoices = new List<string>();
			foreach(var pair in choices) {
				var choiceConfig = new Configuration();
				if(pair.Value.Configure(reader, path, config)) {
					matchedChoices.Add(pair.Key);
					if(!configured) {
						config.Absorb(choiceConfig);
						configured = true;
					}
				}
			}

			if(matchedChoices.Count > 1) {
				log.Error("Configuration of {0} matched multiple choices:", path);

				foreach(var choice in matchedChoices) {
					log.Error("    {0}", choice);
				}
				return false;
			}

			if(!configured) {
				log.Error("Configuration of {0} matches no possibilities", path);
				return false;
			}

			config.Add(path, matchedChoices[0]);
			return true;
		}
	}

	public class MarkerParameter : ParameterSpecification
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		public MarkerParameter(string name, bool required) : base(ParameterType.kMarker, name, required) {
		}

		public override ParameterSpecification Copy() {
			return new MarkerParameter(Name, Required);
		}

		public override bool Configure(Configuration config, LuaReader reader, string b) {
			var path = b + "." + Name;
			input.Marker marker = null;
			var specified = reader.Get(path, ref marker);
			if(!specified) {
				log.Error("{0} must be specified", path);
				return false;
			}

			config.Add(path, marker);
			return true;
		}
	}
}

