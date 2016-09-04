using System;
using System.Collections.Generic;
using System.Linq;
using Rage;
using LtFlash.Common.ScriptManager.ScriptStarters;
using LtFlash.Common.Processes;
using LtFlash.Common.Logging;
using LtFlash.Common.ScriptManager.Scripts;

namespace LtFlash.Common.ScriptManager.Managers
{
    public class AdvancedScriptManager
    {
        //PUBLIC
        public bool IsRunning { get; private set; }
        public double DefaultTimerIntervalMax { get; set; } = 30000;
        public double DefaultTimerIntervalMin { get; set; } = 15000;
        public bool AutoSwapFromSequentialToTimer { get; set; } = true;
        public bool HasFinished { get; private set; }

        //PRIVATE
        private List<IScript> _off = new List<IScript>();
        private List<IScript> _queue = new List<IScript>();
        private List<IScriptStarter> _running = new List<IScriptStarter>();

        private Dictionary<string, bool> statusOfScripts = new Dictionary<string, bool>();

        private ProcessHost stages = new ProcessHost();

        public AdvancedScriptManager()
        {
            stages.AddProcess(Process_RunScriptsFromQueue);
            stages.AddProcess(Process_UnsuccessfullyFinishedScripts);
            stages.AddProcess(Process_WaitScriptsForFinish);
            stages.AddProcess(Process_CheckIfAllFinished);
        }

        public void AddScript(
            Type typeImplIScript, string id, Scripts.EInitModels initModel, 
            string[] nextScriptsToRun, List<List<string>> scriptsToFinishPrior,
            double timerIntervalMin, double timerIntervalMax)
        {
            if (!typeImplIScript.GetInterfaces().Contains(typeof(IScript)))
            {
                throw new ArgumentException(
                    $"Parameter does not implement {nameof(IScript)} interface.",
                    nameof(typeImplIScript));
            }

            //ScriptStatus s = new ScriptStatus(
                //id, typeImplIScript, initModel,
                //nextScriptsToRun, scriptsToFinishPrior,
                //timerIntervalMin, timerIntervalMax);
            IScriptStatus s = new ScriptStatus(id);
            s.InitModel = initModel;
            s.TimerIntervalMin = timerIntervalMin;
            s.TimerIntervalMax = timerIntervalMax;
            s.NextScripts = new List<string>(nextScriptsToRun);
            s.ScriptsToFinishPriorThis = scriptsToFinishPrior;

            IScript sc = CreateInstance(typeImplIScript);
            sc.Status = s;

            AddNewScriptToList(sc, id);
        }

        private IScript CreateInstance(Type type)
            => (IScript)Activator.CreateInstance(type);

        public void AddScript(
            Type typeImplIScript, string id, Scripts.EInitModels initModel,
            string[] nextScriptsToRun, List<List<string>> scriptsToFinishPrior)
        {
            AddScript(typeImplIScript, id, initModel,
                nextScriptsToRun, scriptsToFinishPrior,
                DefaultTimerIntervalMin, DefaultTimerIntervalMax);
        }

        public void AddScript(
            Type typeBaseScript, string id,
            Scripts.EInitModels initModel)
        {
            AddScript(
                typeBaseScript, id, initModel,
                new string[0], new List<List<string>>(),
                DefaultTimerIntervalMin, DefaultTimerIntervalMin);
        }

        public void Start() 
        {
            StartScript(_off.First().Status.Id);
        }

        public void StartScript(string id)
        {
            //clear prior list to prevent blockage
            GetScriptById(id, _off).Status.ScriptsToFinishPriorThis = new List<List<string>>();

            MoveInactiveScriptToQueue(id, _off, _queue);

            RegisterProcesses();
            IsRunning = true;
        }

        public bool HasScriptFinished(string id)
        {
            if(!statusOfScripts.ContainsKey(id))
            {
                throw new ArgumentException(
                    $"{nameof(HasScriptFinished)}: Script with id [{id}] does not exist.");
            }

            return statusOfScripts[id];
        }

        private void RegisterProcesses()
        {
            stages.ActivateProcess(Process_RunScriptsFromQueue);
            stages.ActivateProcess(Process_UnsuccessfullyFinishedScripts);
            stages.ActivateProcess(Process_WaitScriptsForFinish);
            stages.ActivateProcess(Process_CheckIfAllFinished);
            stages.Start();
        }

        private void Process_RunScriptsFromQueue()
        {
            for (int i = 0; i < _queue.Count; i++)
            {
                if(CheckIfScriptCanBeStarted(_queue[i]))
                {
                    MoveScriptFromQueueToRunning(_queue[i], _queue, _running);
                }
            }
        }

        private void Process_UnsuccessfullyFinishedScripts()
        {
            List<IScriptStarter> ufs = GetUnsuccessfullyFinishedScripts(_running);
            ufs = GetScriptsWithSequentialStarter(ufs);

            if (ufs.Count < 1) return;

            for (int i = 0; i < ufs.Count; i++)
            {
                ufs[i].Stop();

                IScript s = ufs[i].Script;

                s.Status = new ScriptStatus(s.Status.Id);
                s.Status.InitModel = EInitModels.TimerBased;
                s.Status.ScriptsToFinishPriorThis = new List<List<string>>();

                IScript newScript = (IScript)Activator.CreateInstance(ufs[i].Script.GetType());
                newScript.Status = new ScriptStatus(s.Status.Id);
                newScript.Status.InitModel = EInitModels.TimerBased;
                newScript.Status.NextScripts = s.Status.NextScripts;
                newScript.Status.ScriptsToFinishPriorThis = new List<List<string>>();
                newScript.Status.TimerIntervalMax = s.Status.TimerIntervalMax;
                newScript.Status.TimerIntervalMin = s.Status.TimerIntervalMin;

                _queue.Add(newScript);
            }
            Logger.LogDebug(
                nameof(AdvancedScriptManager), 
                nameof(Process_UnsuccessfullyFinishedScripts), 
                "pre removing: running; " + _running.Count);

            RemoveScripts(ufs, _running);

            Logger.LogDebug(
                nameof(AdvancedScriptManager), 
                nameof(Process_UnsuccessfullyFinishedScripts), 
                "removed: running; " + _running.Count);
        }

        private void Process_WaitScriptsForFinish()
        {
            List<IScriptStarter> fs = GetSuccessfullyFinishedScripts(_running);

            if (fs.Count < 1) return;

            SetScriptStatusAsFinished(fs);

            for (int i = 0; i < fs.Count; i++)
            {
                AddScriptsToQueue(fs[i].NextScriptsToRun);
            }

            RemoveScripts(fs, _running);
        }

        private void Process_CheckIfAllFinished()
        {
            if(_off.Count == 0 && _queue.Count == 0 && _running.Count == 0)
            {
                HasFinished = true;
                Stop();

                Logging.Logger.Log(
                    nameof(AdvancedScriptManager), 
                    nameof(Process_CheckIfAllFinished), 
                    "All script finished");
            }
        }

        private void AddNewScriptToList(IScript script, string id)
        {
            _off.Add(script);
            statusOfScripts.Add(id, false);
        }

        private bool CheckIfScriptCanBeStarted(IScript script)
        {
            if (script.Status.ScriptsToFinishPriorThis.Count < 1)
                return true;
            else
                return CheckIfNecessaryScriptsAreFinished(
                    script.Status.ScriptsToFinishPriorThis, statusOfScripts);
        }

        private IScript GetScriptById(string id, List<IScript> from)
        {
            IScript s = from.FirstOrDefault(ss => ss.Status.Id == id);
            if(s == null)
            {
                throw new ArgumentException(
                    $"{nameof(GetScriptById)}: Script with id [{id}] does not exist.");
            }
            else return s;
        }

        private IScriptStarter CreateScriptStarterByScriptId(
            string id, 
            List<IScript> scriptsToRun)
        {
            IScript s = GetScriptById(id, scriptsToRun); 
            return CreateScriptStarter(s);
        }

        private List<IScriptStarter> CreateScriptsStartersByIds(
            string[] ids, 
            List<IScript> scripts)
        {
            List<IScriptStarter> result = new List<IScriptStarter>();

            for (int i = 0; i < ids.Length; i++)
            {
                IScriptStarter ss = CreateScriptStarterByScriptId(ids[i], scripts);

                result.Add(ss);
            }

            return result;
        }

        private IScriptStarter CreateScriptStarter(IScript ss)
        {
            switch (ss.Status.InitModel)
            {
                case EInitModels.Sequential:
                    return new SequentialScriptStarter(ss, true);

                case EInitModels.TimerBased:
                default:
                    return new TimerControlledScriptStarter(ss, true);
            }
        }

        private bool CheckIfNecessaryScriptsAreFinished(
            List<List<string>> scripts, 
            Dictionary<string, bool> status)
        {
            List<bool> arrays = new List<bool>();

            for (int i = 0; i < scripts.Count; i++)
            {
                //TODO: protection check for non-existant key - no sense in running
                //the mod when a crucial script might be missing?
                //implement a function to CheckIfAll()?
                arrays.Add(scripts[i].All(s => status[s])); 
            }

            return arrays.Any(b => b == true);
        }

        private void StartScripts(List<IScriptStarter> starters)
        {
            for (int i = 0; i < starters.Count; i++)
            {
                starters[i].Start();
            }
        }

        private void RemoveScripts(
            List<IScriptStarter> scriptsToRemove, List<IScriptStarter> from)
        {
            for (int i = 0; i < scriptsToRemove.Count; i++)
            {
                for (int j = 0; j < from.Count; j++)
                {
                    if (from[j].Id == scriptsToRemove[i].Id) from.RemoveAt(j);
                }
            }
        }

        private List<IScriptStarter> GetSuccessfullyFinishedScripts(List<IScriptStarter> running)
            => GetScripts(running, s => s.HasFinishedSuccessfully);

        private List<IScriptStarter> GetUnsuccessfullyFinishedScripts(List<IScriptStarter> running)
            => GetScripts(running, s => s.HasFinishedUnsuccessfully);
        

        private List<IScriptStarter> GetScriptsWithSequentialStarter(List<IScriptStarter> running)
            => GetScripts(running, s => s.Script.Status.InitModel == EInitModels.Sequential);
        

        private List<IScriptStarter> GetScripts(
            List<IScriptStarter> running,
            Func<IScriptStarter, bool> conditions)
            => running.Where(conditions).ToList();

        private void SetScriptStatusAsFinished(List<IScriptStarter> scripts)
        {
            for (int i = 0; i < scripts.Count; i++)
            {
                statusOfScripts[scripts[i].Id] = true;
            }
        }

        private void MoveInactiveScriptToQueue(
            string scriptId, 
            List<IScript> from, List<IScript> to)
        {
            IScript s = GetScriptById(scriptId, from);
            to.Add(s);
            from.Remove(s);
            Game.LogVerbose($"{nameof(AdvancedScriptManager)}.{nameof(MoveInactiveScriptToQueue)}: {s.Status.Id}");
        }

        private void MoveScriptFromQueueToRunning(
            IScript scriptToRun, 
            List<IScript> from, List<IScriptStarter> to)
        {
            IScriptStarter s = CreateScriptStarter(scriptToRun);
            s.Start();
            to.Add(s);
            from.Remove(scriptToRun);
            Game.LogVerbose(nameof(AdvancedScriptManager) + "." + nameof(MoveScriptFromQueueToRunning) + ":" + s.Id);
        }

        private void AddScriptsToQueue(List<string> scriptsToRun)
        {
            for (int i = 0; i < scriptsToRun.Count; i++)
            {
                MoveInactiveScriptToQueue(scriptsToRun[i], _off, _queue);
            }
        }

        private void Stop()
        {
            stages.Stop();
        }
    }
}
