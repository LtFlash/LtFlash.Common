using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LtFlash.Common.ScriptManager.Scripts
{
    public abstract class BasicScript : ScriptBase, IScript
    {
        public BasicScript()
        {
            RegisterStages();
        }

        private void RegisterStages()
        {
            AddStage(InternalInitialize);
            AddStage(Process);
            AddStage(InternalEnd);

            ActivateStage(InternalInitialize);
        }

        private void InternalInitialize()
        {
            Initialize();
            SwapStages(InternalInitialize, Process);
        }


        private void InternalEnd()
        {
            End();
            Stop();
        }

        public void SetScriptFinished(bool completed)
        {
            Completed = completed;
            HasFinished = true;
            InternalEnd();
        }
    }
}
