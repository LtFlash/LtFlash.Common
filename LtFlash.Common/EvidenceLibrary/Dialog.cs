using System.Timers;
using Rage;
using Rage.Native;

namespace LtFlash.Common.EvidenceLibrary
{
    public class Dialog : IDialog
    {
        // PUBLIC
        public bool IsRunning { get; private set; }
        public bool HasEnded { get; private set; }
        public int TimerInterval { get; set; } = 3000;
        public int LineDuration { get; set; } = 2500;
        public Ped PedOne { get; set; }
        public Ped PedTwo { get; set; }

        //use to play anims?
        //private Ped _ped1;
        //private Ped _ped2;
        
        // PRIVATE
        private string[] dialog;
        private Timer timer;
         
        private int currentLine; 
        private int linesInDialog;

        public Dialog(string[] dialog)
        {
            this.dialog = dialog;
            linesInDialog = this.dialog.Length;

            timer = new Timer(TimerInterval);
            timer.AutoReset = true;
            timer.Elapsed += (s, e) => ShowLine();
        }

        public void StartDialog()
        {
            if (IsRunning) return;

            if(PedOne && PedTwo)
            {
                TurnTo(PedOne, PedTwo);
                TurnTo(PedTwo, PedOne);
            }

            timer.Start();
            ShowLine();
            IsRunning = true;
            HasEnded = false;
        }
         
        private void TurnTo(Ped ped, Entity entity, int duration = 1500)
            => NativeFunction.Natives.TaskTurnPedToFaceEntity(ped, entity, duration);

        private void ShowLine()
        {
            GameFiber.StartNew(delegate
            {
                Game.DisplaySubtitle(dialog[currentLine], LineDuration);
             
                currentLine++;

                if (currentLine == linesInDialog) 
                {
                    timer.Stop();
                    HasEnded = true;
                    IsRunning = false;
                }
            });
        }
    }
}
