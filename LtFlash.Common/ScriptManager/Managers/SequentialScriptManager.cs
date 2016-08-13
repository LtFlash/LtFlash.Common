namespace LtFlash.Common.ScriptManager.Managers
{
    public class SequentialScriptManager : NewScriptManagerBase
    {
        public SequentialScriptManager() : base()
        {
            ProcHost.AddProcess(StartNewScript);
            ProcHost.ActivateProcess(StartNewScript);
        }

        private void StartNewScript()
        {
            if (!canStartNewScript) return;
            StartFromFirstScript();
            canStartNewScript = false;
        }
    }
}
