using System;
using System.Collections.Generic;
using System.Threading;

using NLog;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;

using cavr;
using cavr.input;

namespace opengl
{
	public delegate void FunctionCallback();

	public class Gl : Plugin
	{
		private static Logger log = LogManager.GetCurrentClassLogger();
		private static bool contextError = false;

		public static Mutex Mutex { get; private set; } = new Mutex();

		private List<Window> stereoWindows;
		private GraphicsContext stereoContext;
		private object stereoContextData;

		private List<Window> monoWindows;
		private GraphicsContext monoContext;
		private object monoContextData;

		private string displayName;
		private DisplayDevice display;

		private string inputName;
		private Dictionary<NativeWindow, Window> windowMap;
		private Dictionary<Key, Button> buttonByKey;

		private FunctionCallback updateContextCallback;
		private FunctionCallback destructContextCallback;

		public Gl()
		{
			display = null;
			monoContext = null;
			stereoContext = null;

			stereoWindows = new List<Window>();
			monoWindows = new List<Window>();
			windowMap = new Dictionary<NativeWindow, Window>();
			buttonByKey = new Dictionary<Key, Button>();
		}

		public virtual bool Init(cavr.config.Configuration config) {
			displayName = config.Get<string>("display");
			inputName = config.Get<string>("input_name");
			updateContextCallback = new FunctionCallback(cavr.System.GetCallback(config.Get<string>("update_callback")));
			destructContextCallback = new FunctionCallback(cavr.System.GetCallback(config.Get<string>("destruct_callback")));

			// TODO: Pick Display from displayName
			display = DisplayDevice.Default;

			if(display == null) {
				log.Error("Failed to open display {0}", displayName);
				return false;
			}

			var windowKeys = config.Get<List<string>>("windows.__keys");
			config.PushPrefix("windows.");
			foreach(var windowName in windowKeys) {
				config.PushPrefix(windowName + ".");
				var window = Window.Configure(config, display);
				config.PopPrefix();
				if(window != null) {
					if(window.IsStereo) {
						stereoWindows.Add(window);
					}
					else {
						monoWindows.Add(window);
					}
				}
				else {
					log.Error("Failed to configure window");
				}
			}

			config.PopPrefix();

			if(stereoWindows.Count > 0) {
				var stereoConfig = new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 8, 0, 0, 2, true);
				stereoContext = new GraphicsContext(stereoConfig, stereoWindows[0].WindowInfo);
			}

			if(monoWindows.Count > 0) {
				var monoConfig = new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 8, 0, 0, 2, false);
				monoContext = new GraphicsContext(monoConfig, monoWindows[0].WindowInfo);
			}

			var result = true;
			foreach(var window in stereoWindows) {
				window.Visible = true;
			}

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

		private static bool GetFramebufferConfig(DisplayDevice display, bool b, ref GraphicsMode config) {
			return false;
		}
	}
}

