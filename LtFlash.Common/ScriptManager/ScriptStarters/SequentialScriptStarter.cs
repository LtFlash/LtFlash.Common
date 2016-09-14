using LtFlash.Common.Logging;
using LtFlash.Common.ScriptManager.Scripts;
using System.Timers;

namespace LtFlash.Common.ScriptManager.ScriptStarters
{
    internal class SequentialScriptStarter : ScriptStarterBase
    {
        private const double INTERVAL = 500;
        private Timer timer = new Timer(INTERVAL);

        public SequentialScriptStarter(IScript s, bool autoRestart) 
            : base(s, autoRestart)
        {
            timer.Elapsed += TimerTick;
        }

        private void TimerTick(object sender, ElapsedEventArgs e)
        {
            if (!ScriptStarted || Script.HasFinishedUnsuccessfully)
            {
                if(ScriptStarted && Script.HasFinishedUnsuccessfully && !AutoRestart)
                {
                    timer.Stop();
                    HasFinishedUnsuccessfully = true;
                    return;
                }

                StartScriptInThisTick = true;

                Logger.Log(nameof(SequentialScriptStarter),
                    nameof(TimerTick), ScriptStarted.ToString());
            }
            else if (Script.HasFinishedSuccessfully)
            {
                timer.Stop();
            }
        }

        public override void Start() => timer.Start();

        public override void Stop() => timer.Stop();
    }
}
