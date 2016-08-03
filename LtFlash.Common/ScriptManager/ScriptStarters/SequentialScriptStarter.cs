using LtFlash.Common.ScriptManager.Managers;
using LtFlash.Common.ScriptManager.Resources;

namespace LtFlash.Common.ScriptManager.ScriptStarters
{
    internal class SequentialScriptStarter : ScriptStarterBase
    {
        private const double INTERVAL = 500;
        private System.Timers.Timer _timer = new System.Timers.Timer(INTERVAL);

        public SequentialScriptStarter(ScriptStatus s, bool autoRestart) 
            : base(s, autoRestart)
        {
            _timer.Elapsed += TimerTick;
        }

        private void TimerTick(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!ScriptStarted || script.HasFinishedUnsuccessfully)
            {
                if(ScriptStarted && script.HasFinishedUnsuccessfully && !AutoRestart)
                {
                    _timer.Stop();
                    HasFinishedUnsuccessfully = true;
                    return;
                }

                StartScriptInThisTick = true;
                Logger.Log(nameof(SequentialScriptStarter),
                    nameof(TimerTick), ScriptStarted.ToString());
            }
            else if (script.HasFinishedSuccessfully)
            {
                _timer.Stop();
            }
        }

        public override void Start()
        {
            _timer.Start();
        }
    }
}
