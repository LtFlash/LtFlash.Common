using Rage;
using LtFlash.Common.Logging;
<<<<<<< HEAD
using LtFlash.Common.ScriptManager.Scripts;
=======
>>>>>>> refs/remotes/origin/master
using System.Timers;

namespace LtFlash.Common.ScriptManager.ScriptStarters
{
    internal class TimerControlledScriptStarter : ScriptStarterBase
    {
        //PRIVATE
        private Timer _timer = new Timer();
<<<<<<< HEAD
=======
        private double 
            _intervalMin, 
            _intervalMax;
>>>>>>> refs/remotes/origin/master

        public TimerControlledScriptStarter(
            IScript ss, bool autoRestart = true) 
            : base(ss, autoRestart)
        {
<<<<<<< HEAD
            _timer.Interval = GetRandomInterval();
=======
            //TODO: remove vars and use IScript.IScrtipStatus.InervalMax/Min
            //directly in GetRandomInterval(), no params
            _intervalMin = ss.TimerIntervalMin;
            _intervalMax = ss.TimerIntervalMax;

            _timer.Interval = GetRandomInterval(_intervalMin, _intervalMax);
>>>>>>> refs/remotes/origin/master
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

<<<<<<< HEAD
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
=======
        private double GetRandomInterval(double min, double max)
            => MathHelper.GetRandomDouble(min, max);
>>>>>>> refs/remotes/origin/master
    }
}
