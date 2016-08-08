using System;
using System.Linq;
using System.Collections.Generic;
using Rage;

namespace LtFlash.Common.Processes
{
    public class ProcessHost
    {
        //PUBLIC
        public bool IsRunning { get; private set; }

        //PRIVATE
        private GameFiber _process;
        private bool _canRun = true;
        private List<Proc> _processes = new List<Proc>();

        private class Proc
        {
            public Action Function;
            public bool Active;

            public Proc(Action act, bool active)
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
        }

        public void AddProcess(Action proc)
        {
            if(!CheckIfListed(proc)) _processes.Add(new Proc(proc, false));
        }

        public void ActivateProcess(Action proc)
        {
            if (!CheckIfListed(proc)) AddProcess(proc);
            _processes.Find(a => a.Function == proc).Active = true;
        }

        public void DeactivateProcess(Action proc)
        {
            if(CheckIfListed(proc))
                _processes.Find(a => a.Function == proc).Active = false;
        }

        private bool CheckIfListed(Action proc)
        {
            return _processes.FirstOrDefault(s => s.Function == proc) != null;
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
            for (int i = 0; i < _processes.Count; i++)
            {
                if (_processes[i].Active) _processes[i].Function();
            }
        }
    }
}
