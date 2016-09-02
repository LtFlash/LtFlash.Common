using LtFlash.Common.Processes;
using LtFlash.Common.ScriptManager.Managers;
using LtFlash.Common.ScriptManager.Scripts;
using System.Collections.Generic;

namespace LtFlash.Common.ScriptManager.ScriptStarters
{
    internal abstract class ScriptStarterBase : IScriptStarter
    {
        //PUBLIC
        public bool HasFinishedSuccessfully => Script.HasFinishedSuccessfully;

        public bool HasFinishedUnsuccessfully
        {
            get { return _finishedUnsuccessfully || Script.HasFinishedUnsuccessfully; }
            protected set { _finishedUnsuccessfully = value; }
        }

        public string Id => Script.Status.Id;

        public List<string> NextScriptsToRun => Script.Status.NextScripts;

        public IScript Script { get; private set; }
        //PROTECTED
        protected bool StartScriptInThisTick { get; set; }
        protected bool ScriptStarted { get; private set; }
        protected bool AutoRestart { get; private set; }
        protected ProcessHost Stages { get; private set; } 
            = new ProcessHost();

        //PRIVATE
        private bool _finishedUnsuccessfully;

        public ScriptStarterBase(IScript scriptStatus, bool autoRestart)
        {
            Script = scriptStatus;

            AutoRestart = autoRestart;

            Stages.AddProcess(InternalProcess);
            Stages.ActivateProcess(InternalProcess);
            Stages.Start();
        }

        public abstract void Start();
        public abstract void Stop();

        private void InternalProcess()
        {
            if(StartScriptInThisTick/* && ss.IsRunning*/)
            {
                ScriptStarted = Script.Start();
                StartScriptInThisTick = false;
            }
        }
    }
}
