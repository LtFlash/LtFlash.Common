using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptManager.Scripts
{
    public class CaseController : ScriptBase
    {
        private Managers.TimerBasedScriptManager _cases = new Managers.TimerBasedScriptManager();

        protected override bool Initialize()
        {
            return true;
        }
        protected override void Process()
        {
            //if all finished SetScriptFinished

        }

        protected override void End()
        {
        }
    }
}
