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
        public bool this[Action proc]
        {
            get
            {
                return IsListed(proc) ? Find(proc).Active : false;
            }
            set
            {
                if (value) ActivateProcess(proc);
                else DeactivateProcess(proc);
            }
        }

        //PRIVATE
        private GameFiber fiber;
        private bool canRun = true;
        private List<Proc> processes = new List<Proc>();

        private class Proc
        {
            public readonly Action Function;
            public bool Active;

            public Proc(Action act, bool active)
            {
                Function = act;
                Active = active;
            }
        }

        public ProcessHost()
        {
            fiber = new GameFiber(InternalProcess);
        }

        public ProcessHost(bool autoRun) : base()
        {
            if (autoRun) Start();
        }

        public void Start()
        {
            IsRunning = true;
            canRun = true;
            fiber.Start();
        }

        public void Stop()
        {
            IsRunning = false;
            canRun = false;
        }

        public void AddProcess(Action proc)
        {
            AddProcess(proc, false);
        }

        public void AddProcess(Action proc, bool isActive)
        {
            if (!IsListed(proc)) processes.Add(new Proc(proc, isActive));
        }

        public void ActivateProcess(Action proc)
        {
            if (!IsListed(proc)) AddProcess(proc);
            Find(proc).Active = true;
        }

        public void DeactivateProcess(Action proc)
        {
            if(IsListed(proc)) Find(proc).Active = false;
        }

        private Proc Find(Action proc)
        {
            return processes.Find(a => a.Function == proc);
        }

        private bool IsListed(Action proc)
        {
            return processes.FirstOrDefault(s => s.Function == proc) != null;
        }

        public void SwapProcesses(Action toDisable, Action toEnable)
        {
            DeactivateProcess(toDisable);
            ActivateProcess(toEnable);
        }

        private void InternalProcess()
        {
            while (canRun)
            {
                Process();
                GameFiber.Yield();
            }
        }

        public void Process()
        {
            for (int i = 0; i < processes.Count; i++)
            {
                if (processes[i].Active) processes[i].Function();
            }
        }
    }
}
