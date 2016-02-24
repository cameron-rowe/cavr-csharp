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
		//private Dictionary<NativeWindow, Window> windowMap;
		private Dictionary<Key, Button> buttonByKey;
        private List<Window> windows;

		private FunctionCallback updateContextCallback;
		private FunctionCallback destructContextCallback;

		public Gl()
		{
			display = null;
			monoContext = null;
			stereoContext = null;

			stereoWindows = new List<Window>();
			monoWindows = new List<Window>();
            windows = new List<Window>();
			//windowMap = new Dictionary<NativeWindow, Window>();
			buttonByKey = new Dictionary<Key, Button>();
		}

		public virtual bool Init(cavr.config.Configuration config) {
			displayName = config.Get<string>("display");
			inputName = config.Get<string>("input_name");
			updateContextCallback = cavr.System.GetCallback(config.Get<string>("update_callback"));
			destructContextCallback = cavr.System.GetCallback(config.Get<string>("destruct_callback"));

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

            var stereoConfig = stereoWindows.Count > 0 ? new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 8, 0, 0, 2, true) : null;
            var monoConfig = monoWindows.Count > 0 ? new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 8, 0, 0, 2, false) : null;

            stereoContext = monoContext = null;
			var result = true;
			foreach(var window in stereoWindows) {
                result &= window.Open(display, stereoConfig, ref stereoContext);
                windows.Add(window);
			}

            foreach(var window in monoWindows) {
                result &= window.Open(display, monoConfig, ref monoContext);
                windows.Add(window);
            }

            if(!result) {
                log.Error("Failed to open all requested windows");
                return false;
            }

            if(monoWindows.Count > 0) {
                monoWindows[0].MakeCurrent();
                cavr.System.SetContextData(null);
                cavr.System.GetCallback(config.Get<string>("init_callback"))();
                monoContextData = cavr.System.GetContextData();
                monoContext.MakeCurrent(null);
            }

            if(stereoWindows.Count > 0) {
                stereoWindows[0].MakeCurrent();
                cavr.System.SetContextData(null);
                cavr.System.GetCallback(config.Get<string>("init_callback"))();
                stereoContextData = cavr.System.GetContextData();
                stereoContext.MakeCurrent(null);
            }

            foreach(var window in windows) {
                window.MakeCurrent();
                if(!window.SetupRenderData()) {
                    log.Error("Failed to setup render data");
                    result = false;
                }
            }

            if(monoContext != null) {
                monoContext.MakeCurrent(null);
            }

            if(stereoContext != null) {
                stereoContext.MakeCurrent(null);
            }

            var analogs = new List<Analog> {
                InputManager.GetAnalog.ByDeviceNameOrNull(inputName + "[analog[x0]]"),
                InputManager.GetAnalog.ByDeviceNameOrNull(inputName + "[analog[y0]]"),
                InputManager.GetAnalog.ByDeviceNameOrNull(inputName + "[analog[x1]]"),
                InputManager.GetAnalog.ByDeviceNameOrNull(inputName + "[analog[y1]]"),
                InputManager.GetAnalog.ByDeviceNameOrNull(inputName + "[analog[x2]]"),
                InputManager.GetAnalog.ByDeviceNameOrNull(inputName + "[analog[y2]]")
            };

            foreach(var window in windows) {
                window.SetAnalogs(analogs.ToArray());
            }

			return result;
		}

		public virtual bool Step() {
            if(monoWindows.Count > 0) {
                monoWindows[0].MakeCurrent();
                cavr.System.SetContextData(null);
                updateContextCallback();
                monoContextData = cavr.System.GetContextData();
                monoContext.MakeCurrent(null);
            }

            if(stereoWindows.Count > 0) {
                stereoWindows[0].MakeCurrent();
                cavr.System.SetContextData(null);
                updateContextCallback();
                stereoContextData = cavr.System.GetContextData();
                stereoContext.MakeCurrent(null);
            }

            foreach(var window in windows) {
                window.Update();
            }

            ProcessEvents();

			return true;
		}

		public void ProcessEvents() {
            foreach(var window in windows) {
                window.ProcessEvents();
            }
		}

		public static PluginGeneratorBase Load() {
			return null;
		}

		private static bool GetFramebufferConfig(DisplayDevice display, bool b, ref GraphicsMode config) {
			return false;
		}
	}
}

