using LtFlash.Common.ScriptManager.Scripts;
using Rage;
using System;
using System.Collections.Generic;

namespace LtFlash.Common.ScriptManager.Managers
{
    public class ScriptStatus
    {
        public string Id { get; private set; }
        public Type TypeImplIScript { get; private set; }
        public IScript Script { get; private set; }
        //public bool Processed { get; set; } = false;

        public double TimerIntervalMax { get; set; }
        public double TimerIntervalMin { get; set; }

        public string[] NextScriptToRunIds { get; private set; }
        public List<string[]> ScriptsToFinishPriorThis { get; set; }

        public EInitModels InitModel { get; set; }

        public bool HasFinishedSuccessfully => Script != null && Script.Completed;

        public bool HasFinishedUnsuccessfully
            => Script != null && Script.HasFinished && !Script.Completed;


        public bool IsRunning => Script != null && Script.IsRunning;

        public bool Start()
        {
            if(Script == null || Script.HasFinished) Script = CreateInstance(TypeImplIScript);
            bool b = Script.CanBeStarted();
            Game.LogVerbose("ScriptStatus.CanStart: " + b);
            Logging.Logger.LogDebug(nameof(ScriptStatus), nameof(Start), "ScriptID: " + Id);
            if (b)
            {
                Script.Start();
                return true;
            }
            else return false;
        }

        private IScript CreateInstance(Type type)
            => (IScript)Activator.CreateInstance(type);

        /// <summary>
        /// Use with simple script managers.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="typeOfBaseScript"></param>
        /// <param name="nextScriptToRunId"></param>
        public ScriptStatus(
            string id, Type typeOfBaseScript) 
            : this(
                  id, typeOfBaseScript,
                  EInitModels.TimerBased, 
                  new string[0], new List<string[]>(), 0, 0)
        {
        }

        /// <summary>
        /// Use with AdvancedScriptManager.
        /// </summary>
        /// <param name="id">Unique ID of the script.</param>
        /// <param name="typeOfBaseScript">Type of a script which implements IScript.</param>
        /// <param name="initModel">Initialization model.</param>
        /// <param name="nextScriptToRunId">Defines which script will be started after finishing this one.</param>
        /// <param name="scriptsToFinishPrior">Defines which script has to be finished before this one starts.</param>
        /// <param name="timerMin">Minimal startup time when using with TimerControlledScriptStarter.</param>
        /// <param name="timerMax">Maximal startup time when using with TimerControlledScriptStarter.</param>
        public ScriptStatus(
            string id, Type typeOfBaseScript, EInitModels initModel,
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
        }
    }
}
