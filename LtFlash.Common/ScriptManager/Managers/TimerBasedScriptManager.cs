using System.Timers;
using Rage;
using System;

namespace LtFlash.Common.ScriptManager.Managers
{
    public class TimerBasedScriptManager : ScriptManagerBase
    {
        //PRIVATE
        private Timer timer = new Timer();
        private double intervalMax = 3 * 60 * 1000;
        private double intervalMin = 1 * 60 * 1000;
        private bool elapsed = false;

        public TimerBasedScriptManager(
            double intervalMin = 60000, double intervalMax = 180000) : base()
        {
            this.intervalMax = intervalMax;
            this.intervalMin = intervalMin;
        }

        public void StartTimer()
        {
            timer.Interval = GetRandomInterval();
            timer.Elapsed += TimerTick;
            timer.AutoReset = true;
            timer.Start();
            canStartNewScript = true;
        }

        private double GetRandomInterval()
            => MathHelper.GetRandomDouble(intervalMin, intervalMax);

        private void StartNewScript()
        {
            if (!(canStartNewScript && elapsed)) return;

            StartScript();
            elapsed = false;
            canStartNewScript = false;

            Logging.Logger.LogDebug(
                nameof(TimerBasedScriptManager), nameof(StartNewScript), "Called");
        }

        protected virtual void StartScript() => StartFromFirstScript();

        private void TimerTick(object sender, ElapsedEventArgs e)
        {
            timer.Interval = GetRandomInterval();
            elapsed = true;

            StartNewScript();

            Logging.Logger.LogDebug(
                nameof(TimerBasedScriptManager), 
                nameof(TimerTick), 
                "interval: " + Math.Round(timer.Interval / 1000) + "s");
        }
    }
}
