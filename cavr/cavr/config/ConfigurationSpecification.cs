using System;
using System.Collections.Generic;

using NLog;

using cavr.util;

namespace cavr.config
{
    public class ConfigurationSpecification : ICopyable<ConfigurationSpecification>
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        private Dictionary<string, ParameterSpecification> parameters;

        public ConfigurationSpecification() {
            parameters = new Dictionary<string, ParameterSpecification>();
        }

        public ConfigurationSpecification(ConfigurationSpecification other) : this() {
            foreach(var pair in other.parameters) {
                parameters.Add(pair.Key, pair.Value.Copy());
            }
        }

        public bool AddParameter(ParameterSpecification parameter) {
            var name = parameter.Name;
            if(parameters.ContainsKey(name)) {
                log.Warn("Parameter {0} is already contained in this configuration specification");
                return false;
            }
            parameters.Add(name, parameter.Copy());
            return true;
        }

        public bool Configure(LuaReader reader, string name, Configuration config) {
            var result = true;
            foreach(var pair in parameters) {
                if(!pair.Value.Configure(config, reader, name)) {
                    log.Info("Failed to configure {0}. {1}", name, pair.Key);
                    result = false;
                }
            }

            return result;
        }

        public ConfigurationSpecification Copy() {
            return new ConfigurationSpecification(this);
        }

        public Dictionary<string, ParameterSpecification> GetMap() {
            return parameters;
        }

        public static bool SetDefault<T>(LuaReader reader, string prefix, ParameterSpecification parameter) {
            T defaultValue = default(T);
            if(!reader.Get(prefix + ".default", ref defaultValue)) {
                log.Error("Default must be specified for {0}", prefix);
                return false;
            }
            parameter.SetDefault(defaultValue);
            return true;
        }

        public static ConfigurationSpecification CreateFromLuaFile(string path, string name) {
            var reader = LuaReader.CreateFromFile(path);
            if(reader == null) {
                log.Error("Failed to load lua file in ConfigurationSpecification : {0}", path);
                return null;
            }

            return ConfigurationSpecification.CreateFromLuaReader(reader, name);
        }

        public static ConfigurationSpecification CreateFromLuaBuffer(string buffer, string name) {
            var reader = LuaReader.CreateFromBuffer(buffer);
            if(reader == null) {
                log.Error("Failed to load lua buffer in ConfigurationSpecification : {0}", name);
                return null;
            }

            return ConfigurationSpecification.CreateFromLuaReader(reader, name);
        }

        public static ConfigurationSpecification CreateFromLuaReader(LuaReader reader, string name) {
            var parameterNames = new List<string>();
            if(!reader.GetKeys(name, ref parameterNames)) {
                log.Warn("Empty specification: {0}", name);
                return new ConfigurationSpecification();
            }

            var result = true;
            var specification = new ConfigurationSpecification();
            foreach(var parameterName in parameterNames) {
                var prefix = name + "." + parameterName;
                var isRequired = false;
                if(!reader.Get(prefix + ".required", ref isRequired)) {
                    log.Error("Required must be specified for {0}", parameterName);
                    result = false;
                    continue;
                }

                var typeName = string.Empty;
                if(!reader.Get(prefix + ".type", ref typeName)) {
                    log.Error("Type must be specified for {0}", parameterName);
                    result = false;
                    continue;
                }

                ParameterSpecification parameter = null;
                switch(typeName) {
                    case "boolean":
                        parameter = new Parameter<bool>(parameterName, isRequired);
                        result &= isRequired || SetDefault<bool>(reader, prefix, parameter);
                    break;

                    case "number":
                        parameter = new Parameter<double>(parameterName, isRequired);
                        result &= isRequired || SetDefault<double>(reader, prefix, parameter);
                    break;

                    case "string":
                        parameter = new Parameter<string>(parameterName, isRequired);
                        result &= isRequired || SetDefault<string>(reader, prefix, parameter);
                    break;

                    case "transform":
                        parameter = new Parameter<Transform>(parameterName, isRequired);
                        result &= isRequired || SetDefault<Transform>(reader, prefix, parameter);
                    break;

                    case "string_list":
                        parameter = new Parameter<List<string>>(parameterName, isRequired);
                        result &= isRequired || SetDefault<List<string>>(reader, prefix, parameter);
                    break;

                    case "marker":
                        parameter = new MarkerParameter(parameterName, isRequired);
                    break;

                    case "list":
                        var subspec = CreateFromLuaReader(reader, prefix + ".subtype");
                        if(subspec != null) {
                            parameter = new ConfigurationListParameter(parameterName, isRequired, subspec);
                        }
                        else {
                            log.Error("Failed to read subtype for list of {0}", parameterName);
                            result = false;
                        }
                    break;

                    case "one_of":
                        var typeKeys = new List<string>();
                        if(reader.GetKeys(prefix + ".possibilities", ref typeKeys)) {
                            var choices = new Dictionary<string, ConfigurationSpecification>();
                            foreach(var type in typeKeys) {
                                subspec = CreateFromLuaReader(reader, prefix + ".possibilities." + type);
                                if(subspec != null) {
                                    choices[type] = subspec;
                                }
                                else {
                                    log.Error("Failed to read subconfiguration");
                                    result = false;
                                }
                            }
                            //parameter = new
                        }
                    break;

                    default:
                        log.Error("Parameter type unknown");
                        result = false;
                    break;
                }

                if(parameter != null && !specification.AddParameter(parameter)) {
                    log.Error("Failed to add parameter {0}", parameterName);
                    result = false;
                }
            }

            if(!result) {
                log.Error("One or more errors encoutered in spec for {0}", name);
                return null;
            }

            return specification;
        }

        public static ConfigurationSpecification CreateFromSchema(string path, string name) {
            var paths = Paths.GetSchemaPaths();

            string schemaPath;
            if(!File.Find(path, paths, out schemaPath)) {
                log.Error("Could not find schema file {0}", path);
                return null;
            }

            return CreateFromLuaFile(schemaPath, name);
        }
    }
}

