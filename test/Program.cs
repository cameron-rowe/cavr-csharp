using System;
using System.Reflection;

using cavr;
using cavr.math;

namespace test
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			cavr.System.InitLogging();
			PluginUtils.LoadPlugin("/Users/cam/Desktop/test/csharp/cavr/extensions/bin/Debug/extensions.dll");
		}
	}
}
