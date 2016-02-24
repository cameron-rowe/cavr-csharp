using System;
using System.Collections.Generic;

using SysFile = System.IO.File;

using NLog;

namespace cavr.util
{
    public static class File
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        public static bool LoadIntoString(string path, out string buffer) {
            if(!SysFile.Exists(path)) {
                log.Error("Failed to open file {0}", path);
                buffer = string.Empty;
                return false;
            }

            buffer = SysFile.ReadAllText(path);
            return true;
        }

        public static bool Find(string name, List<string> searchPaths, out string resultPath) {
            foreach(var path in searchPaths) {
                var p = string.Format("{0}/{1}", path, name);
                if(SysFile.Exists(p)) {
                    resultPath = p;
                    return true;
                }
            }

            resultPath = string.Empty;
            return false;
        }

        public static bool Exists(string path) {
            return SysFile.Exists(path);
        }
    }
}

