using Rage;
using LtFlash.Common.Logging;
using LtFlash.Common.ScriptManager.Scripts;
using System.Timers;

namespace LtFlash.Common.ScriptManager.ScriptStarters
{
    internal class TimerControlledScriptStarter : ScriptStarterBase
    {
        //PRIVATE
        private Timer _timer = new Timer();

        public TimerControlledScriptStarter(
            IScript ss, bool autoRestart = true) 
            : base(ss, autoRestart)
        {
            _timer.Interval = GetRandomInterval();
            _timer.Elapsed += TimerTick;
            _timer.AutoReset = true;
        }

        private void TimerTick(object sender, ElapsedEventArgs e)
        {
            Logger.Log(nameof(TimerControlledScriptStarter),
                    nameof(TimerTick), "0");

            if(!ScriptStarted || Script.HasFinishedUnsuccessfully)
            {
                if (ScriptStarted && Script.HasFinishedUnsuccessfully && !AutoRestart)
                {
                    _timer.Stop();
                    HasFinishedUnsuccessfully = true;
                    return;
                }

                StartScriptInThisTick = true;
                Logger.Log(nameof(TimerControlledScriptStarter), 
                    nameof(TimerTick), ScriptStarted.ToString());
            }
            else if(Script.HasFinishedSuccessfully) _timer.Stop();

            _timer.Interval = GetRandomInterval();
        }

        public override void Start()
        {
            _timer.Start();
        }

        public override void Stop()
        {
            _timer.Stop();
        } 

        private double GetRandomInterval()
        {
            Logger.LogDebug(
                nameof(TimerControlledScriptStarter), 
                nameof(GetRandomInterval), 
                $"id:{Script.Attributes.Id}: {Script.Attributes.TimerIntervalMin}-{Script.Attributes.TimerIntervalMax}");

            return MathHelper.GetRandomDouble(
                Script.Attributes.TimerIntervalMin, 
                Script.Attributes.TimerIntervalMax);
        }
    }
}
