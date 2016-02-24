using System;
using System.Reflection;

using NLog;

using cavr.config;

namespace cavr
{
    public interface Plugin
    {
        bool Init(Configuration config);
        bool Step();
    }

    public abstract class PluginGeneratorBase
    {
        public Assembly Handle { private get; set; }

        public abstract string Name { get; protected set; }
        public abstract ConfigurationSpecification Configuration { get; protected set; }

        public PluginGeneratorBase() {
            Handle = null;
        }

        public abstract Plugin Generate();
    }

    public class PluginGenerator<T> : PluginGeneratorBase where T : Plugin, new()
    {
        public override string Name { get; protected set; }
        public override ConfigurationSpecification Configuration { get; protected set; }

        public PluginGenerator(string name, ConfigurationSpecification spec) : base() {
            Name = name;
            Configuration = spec;
        }

        public override Plugin Generate() {
            return new T();
        }
    }

    public static class PluginUtils
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        public static PluginGeneratorBase LoadPlugin(string path) {

            try {
                var handle = Assembly.LoadFile(path);

                var types = handle.GetExportedTypes();
                foreach(var t in types) {
                    if(typeof(Plugin).IsAssignableFrom(t)) {

                        var load = t.GetMethod("Load");

                        if(load == null) {
                            throw new Exception("No static Load() Method Provided for class: " + t.Name);
                        }
                        var gen = load.Invoke(null, null) as PluginGeneratorBase;

                        if(gen == null) {
                            throw new Exception("Unable to create PluginGeneratorBase from plugin Load() function");
                        }

                        return gen;
                    }
                }
                //var plugin = Assembly

            }
            catch(Exception e) {
                log.Error("Failed to open plugin {0}", path);
                log.Error("Reason: {0}", e.Message);
                return null;
            }

            log.Error("No Plugin found in file {0}", path);
            return null;
        }
    }
}

