using System;

namespace LtFlash.Common.ScriptManager.Managers
{
    public interface IScriptManager
    {
        void AddScript(string id, Type typeImplementsIBaseScript);
        void StartScript(string id, bool checkIfCanBeStarted);
    }

    public interface ITimerBasedScriptManager : IScriptManager
    {
        void StartTimer();
    }
}
