using LtFlash.Common.ScriptManager.Managers;

namespace LtFlash.Common.ScriptManager.Scripts
{
    public abstract class CaseController : BasicScript, IScript
    {
        protected AdvancedScriptManager Stages { get; private set; }
            = new AdvancedScriptManager();

        public CaseController() : base()
        {
            AddStagesOfCase();
        }

        public abstract void AddStagesOfCase();

        protected override bool Initialize()
        {
            Stages.Start();
            return true;
        }

        protected override void Process()
        {
            if(Stages.HasFinished) SetScriptFinished(true);
        }
    }
}
