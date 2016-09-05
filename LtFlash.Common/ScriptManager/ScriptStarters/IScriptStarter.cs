using LtFlash.Common.ScriptManager.Scripts;
using System.Collections.Generic;

namespace LtFlash.Common.ScriptManager.ScriptStarters
{
    interface IScriptStarter
    {
        bool HasFinishedSuccessfully { get; }
        bool HasFinishedUnsuccessfully { get; }
        string Id { get; }
        List<string> NextScriptsToRun { get; }
        void Start();
        void Stop();
        IScript Script { get; }
    }
}
