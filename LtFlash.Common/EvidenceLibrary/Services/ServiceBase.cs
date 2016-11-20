using LtFlash.Common.Processes;
using Rage;
using Rage.Native;
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
        public Ped PedDriver { get; private set; }
        public Ped PedWorker { get; private set; }
        public SpawnPoint SpawnPosition { get; set; }

        //PROTECTED
        protected Vehicle Vehicle { get; private set; }
        protected Vector3 PlayerPos => Game.LocalPlayer.Character.Position;
        protected Dialog Dialogue { get; set; }

        //PRIVATE
        protected ProcessHost Proc { get; private set; } = new ProcessHost();
        private Blip blipVeh;
        private SpawnPoint destPoint;
        private Object notepad;

        public ServiceBase(
            string vehModel, string modelPedDriver, string modelPedWorker,
            SpawnPoint spawnPos, SpawnPoint dest, string[] dialogue)
        {
            ModelVehicle = vehModel;
            ModelPedWorker = modelPedWorker;
            ModelPedDriver = modelPedDriver;

            SpawnPosition = spawnPos;
            destPoint = dest;

            Dialogue = new Dialog(dialogue);
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
