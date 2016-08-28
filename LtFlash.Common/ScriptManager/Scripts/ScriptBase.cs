using LtFlash.Common.ScriptManager.ScriptStartController;
using Rage;
using System;

namespace LtFlash.Common.ScriptManager.Scripts
{
    public abstract class ScriptBase
    {
        //PUBLIC
        public bool HasFinished { get; protected set; }
        public bool Completed { get; protected set; }
        public bool IsRunning { get; private set; }

        //PROTECTED
        protected virtual IScriptStartController ScriptStartController { get; } 
            = new UnconditionalStartController();

        protected Vector3 PlayerPos => Game.LocalPlayer.Character.Position;

        //PRIVATE
        private Processes.ProcessHost ProcHost = new Processes.ProcessHost();

        public ScriptBase()
        {
            //empty, ctor called to check CanBeStarted()
        }

        public bool CanBeStarted() => ScriptStartController.CanBeStarted();

        public void Start()
        {
            if(!ProcHost.IsRunning) ProcHost.Start();
            IsRunning = true;
        }

        public void Stop()
        {
            ProcHost.Stop();
            HasFinished = true;
            IsRunning = false;
        }

        protected void AddStage(Action stage) => ProcHost.AddProcess(stage);

        protected void ActivateStage(Action stage)
            => ProcHost.ActivateProcess(stage);

        protected void DeactivateStage(Action stage)
            => ProcHost.DeactivateProcess(stage);

        protected void SwapStages(Action toDisable, Action toEnable)
            => ProcHost.SwapProcesses(toDisable, toEnable);

        protected abstract bool Initialize();
        protected abstract void Process();
        protected abstract void End();
    }
}
