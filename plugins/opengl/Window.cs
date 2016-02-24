using System;

using OpenTK;

namespace opengl
{
	public class Window : NativeWindow
	{
		private static NativeWindow globalBackgroundContext;

		static Window() {
			globalBackgroundContext = new NativeWindow();
		}

		public bool IsStereo { get; private set; }
		public Window()
		{
		}

		public static Window Configure(cavr.config.Configuration config, DisplayDevice display = null) {
			if(display == null) {
				display = DisplayDevice.Default;
			}

			return null;
		}
	}
}

