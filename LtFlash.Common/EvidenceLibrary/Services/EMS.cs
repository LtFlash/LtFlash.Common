using LtFlash.Common.EvidenceLibrary.Resources;
using Rage;
using Rage.Native;

namespace LtFlash.Common.EvidenceLibrary.Services
{
    public class EMS : ServiceBase
    {
        private Ped patient;
        private bool takeToHospital;
        private Blip blipEmt;

        public EMS(
            Ped patient, 
            SpawnPoint dispTo, 
            string[] dialog, 
            bool transportToHospital, 
            bool spawnAtScene = false, 
            EHospitals dispatchFrom = EHospitals.Closest)
            : base(
                  "AMBULANCE", "s_m_m_paramedic_01", "s_m_m_paramedic_01", 
                  GetSpawn(spawnAtScene, dispatchFrom, dispTo), 
                  dispTo, dialog)
        {
            this.patient = patient;
            takeToHospital = transportToHospital;
        }

        public EMS(
            Ped patient,
            SpawnPoint dispTo,
            string[] dialog,
            bool transportToHospital,
            bool spawnAtScene,
            SpawnPoint dispatchFrom)
            : base(
                  "AMBULANCE", "s_m_m_paramedic_01", "s_m_m_paramedic_01",
                  dispatchFrom,
                  dispTo, dialog)
        {
            this.patient = patient;
            takeToHospital = transportToHospital;
        }

        private static SpawnPoint GetSpawn(
            bool _spawnAtScene, EHospitals dispatchFrom, SpawnPoint dispatchTo)
        {
            return _spawnAtScene ? dispatchTo : 
                (dispatchFrom == EHospitals.Closest ? 
                Hospitals.GetClosestHospitalSpawn(dispatchTo.Position) : 
                Hospitals.GetHospitalSpawn(dispatchFrom));
        }

        protected override void PostSpawn()
        {
            Vehicle.IsSirenOn = true;
        }

        protected override void PostArrival()
        {
            Vehicle.IsSirenSilent = true;
            Proc.SwapProcesses(PostArrival, GoToPatient);
        }

        private void GoToPatient()
        {
            AttachNotepadToPedDriver();

            PedWorker.Tasks.GoToOffsetFromEntity(patient, 1f, 0f, 5f);
            PedDriver.Tasks.GoToOffsetFromEntity(patient, 4f, 8f, 5f);

            Proc.SwapProcesses(GoToPatient, CheckIfWithPatient);
        }

        private void CheckIfWithPatient()
        {
            if (PedWorker.Position.DistanceTo(patient) < 2f &&
                PedDriver.Position.DistanceTo(patient) < 5f)
            {
                Proc.SwapProcesses(CheckIfWithPatient, PerformProcedures);
            }
        }

        private void PerformProcedures()
        {
            Procedures();

            if (takeToHospital) MovePatientToAmbulance(patient);

            blipEmt = new Blip(PedWorker);
            blipEmt.Color = System.Drawing.Color.Green;
            blipEmt.Sprite = BlipSprite.Health;
            blipEmt.Scale = 0.25f;

            Game.DisplayNotification("Talk to EMS to receive a medical report.");
            Proc.SwapProcesses(PerformProcedures, WaitForDialogueActivation);
        }

        private void WaitForDialogueActivation()
        {
            if (Vector3.Distance(Game.LocalPlayer.Character.Position, PedWorker.Position) <= 3f)
            {
                Game.DisplayHelp($"Press ~y~{KeyStartDialogue}~s~ to talk to the paramedic.");

                if (Game.IsKeyDown(KeyStartDialogue))
                {
                    if (blipEmt) blipEmt.Delete();

                    Dialogue.StartDialog(PedWorker, Game.LocalPlayer.Character);
                    Proc.SwapProcesses(WaitForDialogueActivation, CheckForDialogueFinished);
                }
            }
        }

        private void CheckForDialogueFinished()
        {
            if (Dialogue.HasEnded)
                Proc.SwapProcesses(CheckForDialogueFinished, BackToVehicle);
        }

        private void MovePatientToAmbulance(Ped patient)
        {
            for (int i = 1; i < 51; i++)
            {
                if (patient) NativeFunction.Natives.SetEntityAlpha(patient, 255 - i * 5, false);
                GameFiber.Wait(10);
            }

            if (patient) patient.Delete();
        }

        public void Procedures()
        {
            bool emt = false, emtd = false;
            GameFiber.StartNew(delegate
            {
                GameFiber.Sleep(3000);
                PedWorker.Position = patient.LeftPosition;

                NativeFunction.CallByName<uint>("TASK_TURN_PED_TO_FACE_ENTITY", PedDriver, patient, 1000);
                GameFiber.Sleep(1100);

                PedDriver.Tasks.PlayAnimation("amb@medic@standing@timeofdeath@enter", "enter", 
                    4, AnimationFlags.StayInEndFrame);
                GameFiber.Sleep(9000);

                PedDriver.Tasks.PlayAnimation("amb@medic@standing@timeofdeath@idle_a", "idle_b", 
                    4, AnimationFlags.StayInEndFrame);
                GameFiber.Sleep(6000);

                PedDriver.Tasks.PlayAnimation("amb@medic@standing@timeofdeath@exit", "exit", 
                    4, AnimationFlags.StayInEndFrame);
                GameFiber.Sleep(7000);

                emtd = true;
            });
            //====================
            GameFiber.StartNew(delegate
            {

                NativeFunction.CallByName<uint>("TASK_TURN_PED_TO_FACE_ENTITY", PedWorker, patient, 1000);
                GameFiber.Sleep(1100);

                PedWorker.Tasks.PlayAnimation("amb@medic@standing@tendtodead@enter", "enter", 
                    4, AnimationFlags.StayInEndFrame);
                GameFiber.Sleep(2000);

                PedWorker.Tasks.PlayAnimation("amb@medic@standing@tendtodead@idle_a", "idle_b", 
                    4, AnimationFlags.StayInEndFrame);
                GameFiber.Sleep(4000);

                PedWorker.Tasks.PlayAnimation("amb@medic@standing@tendtodead@exit", "exit", 
                    4, AnimationFlags.StayInEndFrame);
                GameFiber.Sleep(2000);

                PedWorker.Tasks.PlayAnimation("amb@code_human_police_investigate@idle_intro", "idle_intro", 
                    4, AnimationFlags.StayInEndFrame);
                GameFiber.Sleep(1500);

                PedWorker.Tasks.PlayAnimation("amb@code_human_police_investigate@idle_b", "idle_d", 
                    3, AnimationFlags.None);
                GameFiber.Sleep(9000);

                emt = true;
            });

            while (!emt && !emtd)
            {
                GameFiber.Yield();
            }
        }

        public override void Dispose()
        {
        }
    }
}
