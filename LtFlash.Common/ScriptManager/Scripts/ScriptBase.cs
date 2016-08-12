using System;
using System.Collections.Generic;
using Rage;
using LtFlash.Common.ScriptManager.ScriptStartController;

namespace LtFlash.Common.ScriptManager.Scripts
{
    public abstract class ScriptBase
    {
        //PUBLIC
        public bool HasFinished { get; protected set; } = false;
        public bool Completed { get; protected set; } = false;
        public bool IsRunning { get; private set; } = false;

        //PROTECTED
        protected virtual IScriptStartController ScriptStartController
            { get; } = new UnconditionalStartController();

        protected Vector3 PlayerPos
            { get { return Game.LocalPlayer.Character.Position; } }

        //PRIVATE
        private GameFiber _process;
        private bool _canRun = true;

        private List<Stage> _stages = new List<Stage>();
        private class Stage
        {
            public Action Function;
            public bool Active;

            public Stage(Action act, bool active)
            {
                Function = act;
                Active = active;
            }
        }

        public ScriptBase()
        {
            //empty, ctor called to check CanBeStarted()
        }

        public bool CanBeStarted()
        {
            return ScriptStartController.CanBeStarted();
        }

        public void Start()
        {
            _process = new GameFiber(InternalProcess);
            _process.Start();
            IsRunning = true;
        }

        public void Stop()
        {
            _canRun = false;
            HasFinished = true;
            IsRunning = false;
        }

        protected void AddStage(Action stage)
        {
            _stages.Add(new Stage(stage, false));
        }

        protected void ActivateStage(Action stage)
        {
            _stages.Find(a => a.Function == stage).Active = true;
        }

        protected void DeactivateStage(Action stage)
        {
            _stages.Find(a => a.Function == stage).Active = false;
        }

        protected void SwapStages(Action toDisable, Action toEnable)
        {
            DeactivateStage(toDisable);
            ActivateStage(toEnable);
        }

        private void InternalProcess()
        {
            while(_canRun)
            {
                ExecStages();
                GameFiber.Yield();
            }
        }

        private void ExecStages()
        {
            for (int i = 0; i < _stages.Count; i++)
            {
                if (_stages[i].Active) _stages[i].Function();
            }
        }

        protected abstract bool Initialize();
        protected abstract void Process();
        protected abstract void End();
    }
}
