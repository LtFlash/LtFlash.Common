using LtFlash.Common.ScriptManager.Scripts;
using System.Collections.Generic;

namespace LtFlash.Common.ScriptManager.Managers
{
    //to use as a member of ScriptBase
    class New_ScriptStatus
    {
        public string Id { get; }
        public double TimerIntervalMax { get; set; }
        public double TimerIntervalMin { get; set; }
        public List<string> NextScripts { get; set; } = new List<string>();
        //TODO: check if doesnt thrown null ref on embedded list
        public List<List<string>> ScriptsToFinishPriorThis { get; set; } = new List<List<string>>();
        public EInitModels InitModel { get; set; } = EInitModels.Sequential;

        public New_ScriptStatus(string id)
        {
            Id = id;
        }
    }

    interface IScriptStatus
    {
        string Id { get; }
        double TimerIntervalMax { get; set; }
        double TimerIntervalMin { get; set; }
        List<string> NextScripts { get; set; }
        List<List<string>> ScriptsToFinishPriorThis { get; set; }
        EInitModels InitModel { get; set; }
    }
}
