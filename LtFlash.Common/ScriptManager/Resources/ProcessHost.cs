using System;
using System.Collections.Generic;
using Rage;

namespace ScriptManager.Resources
{
    public class ProcessHost
    {
        //PUBLIC
        public bool IsRunning { get; private set; }

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

        public ProcessHost()
        {
            _process = new GameFiber(InternalProcess);
        }

        public ProcessHost(bool autoRun) : base()
        {
            if (autoRun) Start();
        }

        public void Start()
        {
            IsRunning = true;
            _canRun = true;
            _process.Start();
        }

        public void Stop()
        {
            IsRunning = false;
            _canRun = false;
            //Abort() has to be the last as it does not return control to function!
            _process.Abort();
        }

        public void AddProcess(Action proc)
        {
            _stages.Add(new Stage(proc, false));
        }

        public void ActivateProcess(Action proc)
        {
            _stages.Find(a => a.Function == proc).Active = true;
        }

        public void DeactivateProcess(Action proc)
        {
            _stages.Find(a => a.Function == proc).Active = false;
        }

        public void SwapProcesses(Action toDisable, Action toEnable)
        {
            DeactivateProcess(toDisable);
            ActivateProcess(toEnable);
        }

        private void InternalProcess()
        {
            while (_canRun)
            {
                Process();
                GameFiber.Yield();
            }
        }

        public void Process()
        {
            for (int i = 0; i < _stages.Count; i++)
            {
                if (_stages[i].Active) _stages[i].Function();
            }
        }
    }
}
