namespace LtFlash.Common.ScriptManager.ScriptStartController
{
    public class DelayedStartController : IScriptStartController
    {
        private System.Timers.Timer _timer = new System.Timers.Timer();
        private bool _canBeStarted = false;

        public DelayedStartController(double delayInMs)
        {
            _timer.Interval = delayInMs;
            _timer.AutoReset = false;
            _timer.Elapsed += (s, e) => _canBeStarted = true;
            _timer.Start();
        }

        public bool CanBeStarted()
        {
            return _canBeStarted;
        }
    }
}
