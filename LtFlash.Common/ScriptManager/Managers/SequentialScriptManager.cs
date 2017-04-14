namespace LtFlash.Common.ScriptManager.Managers
{
    public class SequentialScriptManager : ScriptManagerBase
    {
        public SequentialScriptManager() : base()
        {
            ProcHost.ActivateProcess(StartNewScript);
            RemoveScriptWhenSuccessful = true;
        }

        private void StartNewScript()
        {
            if (!canStartNewScript) return;
            StartFromFirstScript();
            canStartNewScript = false;
        }
    }
}
