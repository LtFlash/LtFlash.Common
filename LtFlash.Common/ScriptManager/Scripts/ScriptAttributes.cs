﻿using System.Collections.Generic;

namespace LtFlash.Common.ScriptManager.Scripts
{
    public class ScriptAttributes : IScriptAttributes
    {
        public string Id { get; }
        public double TimerIntervalMax { get; set; }
        public double TimerIntervalMin { get; set; }
        public List<string> NextScripts { get; set; } = new List<string>();
        public List<List<string>> ScriptsToFinishPriorThis { get; set; } 
            = new List<List<string>>();
        public EInitModels InitModel { get; set; } = EInitModels.Sequential;

        public ScriptAttributes()
        {
        }

        public ScriptAttributes(string id) : this()
        {
            Id = id;
        }
    }

    public interface IScriptAttributes
    {
        string Id { get; }
        double TimerIntervalMax { get; set; }
        double TimerIntervalMin { get; set; }
        List<string> NextScripts { get; set; }
        List<List<string>> ScriptsToFinishPriorThis { get; set; }
        EInitModels InitModel { get; set; }
    }
}