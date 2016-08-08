using System.Collections.Generic;
using System.Linq;
using Rage;
using System;

namespace LtFlash.Common.ScriptManager.Managers
{
    public abstract class ScriptManagerBase : IScriptManager
    {
        //PROTECTED
        protected ScriptStatus _scriptToRunInFiber { get; set; }
        protected List<ScriptStatus> _scripts = new List<ScriptStatus>();

        //PRIVATE
        private GameFiber _process;
        private bool _canRun = true;

        public ScriptManagerBase()
        {
            _process = new GameFiber(InternalProcess);
            _process.Start();
        }

        public ScriptStatus this[string id]
        {
            get
            {
                return _scripts.Find(s => s.Id == id);
            }
        }

        public void AddScript(string id, Type type)
        {
            if (type.GetInterfaces().Contains(typeof(Scripts.IScript)))
            {
                _scripts.Add(new ScriptStatus(id, type));
            }
        }

        public void StartScript(string id, bool checkIfCanBeStarted)
        {
            ScriptStatus ss = _scripts.FirstOrDefault(s => s.Id == id);
            if (ss != null) ss.Start();
        }

        //public void StopScript(string id)
        //{
        //    ScriptStatus ss = _scripts.FirstOrDefault(s => s.Id == id);
        //    if (ss != null) ss.Script.End();
        //}

        private void InternalProcess()
        {
            while(_canRun)
            {
                //is being started from inside a GameFiber because of issues
                //with initialization from outside
                if(_scriptToRunInFiber != null)
                {
                    _scriptToRunInFiber.Start();
                    _scriptToRunInFiber = null;
                }

                Process();

                GameFiber.Yield();
            }
        }

        protected virtual void Process()
        {

        }
    }
}
