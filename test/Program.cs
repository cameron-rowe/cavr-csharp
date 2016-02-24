using System;
using System.Reflection;
using System.Threading;

using NLog;
using OpenTK;
using OpenTK.Graphics;

using cavr;

namespace test
{
    class MainClass
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            var inputMap = new cavr.input.InputMap();
            inputMap.buttonMap["exit"] = "keyboard[Escape]";

            if(!cavr.System.Init(args, inputMap)) {
                log.Error("Unable to initialize cavr!");
                return;
            }

            cavr.System.Run();
        }
    }
}
