using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;

namespace ScriptManager.Resources
{
    internal static class Logger
    {
        private static bool LOG = true;

        public static void Log(string className, string function, string msg)
        {
            Log($"{className}.{function}: {msg}");
        }

        public static void Log(string msg)
        {
            if (!LOG) return;
            Game.LogVerbose(msg);
        }
    }
}
