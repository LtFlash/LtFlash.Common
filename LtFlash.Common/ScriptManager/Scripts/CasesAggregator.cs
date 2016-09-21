namespace LtFlash.Common.ScriptManager.Scripts
{
    public abstract class CasesAggregator : BasicScript, IScript
    {
        protected Managers.RandomScriptManager Cases 
            = new Managers.RandomScriptManager(15000, 30000);

        public CasesAggregator() : base()
        {
            AddCases();
        }

        protected abstract void AddCases();

        protected override bool Initialize()
        {
            Cases.StartTimer();
            return true;
        }

        protected override void Process()
        {
        }

        protected override void End()
        {
        }
    }
}
