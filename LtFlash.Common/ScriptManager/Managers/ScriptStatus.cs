using Rage;
using System;
using System.Collections.Generic;

namespace LtFlash.Common.ScriptManager.Managers
{
    public class ScriptStatus
    {
        public string Id { get; private set; }
        public Type TypeImplIScript { get; private set; }
        public Scripts.IScript Script { get; private set; }
        public bool Processed { get; set; } = false;

        public double TimerIntervalMax { get; set; }
        public double TimerIntervalMin { get; set; }

        public string[] NextScriptToRunIds { get; private set; }
        public List<string[]> ScriptsToFinishPriorThis { get; set; }

        public Scripts.EInitModels InitModel { get; set; }

        public bool HasFinishedSuccessfully
        {
            get
            {
                return Script != null && Script.Completed;
            }
        }

        public bool HasFinishedUnsuccessfully
        {
            get
            {
                return Script != null && Script.HasFinished && !Script.Completed;

            }
        }

        public bool IsRunning
        {
            get
            {
                return Script != null && Script.IsRunning;
            }
        }

        public bool Start()
        {
            if (Script.CanBeStarted())
            {
                Script.Start();
                return true;
            }
            else return false;
        }

        private Scripts.IScript CreateInstance(Type type)
        {
            return (Scripts.IScript)Activator.CreateInstance(type);
        }

        //NOTE: to use with simple SM
        public ScriptStatus(
            string id, Type typeOfBaseScript, 
            string nextScriptToRunId = "")
        {
            Id = id;
            TypeImplIScript = typeOfBaseScript;
            NextScriptToRunIds = new string[] { nextScriptToRunId };
        }

        public ScriptStatus(
            string id, Type typeOfBaseScript, Scripts.EInitModels initModel,
            string[] nextScriptToRunId, List<string[]> scriptsToFinishPrior,
            double timerMin, double timerMax)
        {
            Id = id;
            TypeImplIScript = typeOfBaseScript;
            NextScriptToRunIds = nextScriptToRunId;
            ScriptsToFinishPriorThis = scriptsToFinishPrior;
            InitModel = initModel;

            TimerIntervalMin = timerMin;
            TimerIntervalMax = timerMax;

            Script = CreateInstance(TypeImplIScript);
        }
    }
}
