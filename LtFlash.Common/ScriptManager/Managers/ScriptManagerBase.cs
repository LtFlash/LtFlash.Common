using System;
using System.Collections.Generic;
using System.Linq;
using Rage;
using LtFlash.Common.Processes;
using LtFlash.Common.ScriptManager.Scripts;

//CHANGES:
// - added RemoveScriptWhenSuccessful and RemoveScriptWhenUnsuccessful
// - added StartScriptById(id)

namespace LtFlash.Common.ScriptManager.Managers
{
    public class ScriptManagerBase
    {
        public bool HasFinished { get; private set; }
        public bool RemoveScriptWhenSuccessful { get; set; }
        public bool RemoveScriptWhenUnsuccessful { get; set; }

        protected ProcessHost ProcHost { get; private set; } = new ProcessHost();
        
        protected bool canStartNewScript = true;

        private readonly List<IScript> off = new List<IScript>();
        private IScript await;
        private IScript running;

        private Dictionary<string, bool> statusOfScripts = new Dictionary<string, bool>();

        public ScriptManagerBase()
        {
            ProcHost.Start();
        }

        public void AddScript(string id, Type typeImplIScript)
        {
            if (!typeImplIScript.GetInterfaces().Contains(typeof(IScript)))
            {
                var msg = $"{nameof(AddScript)}(id, type): parameter does not implement {nameof(IScript)} interface: {typeImplIScript}";
                throw new ArgumentException(msg);
            }

            if(statusOfScripts.ContainsKey(id))
            {
                var msg = $"{nameof(AddScript)}(id, type): given id already exists: {id}";
                throw new ArgumentException(msg);
            }

            AddScriptFinishedSuccessfullyToQueue(off, typeImplIScript, id);
            statusOfScripts.Add(id, false);
        }

        public void StartScriptById(string id)
        {
            if (!canStartNewScript) return;
            var s = GetScriptById(id, off);
            StartScript(s);
        }

        private static void AddScriptFinishedSuccessfullyToQueue(List<IScript> queue, Type s, string id)
        {
            IScript scr = CreateInstanceWithId(s, id);
            queue.Add(scr);
        }

        private static void AddScriptFinishedUnsuccessfullyToQueue(List<IScript> queue, Type s, string id)
        {
            IScript scr = CreateInstanceWithId(s, id);
            queue.Insert(0, scr);
        }

        private static IScript CreateInstanceWithId(Type t, string id)
        {
            IScript scr = (IScript)Activator.CreateInstance(t);
            scr.Attributes = new ScriptAttributes(id);
            return scr;
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
            var scr = MathHelper.Choose<IScript>(off);
            StartScript(scr);
            return true;
        }

        protected void StartScript(IScript script)
        {
            await = script;
            off.Remove(script);
            ProcHost[CheckWaiting] = true;
        }

        private void CheckWaiting()
        {
            if (await == null) return;
            if (await.CanBeStarted())
            {
                await.Start();
                running = await;
                await = null;
                ProcHost.SwapProcesses(CheckWaiting, CheckRunningScript);
            }
        }

        private void CheckRunningScript()
        {
            if (running == null) return;

            if (!running.HasFinished) return;

            if(running.HasFinishedSuccessfully && !RemoveScriptWhenSuccessful)
            {
                AddScriptFinishedSuccessfullyToQueue(off, running.GetType(), running.Attributes.Id);
            }

            if (running.HasFinishedUnsuccessfully && !RemoveScriptWhenUnsuccessful)
            {
                AddScriptFinishedUnsuccessfullyToQueue(off, running.GetType(), running.Attributes.Id);
            }

            statusOfScripts[running.Attributes.Id] = running.HasFinishedSuccessfully;

            running = null;
            canStartNewScript = true;

            ProcHost[CheckRunningScript] = false;

            if (off.Count == 0)
            {
                HasFinished = true;
                Stop();
            }
        }

        public void Stop() => ProcHost.Stop();

        private static IScript GetScriptById(string id, List<IScript> from)
        {
            IScript s = from.FirstOrDefault(ss => ss.Attributes.Id == id);
            if (s == default(IScript))
            {
                var msg = $"{nameof(GetScriptById)}: script with id [{id}] does not exist.";
                throw new ArgumentException(msg);
            }
            else return s;
        }
    }
}
