using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LtFlash.Common.ScriptManager.Managers
{
    public class RandomScriptManager : TimerBasedScriptManager
    {
        public RandomScriptManager(double intervalMin, double intervalMax) 
            : base(intervalMin, intervalMax)
        {
            removeOnStart = false;
        }

        protected override void StartScript() => StartRandomScript();
    }
}
