using System;
using System.Collections.Generic;
using System.Linq;
using Rage;
using LtFlash.Common.Processes;

namespace LtFlash.Common.ScriptManager.Managers
{
    public class ScriptManagerBase
    {
        //PUBLIC
        public bool HasFinished { get; private set; }

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


        public ScriptManagerBase()
        {
            ProcHost.AddProcess(CheckWaiting);
            ProcHost.AddProcess(CheckRunningScript);
            //ProcHost.ActivateProcess(ShowStatusOfScripts);
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

        //private void ShowStatusOfScripts()
        //{
        //    Game.DisplaySubtitle("_off.Count: " + _off.Count + 
        //        " | await: " + (_await != null) + " | running: " + (_running != null) + 
        //        "~n~" + "canStart: " + canStartNewScript);
        //}

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

                if(_off.Count == 0)
                {
                    HasFinished = true;
                    Stop();
                }
            }
            //TODO: if unsuccessful -> add current to _await list/add as [0] to _off
            else if(_running.HasFinishedUnsuccessfully)
            {
                _off.Insert(0, _running);
                _running = null;
                canStartNewScript = true;
                ProcHost.DeactivateProcess(CheckRunningScript);
            }
        }

        public void Stop() => ProcHost.Stop();

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
