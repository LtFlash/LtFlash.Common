namespace LtFlash.Common.ScriptManager.Managers
{
    public class RandomScriptManager : TimerBasedScriptManager
    {
        public RandomScriptManager(double intervalMin, double intervalMax) 
            : base(intervalMin, intervalMax)
        {
        }

        protected override void StartScript() => StartRandomScript();
    }
}
