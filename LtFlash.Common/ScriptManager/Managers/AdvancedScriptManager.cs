using System;
using System.Collections.Generic;
using System.Linq;
using Rage;
using ScriptManager.ScriptStarters;

namespace ScriptManager.Managers
{
    public class AdvancedScriptManager
    {
        //PUBLIC
        public double DefaultTimerIntervalMax { get; set; } = 30000;
        public double DefaultTimerIntervalMin { get; set; } = 15000;
        public bool IsRunning { get; private set; }
        public bool AutoSwapFromSequentialToTimer { get; set; } = true;

        //PRIVATE
        private List<ScriptStatus> _off = new List<ScriptStatus>();
        private List<ScriptStatus> _queue = new List<ScriptStatus>();
        private List<IScriptStarter> _running = new List<IScriptStarter>();

        private Dictionary<string, bool> _statusOfScripts = new Dictionary<string, bool>();

        private Resources.ProcessHost _stages = new Resources.ProcessHost();

        public AdvancedScriptManager()
        {
            _stages.AddProcess(Process_RunScriptsFromQueue);
            _stages.AddProcess(Process_UnsuccessfullyFinishedScripts);
            _stages.AddProcess(Process_WaitScriptsForFinish);
        }

        public void AddScript(
            Type typeImplIScript, string id, Scripts.EInitModels initModel, 
            string[] nextScriptsToRun, List<string[]> scriptsToFinishPrior,
            double timerIntervalMin, double timerIntervalMax)
        {
            if (!typeImplIScript.GetInterfaces().Contains(typeof(Scripts.IScript)))
            {
                throw new ArgumentException(
                    $"Parameter does not implement {nameof(Scripts.IScript)} interface.",
                    nameof(typeImplIScript));
            }

            ScriptStatus s = new ScriptStatus(
                id, typeImplIScript, initModel,
                nextScriptsToRun, scriptsToFinishPrior,
                timerIntervalMin, timerIntervalMax);

            AddNewScriptToList(s, id);
        }

        public void AddScript(
            Type typeImplIScript, string id, Scripts.EInitModels initModel,
            string[] nextScriptsToRun, List<string[]> scriptsToFinishPrior)
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
                new string[0], new List<string[]>(),
                DefaultTimerIntervalMin, DefaultTimerIntervalMin);
        }

        public void Start() 
        {
            StartScript(_off.First().Id);
        }

        public void StartScript(string id)
        {
            //clear prior list to prevent blockage
            GetScriptById(id, _off).ScriptsToFinishPriorThis = new List<string[]>();

            MoveInactiveScriptToQueue(id, _off, _queue);

            RegisterProcesses();
            IsRunning = true;
        }

        public bool HasScriptFinished(string id)
        {
            if(!_statusOfScripts.ContainsKey(id))
            {
                throw new ArgumentException(
                    $"{nameof(HasScriptFinished)}: Script with id [{id}] does not exist.");
            }

            return _statusOfScripts[id];
        }

        private void RegisterProcesses()
        {
            _stages.ActivateProcess(Process_RunScriptsFromQueue);
            _stages.ActivateProcess(Process_UnsuccessfullyFinishedScripts);
            _stages.ActivateProcess(Process_WaitScriptsForFinish);
            _stages.Start();
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
                ScriptStatus s = ufs[i].GetScriptStatus();
                ScriptStatus newScript = new ScriptStatus(
                    s.Id, s.TypeImplIScript, Scripts.EInitModels.TimerBased,
                    s.NextScriptToRunIds, new List<string[]>(), 
                    s.TimerIntervalMin, s.TimerIntervalMax);

                _running.Add(CreateScriptStarter(s));
            }

            RemoveScripts(_running, ufs);
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

        private void AddNewScriptToList(ScriptStatus script, string id)
        {
            _off.Add(script);
            _statusOfScripts.Add(id, false);
        }

        private bool CheckIfScriptCanBeStarted(ScriptStatus script)
        {
            if (script.ScriptsToFinishPriorThis.Count < 1)
                return true;
            else
                return CheckIfNecessaryScriptsAreFinished(
                    script.ScriptsToFinishPriorThis, _statusOfScripts);
        }

        private ScriptStatus GetScriptById(string id, List<ScriptStatus> from)
        {
            ScriptStatus s = from.FirstOrDefault(ss => ss.Id == id);
            if(s == null)
            {
                throw new ArgumentException(
                    $"{nameof(GetScriptById)}: Script with id [{id}] does not exist.");
            }
            else return s;
        }

        private IScriptStarter CreateScriptStarterByScriptId(
            string id, 
            List<ScriptStatus> scriptsToRun)
        {
            ScriptStatus s = GetScriptById(id, scriptsToRun); 
            return CreateScriptStarter(s);
        }

        private List<IScriptStarter> CreateScriptsStartersByIds(
            string[] ids, 
            List<ScriptStatus> scripts)
        {
            List<IScriptStarter> result = new List<IScriptStarter>();

            for (int i = 0; i < ids.Length; i++)
            {
                IScriptStarter ss = CreateScriptStarterByScriptId(ids[i], scripts);

                result.Add(ss);
            }

            return result;
        }

        private IScriptStarter CreateScriptStarter(ScriptStatus ss)
        {
            switch (ss.InitModel)
            {
                case Scripts.EInitModels.Sequential:
                    return new SequentialScriptStarter(ss, true);

                case Scripts.EInitModels.TimerBased:
                default:
                    return new TimerControlledScriptStarter(ss, true);
            }
        }

        private bool CheckIfNecessaryScriptsAreFinished(
            List<string[]> scripts, 
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
                from.Remove(scriptsToRemove[i]);
            }
        }

        private List<IScriptStarter> GetSuccessfullyFinishedScripts(
            List<IScriptStarter> running)
            => GetScripts(running, s => s.HasFinishedSuccessfully);

        private List<IScriptStarter> GetUnsuccessfullyFinishedScripts(
            List<IScriptStarter> running)
            => GetScripts(running, s => s.HasFinishedUnsuccessfully);
        

        private List<IScriptStarter> GetScriptsWithSequentialStarter(
            List<IScriptStarter> running)
            => GetScripts(running, s => s.GetScriptStatus()
            .InitModel == Scripts.EInitModels.Sequential);
        

        private List<IScriptStarter> GetScripts(
            List<IScriptStarter> running,
            Func<IScriptStarter, bool> conditions)
        {
            Game.LogVerbose($"{nameof(AdvancedScriptManager)}.{nameof(GetScripts)}");
            return running.Where(conditions).ToList();
        }

        private void SetScriptStatusAsFinished(List<IScriptStarter> scripts)
        {
            for (int i = 0; i < scripts.Count; i++)
            {
                _statusOfScripts[scripts[i].Id] = true;
            }
        }

        private void MoveInactiveScriptToQueue(
            string scriptId, 
            List<ScriptStatus> from, List<ScriptStatus> to)
        {
            ScriptStatus s = GetScriptById(scriptId, from);
            to.Add(s);
            from.Remove(s);
            Game.LogVerbose($"{nameof(AdvancedScriptManager)}.{nameof(MoveInactiveScriptToQueue)}: {s.Id}");
        }

        private void MoveScriptFromQueueToRunning(
            ScriptStatus scriptToRun, 
            List<ScriptStatus> from, List<IScriptStarter> to)
        {
            IScriptStarter s = CreateScriptStarter(scriptToRun);
            s.Start();
            to.Add(s);
            from.Remove(scriptToRun);
            Game.LogVerbose(nameof(AdvancedScriptManager) + "." + nameof(MoveScriptFromQueueToRunning) + ":" + s.Id);
        }

        private void AddScriptsToQueue(string[] scriptsToRun)
        {
            for (int i = 0; i < scriptsToRun.Length; i++)
            {
                MoveInactiveScriptToQueue(scriptsToRun[i], _off, _queue);
            }
        }
    }
}
