using LtFlash.Common.Processes;
using Rage;
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

        public Keys KeyStartDialogue { get; set; } = Keys.Y;

        //PROTECTED
        protected Ped PedDriver { get; private set; }
        protected Ped PedWorker { get; private set; }
        protected Vehicle Vehicle { get; private set; }
        protected Vector3 PlayerPos => Game.LocalPlayer.Character.Position;
        protected Dialog Dialogue { get; set; }

        //PRIVATE
        protected ProcessHost Proc { get; private set; } = new ProcessHost();
        private string vehModel;
        private Blip blipVeh;
        private string modelPedDriver;
        private string modelPedWorker;
        private SpawnPoint spawnPos;
        private SpawnPoint destPoint;

        public ServiceBase(
            string vehModel, string modelPedDriver, string modelPedWorker,
            SpawnPoint spawnPos, SpawnPoint dest, string[] dialogue)
        {
            this.vehModel = vehModel;
            this.spawnPos = spawnPos;
            destPoint = dest;

            this.modelPedDriver = modelPedDriver;
            this.modelPedWorker = modelPedWorker;

            Dialogue = new Dialog(dialogue);

            Proc.AddProcess(CreateEntities);
            Proc.AddProcess(DispatchFromSpawnPoint);
            Proc.AddProcess(WaitForArrival);
            Proc.AddProcess(PostArrival);
            Proc.AddProcess(BackToVehicle);
            Proc.AddProcess(CheckIfPedDriverCloseToVeh);
            Proc.AddProcess(CheckIfPedWorkerCloseToVeh);
            Proc.AddProcess(CheckIfPedsAreInVeh);
            Proc.AddProcess(DriveBackToSpawn);
            Proc.AddProcess(CheckIfCanBeDisposed);

            Proc.ActivateProcess(CreateEntities);
            Proc.Start();
        }        

        private void CreateEntities()
        {
            Vehicle = new Vehicle(vehModel, spawnPos.Position);
            Vehicle.Heading = spawnPos.Heading;
            Vehicle.MakePersistent();
            blipVeh = new Blip(Vehicle);
            blipVeh.Scale = 0.5f;
            Vehicle.IsInvincible = true;

            PedDriver = new Ped(modelPedDriver, Vehicle.Position.Around2D(5f), 0f);
            PedDriver.RandomizeVariation();
            PedDriver.WarpIntoVehicle(Vehicle, -1);
            PedDriver.BlockPermanentEvents = true;
            PedDriver.KeepTasks = true;

            PedWorker = new Ped(modelPedWorker, Vehicle.Position.Around2D(5f), 0f);
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
            if (Vehicle.Position.DistanceTo(destPoint.Position) <= 10f && 
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
            Vehicle.GetAttachedBlip().Delete();

            PedDriver.Tasks.DriveToPosition(
                spawnPos.Position, VehicleDrivingSpeed, VehDrivingFlags);

            Proc.SwapProcesses(DriveBackToSpawn, CheckIfCanBeDisposed);
        }

        private void CheckIfCanBeDisposed()
        {
            if (Vector3.Distance(PlayerPos, Vehicle.Position) >= DisposeDistance ||
                Vector3.Distance(Vehicle.Position, spawnPos.Position) <= 10f)
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
