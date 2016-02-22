using System;
using System.Threading;

using cavr;
using cavr.config;

namespace opengl
{
	public class Gl : Plugin
	{
		public static Mutex Mutex { get; private set; }

		public Gl()
		{
		}

		public virtual bool Init(Configuration config) {
			return false;
		}

		public virtual bool Step() {
			return false;
		}

		public void ProeccessEvents() {
			
		}

		public static PluginGeneratorBase Load() {
			return null;
		}
	}
}

