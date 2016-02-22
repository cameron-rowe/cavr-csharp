using System;

using OpenTK;

namespace opengl
{
	public class Window : GameWindow
	{
		private static NativeWindow globalBackgroundContext;

		static Window() {
			globalBackgroundContext = new NativeWindow();

		}
		public Window()
		{
		}
	}
}

