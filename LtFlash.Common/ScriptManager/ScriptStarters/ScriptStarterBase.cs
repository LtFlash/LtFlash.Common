using LtFlash.Common.Processes;
using LtFlash.Common.ScriptManager.Scripts;
using System;
using System.Collections.Generic;

namespace LtFlash.Common.ScriptManager.ScriptStarters
{
    internal abstract class ScriptStarterBase : IScriptStarter
    {
        public string Id => Script.Attributes.Id;
        public bool HasFinishedSuccessfully => Script.HasFinishedSuccessfully;

        public bool HasFinishedUnsuccessfully
        {
            get { return finishedUnsuccessfully || Script.HasFinishedUnsuccessfully; }
            protected set { finishedUnsuccessfully = value; }
        }

        public List<string> NextScriptsToRun => Script.Attributes.NextScripts;

        public IScript Script { get; private set; }

        protected bool StartScriptInThisTick { get; set; }
        protected bool ScriptStarted { get; private set; }
        protected bool AutoRestart { get; private set; }
        protected ProcessHost Stages { get; private set; } 
            = new ProcessHost();

        private bool finishedUnsuccessfully;

        public ScriptStarterBase(IScript script, bool autoRestart)
        {
            Script = script;

            AutoRestart = autoRestart;

            Stages.ActivateProcess(InternalProcess);
            Stages.Start();
        }

        public abstract void Start();
        public abstract void Stop();

        private void InternalProcess()
        {
            if(StartScriptInThisTick/* && ss.IsRunning*/)
            {
                ScriptStarted = Start(Script);
                StartScriptInThisTick = false;
            }
        }

        public bool Start(IScript Script)
        {
            if (Script.HasFinished)
            {
                IScriptAttributes s = ScriptAttributes.Clone(Script.Attributes);
                Script = Managers.AdvancedScriptManager.CreateInstance<IScript>(Script.GetType(), s.CtorParams);
                Script.Attributes = s;
            }
            bool b = Script.CanBeStarted();

            if (b) Script.Start();
            return b;
        }
    }
}
