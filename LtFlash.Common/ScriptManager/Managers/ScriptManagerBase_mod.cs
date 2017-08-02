using System;
using System.Collections.Generic;
using System.Linq;
using Rage;
using LtFlash.Common.Processes;
using LtFlash.Common.ScriptManager.Scripts;

namespace LtFlash.Common.ScriptManager.Managers
{
    public class ScriptManagerBase
    {
        public bool HasFinished { get; private set; }
        public bool RemoveAfterExecution
        {
            get {return removeOnStart;}
            set {removeOnStart = value;}
        }

        //PROTECTED
        protected ProcessHost ProcHost { get; private set; } = new ProcessHost();
        
        protected bool canStartNewScript = true;
        protected bool removeOnStart = true;
        
        private List<Type> off = new List<Type>();
        private IScript _await;
        private IScript _running;

        private Dictionary<string, bool> statusOfScripts 
            = new Dictionary<string, bool>();

        //TODO: implement restarting
        private bool _restartOnFailure;

        public ScriptManagerBase()
        {
            ProcHost.AddProcess(CheckWaiting);
            ProcHost.AddProcess(CheckRunningScript);
            ProcHost.Start();
        }

        public void AddScript(string id, Type typeImplIScript)
        {
            off.Add(typeImplIScript);
            //IScript s = (IScript)Activator.CreateInstance(typeImplIScript);
            //s.Attributes = new ScriptAttributes(id);
            //_off.Add(s);
            statusOfScripts.Add(id, false);
        }

        protected bool StartFromFirstScript()
        {
            if (off.Count < 1) return false;

            StartScript(off.First());
            return true;
        }

        protected bool StartRandomScript()
        {
            if (off.Count < 1) return false;

            StartScript(off[MathHelper.GetRandomInteger(off.Count)]);
            return true;
        }

        protected void StartScript(Type script)
        {
            IScript s = (IScript)Activator.CreateInstance(script);
            //s.Attributes = new ScriptAttributes(id);
            
            _await = s;
            if(removeOnStart) off.Remove(script);
            ProcHost.ActivateProcess(CheckWaiting);
        }

        private void CheckWaiting()
        {
            if (_await == null) return;
            if (_await.CanBeStarted())
            {
                _await.Start();
                _running = _await;
                _await = null;
                ProcHost.SwapProcesses(CheckWaiting, CheckRunningScript);
            }
        }

        private void CheckRunningScript()
        {
            if (_running == null) return;

            if (_running.HasFinishedSuccessfully)
            {
                //statusOfScripts[_running.Attributes.Id] = true;
                _running = null;
                canStartNewScript = true;
                ProcHost.DeactivateProcess(CheckRunningScript);

                if(off.Count == 0)
                {
                    HasFinished = true;
                    Stop();
                }
            }
            //TODO: if unsuccessful -> add current to _await list/add as [0] to _off
            else if(_running.HasFinishedUnsuccessfully)
            {
                //off.Insert(0, _running.GetType());
                _running = null;
                canStartNewScript = true;
                ProcHost.DeactivateProcess(CheckRunningScript);
            }
        }

        public void Stop() => ProcHost.Stop();

        private IScript GetScriptById(string id, List<IScript> from)
        {
            IScript s = from.FirstOrDefault(ss => ss.Attributes.Id == id);
            if (s == null)
            {
                throw new ArgumentException(
                    $"{nameof(GetScriptById)}: Script with id [{id}] does not exist.");
            }
            else return s;
        }
    }
}
