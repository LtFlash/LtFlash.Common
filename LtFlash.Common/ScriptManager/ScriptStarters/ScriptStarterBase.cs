using LtFlash.Common.Processes;
using LtFlash.Common.ScriptManager.Managers;

namespace LtFlash.Common.ScriptManager.ScriptStarters
{
    internal abstract class ScriptStarterBase : IScriptStarter
    {
        //PUBLIC
        public bool HasFinishedSuccessfully => script.HasFinishedSuccessfully;

        public bool HasFinishedUnsuccessfully
        {
            get { return _finishedUnsuccessfully || script.HasFinishedUnsuccessfully; }
            protected set { _finishedUnsuccessfully = value; }
        }

        public string Id => script.Id;

        public string[] NextScriptsToRun => script.NextScriptToRunIds;

        //PROTECTED
        protected ScriptStatus script;
        protected bool StartScriptInThisTick { get; set; }
        protected bool ScriptStarted { get; private set; }
        protected bool AutoRestart { get; private set; }
        protected ProcessHost Stages { get; private set; } 
            = new ProcessHost();

        //PRIVATE
        private bool _finishedUnsuccessfully;

        public ScriptStarterBase(ScriptStatus scriptStatus, bool autoRestart)
        {
            script = scriptStatus;

            AutoRestart = autoRestart;

            Stages.AddProcess(InternalProcess);
            Stages.ActivateProcess(InternalProcess);
            Stages.Start();
        }

        public abstract void Start();

        public ScriptStatus GetScriptStatus()
        {
            return script;
        }

        private void InternalProcess()
        {
            if(StartScriptInThisTick/* && ss.IsRunning*/)
            {
                ScriptStarted = script.Start();
                StartScriptInThisTick = false;
            }
        }
    }
}
