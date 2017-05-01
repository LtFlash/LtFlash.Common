using LtFlash.Common.Processes;
using Rage;
using Rage.Native;
using System.Diagnostics;
using System.Windows.Forms;

namespace LtFlash.Common.EvidenceLibrary.Services
{
    public abstract class ServiceBase : ICollectable
    {
        //PUBLIC
        public bool IsCollected { get; protected set; }
        public string MsgIsCollected { get; set; }
        public float DisposeDistance { get; set; } = 200.0f;
        public float VehicleDrivingSpeed { get; set; } = 10.0f;
        public VehicleDrivingFlags VehDrivingFlags { get; set; } 
            = VehicleDrivingFlags.Emergency;

        public string ModelVehicle { get; set; }
        public string ModelPedDriver { get; set; }
        public string ModelPedWorker { get; set; }

        public Keys KeyStartDialogue { get; set; } = Keys.Y;
        public Keys KeyTeleportToDestination { get; set; } = Keys.D8;

        public Ped PedDriver { get; private set; }
        public Ped PedWorker { get; private set; }

        public SpawnPoint SpawnPosition { get; set; }
        public bool AlwaysNotifyToTeleport { get; set; }

        //PROTECTED
        protected Vehicle Vehicle { get; private set; }
        protected Vector3 PlayerPos => Game.LocalPlayer.Character.Position;
        protected IDialog Dialogue { get; set; }
        protected abstract string MessageNotifyTPWhenStuck { get; }

        //PRIVATE
        protected ProcessHost Proc { get; private set; } = new ProcessHost();
        private Blip blipVeh;
        private SpawnPoint destPoint;
        private Object notepad;
        private Stopwatch timerNotifyTPWhenStuck;
        private Vector3 previousPosition;

        public ServiceBase(
            string vehModel, string modelPedDriver, string modelPedWorker,
            SpawnPoint spawnPos, SpawnPoint dest, string[] dialogue)
        {
            ModelVehicle = vehModel;
            ModelPedWorker = modelPedWorker;
            ModelPedDriver = modelPedDriver;

            SpawnPosition = spawnPos;
            destPoint = dest;

            var d = new Dialog(dialogue);
            d.PedOne = PedWorker;
            d.PedTwo = Game.LocalPlayer.Character;
            Dialogue = d;
        }

        public ServiceBase(
            string vehModel, string modelPedDriver, string modelPedWorker,
            SpawnPoint spawnPos, SpawnPoint dest, IDialog dialog)
        {
            ModelVehicle = vehModel;
            ModelPedWorker = modelPedWorker;
            ModelPedDriver = modelPedDriver;

            SpawnPosition = spawnPos;
            destPoint = dest;

            Dialogue = dialog;
        }

        public void Dispatch()
        {
            Proc.ActivateProcess(CreateEntities);
            Proc.Start();
        }

        protected void AttachNotepadToPedDriver()
        {
            notepad = new Object("prop_notepad_01", PedDriver.Position);
            int boneId = PedDriver.GetBoneIndex(PedBoneId.LeftPhHand);
            NativeFunction.Natives.AttachEntityToEntity(
                notepad, PedDriver, boneId, 
                0f, 0f, 0f, 0f, 0f, 0f, 
                true, false, false, false, 2, 1);
        }
        
        private void CreateEntities()
        {
            Vehicle = new Vehicle(ModelVehicle, SpawnPosition.Position);
            Vehicle.Heading = SpawnPosition.Heading;
            Vehicle.MakePersistent();
            blipVeh = new Blip(Vehicle);
            blipVeh.Scale = 0.5f;
            Vehicle.IsInvincible = true;

            PedDriver = new Ped(ModelPedDriver, Vehicle.Position.Around2D(5f), 0f);
            PedDriver.RandomizeVariation();
            PedDriver.WarpIntoVehicle(Vehicle, -1);
            PedDriver.BlockPermanentEvents = true;
            PedDriver.KeepTasks = true;

            PedWorker = new Ped(ModelPedWorker, Vehicle.Position.Around2D(5f), 0f);
            PedWorker.RandomizeVariation();
            PedWorker.WarpIntoVehicle(Vehicle, 0);
            PedWorker.BlockPermanentEvents = true;
            PedWorker.KeepTasks = true;

            PostSpawn();

            Proc.SwapProcesses(CreateEntities, DispatchFromSpawnPoint);
            Proc.ActivateProcess(AntiRollOver);

            previousPosition = Vehicle.Position;
            timerNotifyTPWhenStuck = new Stopwatch();
            timerNotifyTPWhenStuck.Start();
            Proc.ActivateProcess(NotifyWhenStuck);
        }

        private void AntiRollOver()
        {
            if (!Vehicle.Exists()) Proc.DeactivateProcess(AntiRollOver);

            if (Vehicle.Rotation.Roll > 70f || Vehicle.Rotation.Roll < -70f)
            {
                Vehicle.SetRotationRoll(0f);
            }
        }

        private void NotifyWhenStuck()
        {
            if (timerNotifyTPWhenStuck.Elapsed.Seconds < 35) return;

            timerNotifyTPWhenStuck.Restart();

            if(Vector3.Distance(previousPosition, Vehicle.Position) < 10f ||
                AlwaysNotifyToTeleport)
            {
                Game.DisplayHelp(string.Format(MessageNotifyTPWhenStuck, KeyTeleportToDestination));

                Proc.ActivateProcess(VehicleStuck);
            }

            previousPosition = Vehicle.Position;
        }

        private void VehicleStuck()
        {
            if (Game.IsKeyDown(KeyTeleportToDestination))
            {
                if (Vehicle && PedDriver.IsInVehicle(Vehicle, false) && PedWorker.IsInVehicle(Vehicle, false))
                {
                    Vehicle.SetPositionWithSnap(destPoint.Position);
                    Vehicle.Heading = destPoint.Heading;

                    Proc.DeactivateProcess(NotifyWhenStuck);
                    Proc.DeactivateProcess(VehicleStuck);

                    Proc.DeactivateProcess(WaitForArrival);
                    Proc.ActivateProcess(PostArrival);

                    timerNotifyTPWhenStuck.Stop();
                }
            }
        }

        protected abstract void PostSpawn();

        private void DispatchFromSpawnPoint()
        {
            PedDriver.Tasks.DriveToPosition(
                Vehicle, destPoint.Position, 
                VehicleDrivingSpeed, VehDrivingFlags, 5f);

            Proc.SwapProcesses(DispatchFromSpawnPoint, WaitForArrival); 
        }

        private void WaitForArrival()
        {
            if (Vector3.Distance(Vehicle.Position, destPoint.Position) <= 10f && 
                Vehicle.Speed == 0f)
            {
                Proc.DeactivateProcess(NotifyWhenStuck);
                Proc.DeactivateProcess(VehicleStuck);

                timerNotifyTPWhenStuck.Stop();

                Proc.SwapProcesses(WaitForArrival, PostArrival);
            }
        }

        protected abstract void PostArrival();

        protected void BackToVehicle()
        {
            IsCollected = true;

            DisplayMsgIsCollected();

            PedWorker.Tasks.GoToOffsetFromEntity(Vehicle, 0.1f, 0f, 1f);
            PedDriver.Tasks.GoToOffsetFromEntity(Vehicle, 0.1f, 0f, 1f);

            Proc.DeactivateProcess(BackToVehicle);
            Proc.ActivateProcess(CheckIfPedDriverCloseToVeh);
            Proc.ActivateProcess(CheckIfPedWorkerCloseToVeh);
        }

        private void DisplayMsgIsCollected()
        {
            if(MsgIsCollected != string.Empty) Game.DisplayHelp(MsgIsCollected);
        }

        private void CheckIfPedDriverCloseToVeh()
        {
            if (Vector3.Distance(PedDriver.Position, Vehicle.Position) <= 5f)
            {
                PedDriver.Tasks.EnterVehicle(Vehicle, -1);
                Proc.SwapProcesses(CheckIfPedDriverCloseToVeh, CheckIfPedsAreInVeh);
            }
        }

        private void CheckIfPedWorkerCloseToVeh()
        {
            if (Vector3.Distance(PedWorker.Position, Vehicle.Position) <= 5f)
            {
                PedWorker.Tasks.EnterVehicle(Vehicle, 0);
                Proc.SwapProcesses(CheckIfPedWorkerCloseToVeh, CheckIfPedsAreInVeh);
            }
        }

        private void CheckIfPedsAreInVeh()
        {
            if (PedDriver.IsInVehicle(Vehicle, false) && 
                PedWorker.IsInVehicle(Vehicle, false))
            {
                Proc.SwapProcesses(CheckIfPedsAreInVeh, DriveBackToSpawn);
            }
        }

        protected void DriveBackToSpawn()
        {
            if (blipVeh.IsValid()) blipVeh.Delete();

            PedDriver.Tasks.DriveToPosition(
                SpawnPosition.Position, VehicleDrivingSpeed, VehDrivingFlags);

            Proc.SwapProcesses(DriveBackToSpawn, CheckIfCanBeDisposed);
        }

        private void CheckIfCanBeDisposed()
        {
            if (Vector3.Distance(PlayerPos, Vehicle.Position) >= DisposeDistance ||
                Vector3.Distance(Vehicle.Position, SpawnPosition.Position) <= 10f)
            {
                Proc.DeactivateProcess(CheckIfCanBeDisposed);
                Proc.DeactivateProcess(AntiRollOver);
                InternalDispose();
            }
        }

        private void InternalDispose()
        {
            if (blipVeh) blipVeh.Delete();
            if (PedDriver) PedDriver.Dismiss();
            if (PedWorker) PedWorker.Dismiss();
            if (Vehicle) Vehicle.Dismiss();

            Dispose();
        }

        public abstract void Dispose();
    }
}
