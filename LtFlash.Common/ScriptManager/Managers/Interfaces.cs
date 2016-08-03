using System;

namespace ScriptManager.Managers
{
    public interface IScriptManager
    {
        void AddScript(string id, Type typeImplementsIBaseScript);
        void StartScript(string id, bool checkIfCanBeStarted);
        ScriptStatus this[string id] { get; }
    }

    public interface ITimerBasedScriptManager : IScriptManager
    {
        void StartTimer();
    }
}
