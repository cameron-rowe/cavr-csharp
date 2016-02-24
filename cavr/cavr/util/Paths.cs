using System;
using System.Collections.Generic;

namespace cavr.util
{
	public static class Paths
	{
		public static List<string> GetConfigPaths() {
			var paths = new List<string> {"."};

            paths.Add("/Users/cam/Desktop/test/csharp/cavr/test");

			var cavrPaths = GetCavrPaths();

			paths.AddRange(cavrPaths);
			paths.Add("/usr/local/etc/cavr");

			return paths;
		}

		public static List<string> GetSchemaPaths() {
			var paths = new List<string> { "/usr/local/etc/cavr/schema" };

			return paths;
		}

		public static List<string> GetCavrPaths() {
			var paths = new List<string>();
			var envPath = Environment.GetEnvironmentVariable("CAVR_PATH");
			if(envPath != null) {
				paths.AddRange(envPath.Split(':'));
			}

			return paths;
		}
	}
}

