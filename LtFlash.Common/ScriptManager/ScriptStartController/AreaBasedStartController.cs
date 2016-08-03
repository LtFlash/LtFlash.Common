using System;
using System.Collections.Generic;
using Rage;
using LtFlash.Common.ScriptManager.Resources;

namespace LtFlash.Common.ScriptManager.ScriptStartController
{
    class AreaBasedStartController : IScriptStartController
    {
        private List<string> _zones = new List<string>();

        public AreaBasedStartController(Zones.EZones[] zones)
        {
            for (int i = 0; i < zones.Length; i++)
            {
                string name = Enum.GetName(typeof(Zones.EZones), zones[i]);
                _zones.Add(name);
            }
        }

        public bool CanBeStarted()
        {
            return _zones.Contains(Zones.GetZoneName(
                Game.LocalPlayer.Character.Position));
        }
    }
}
