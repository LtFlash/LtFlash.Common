namespace LtFlash.Common.ScriptManager.Managers
{
    public class SequentialScriptManager : NewScriptManagerBase
    {
        public SequentialScriptManager() : base()
        {
            _proc.AddProcess(StartNewScript);
            _proc.ActivateProcess(StartNewScript);
        }

        private void StartNewScript()
        {
            if (!canStartNewScript) return;
            StartFromFirstScript();
            canStartNewScript = false;
        }
    }
}
