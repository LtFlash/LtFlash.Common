using Rage;
using LtFlash.Common.ScriptManager.Resources;

namespace LtFlash.Common.ScriptManager.ScriptStarters
{
    internal class TimerControlledScriptStarter : ScriptStarterBase
    {
        //PRIVATE
        private System.Timers.Timer _timer = new System.Timers.Timer();
        private double 
            _intervalMin, 
            _intervalMax;

        public TimerControlledScriptStarter(
            Managers.ScriptStatus ss, bool autoRestart = true) 
            : base(ss, autoRestart)
        {
            _intervalMin = ss.TimerIntervalMin;
            _intervalMax = ss.TimerIntervalMax;

            _timer.Interval = GetRandomInterval(_intervalMin, _intervalMax);
            _timer.Elapsed += TimerTick;
            _timer.AutoReset = true;
        }

        private void TimerTick(object sender, System.Timers.ElapsedEventArgs e)
        {
            Logger.Log(nameof(TimerControlledScriptStarter),
                    nameof(TimerTick), "0");

            if(!ScriptStarted || script.HasFinishedUnsuccessfully)
            {
                if (ScriptStarted && script.HasFinishedUnsuccessfully && !AutoRestart)
                {
                    _timer.Stop();
                    HasFinishedUnsuccessfully = true;
                    return;
                }

                StartScriptInThisTick = true;
                Logger.Log(nameof(TimerControlledScriptStarter), 
                    nameof(TimerTick), ScriptStarted.ToString());
            }
            else if(script.HasFinishedSuccessfully)
            {
                _timer.Stop();
            }

            _timer.Interval = GetRandomInterval(_intervalMin, _intervalMax);
        }

        public override void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        private double GetRandomInterval(double min, double max)
        {
            return MathHelper.GetRandomDouble(min, max);
        }
    }
}
