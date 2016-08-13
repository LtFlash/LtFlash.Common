using System;
using System.Collections.Generic;
using System.Linq;
using Rage;
using LtFlash.Common.Processes;

namespace LtFlash.Common.ScriptManager.Managers
{
    public class NewScriptManagerBase
    {
        //PROTECTED
        protected ProcessHost ProcHost { get; private set; } = new ProcessHost();
        
        protected bool canStartNewScript = true;
        protected bool removeOnStart = true;

        //PRIVATE
        private List<ScriptStatus> _off = new List<ScriptStatus>();
        private ScriptStatus _await;
        private ScriptStatus _running;

        private Dictionary<string, bool> statusOfScripts 
            = new Dictionary<string, bool>();

        private bool _restartOnFailure;


        public NewScriptManagerBase()
        {
            ProcHost.AddProcess(CheckWaiting);
            ProcHost.AddProcess(CheckRunningScript);

            ProcHost.Start();
        }

        public void AddScript(string id, Type typeImplIScript)
        {
            _off.Add(new ScriptStatus(id, typeImplIScript));
            statusOfScripts.Add(id, false);
        }

        protected bool StartFromFirstScript()
        {
            if (_off.Count < 1) return false;

            StartScript(_off.First());
            return true;
        }

        protected bool StartRandomScript()
        {
            if (_off.Count < 1) return false;

            StartScript(_off[MathHelper.GetRandomInteger(_off.Count)]);
            return true;
        }

        protected void StartScript(ScriptStatus script)
        {
            _await = script;
            if(removeOnStart) _off.Remove(script);
            ProcHost.ActivateProcess(CheckWaiting);
        }

        private void CheckWaiting()
        {
            if (_await == null) return;

            if (_await.Start())
            {
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
                statusOfScripts[_running.Id] = true;
                _running = null;
                canStartNewScript = true;
                ProcHost.DeactivateProcess(CheckRunningScript);
            }
            //TODO: if unsuccessful -> add current to _await list
        }

        private ScriptStatus GetScriptById(string id, List<ScriptStatus> from)
        {
            ScriptStatus s = from.FirstOrDefault(ss => ss.Id == id);
            if (s == null)
            {
                throw new ArgumentException(
                    $"{nameof(GetScriptById)}: Script with id [{id}] does not exist.");
            }
            else return s;
        }
    }
}
