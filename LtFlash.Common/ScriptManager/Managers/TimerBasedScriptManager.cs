using System.Timers;
using Rage;

namespace LtFlash.Common.ScriptManager.Managers
{
    public class TimerBasedScriptManager : NewScriptManagerBase
    {
        private Timer _scriptRunTimer = new Timer();
        private double _intervalMax = 3 * 60 * 1000;
        private double _intervalMin = 1 * 60 * 1000;
        private bool _elapsed = false;

        public TimerBasedScriptManager(
            double intervalMin = 60000, double intervalMax = 180000) : base()
        {
            _intervalMax = intervalMax;
            _intervalMin = intervalMin;
        }

        public void StartTimer()
        {
            _scriptRunTimer.Interval = GetRandomInterval();
            _scriptRunTimer.Elapsed += TimerTick;
            _scriptRunTimer.AutoReset = true;
            _scriptRunTimer.Start();
            canStartNewScript = true;
        }

        private double GetRandomInterval()
            => MathHelper.GetRandomDouble(_intervalMin, _intervalMax);

        private void StartNewScript()
        {
            if (!canStartNewScript || !_elapsed) return;
            StartScript();
            _elapsed = false; 
            canStartNewScript = false;
        }

        protected virtual void StartScript() => StartFromFirstScript();

        private void TimerTick(object sender, ElapsedEventArgs e)
        {
            _scriptRunTimer.Interval = GetRandomInterval();
            _elapsed = true;

            StartNewScript();

            Logging.Logger.Log(
                nameof(TimerBasedScriptManager), 
                nameof(TimerTick), 
                "interval: " + _scriptRunTimer.Interval / 1000);
        }
    }
}
