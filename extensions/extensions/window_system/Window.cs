using System;

using cavr;
using cavr.config;

namespace extensions.window_system
{
	public class Window : Plugin
	{
		public Window()
		{
			Console.WriteLine("Created!");
		}

		public bool Init(Configuration config) {
			Console.WriteLine("Init!");
			return false;
		}

		public bool Step() {
			Console.WriteLine("Step!");
			return false;
		}

		public PluginGeneratorBase Load() {
			Console.WriteLine("Load!");
			return null;
		}

		public static PluginGeneratorBase Test() {
			Console.WriteLine("Test!");
			return new PluginGenerator<Window>("Window", null);
		}
	}
}

