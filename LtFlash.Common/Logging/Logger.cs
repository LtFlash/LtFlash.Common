using Rage;

namespace LtFlash.Common.Logging
{
    internal static class Logger
    {
        //TODO: instance + customizable output?
        private static bool LOG = true;
        private static bool DEBUG = true;

        public static void LogDebug(string className, string function, string msg)
        {
            if (!DEBUG) return;
            Log(className, function, msg);
        }

        public static void Log(string className, string function, string msg)
            => Log($"{className}.{function}: {msg}");

        public static void Log(string msg)
        {
            if (!LOG) return;
            Game.LogVerbose(msg);
        }
    }
}
