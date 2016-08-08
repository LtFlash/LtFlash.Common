using Rage;

namespace LtFlash.Common.EvidenceLibrary.Services
{
    public abstract class ServiceBase
    {
        //PUBLIC
        public float DisposeDistance { get; set; } = 200.0f;
        public float VehicleDrivingSpeed { get; set; } = 10.0f;
        public VehicleDrivingFlags VehDrivingFlags { get; set; } 
            = VehicleDrivingFlags.Emergency;

        //PROTECTED
        protected Ped PedDriver { get; private set; }
        protected Ped PedWorker { get; private set; }
        protected Vehicle Vehicle { get; private set; }

        protected Vector3 PlayerPos
            { get { return Game.LocalPlayer.Character.Position; } }

        //PRIVATE
        private string _vehModel;

        private Blip _blipVeh;

        private string _modelPedDriver;
        private string _modelPedWorker;

        private SpawnPoint _spawnPos;
        private SpawnPoint _destPoint;

        private Processes.ProcessHost _proc = new Processes.ProcessHost();

        public ServiceBase(
            string vehModel, string modelPedDriver, string modelPedWorker,
            SpawnPoint spawnPos, SpawnPoint dest)
        {
            _vehModel = vehModel;
            _spawnPos = spawnPos;
            _destPoint = dest;

            _modelPedDriver = modelPedDriver;
            _modelPedWorker = modelPedWorker;

            _proc.AddProcess(CreateEntities);
            _proc.AddProcess(DispatchFromSpawnPoint);
            _proc.AddProcess(WaitForArrival);
            _proc.AddProcess(PostArrival);
            _proc.AddProcess(BackToVehicle);
            _proc.AddProcess(CheckIfPedDriverCloseToVeh);
            _proc.AddProcess(CheckIfPedWorkerCloseToVeh);
            _proc.AddProcess(CheckIfPedsAreInVeh);
            _proc.AddProcess(DriveBackToSpawn);
            _proc.AddProcess(CheckIfCanBeDisposed);
        }        

        private void CreateEntities()
        {
            Vehicle = new Vehicle(_vehModel, _spawnPos.Position);
            Vehicle.Heading = _spawnPos.Heading;
            Vehicle.MakePersistent();
            _blipVeh = new Blip(Vehicle);
            _blipVeh.Scale = 0.5f;
            Vehicle.IsInvincible = true;

            PedDriver = new Ped(_modelPedDriver, Vehicle.Position.Around2D(5f), 0f);
            PedDriver.RandomizeVariation();
            PedDriver.WarpIntoVehicle(Vehicle, -1);
            PedDriver.BlockPermanentEvents = true;
            PedDriver.KeepTasks = true;

            PedWorker = new Ped(_modelPedWorker, Vehicle.Position.Around2D(5f), 0f);
            PedWorker.RandomizeVariation();
            PedWorker.WarpIntoVehicle(Vehicle, 0);
            PedWorker.BlockPermanentEvents = true;
            PedWorker.KeepTasks = true;

            PostSpawn();

            _proc.SwapProcesses(CreateEntities, DispatchFromSpawnPoint);
        }

        protected abstract void PostSpawn();

        private void DispatchFromSpawnPoint()
        {
            PedDriver.Tasks.DriveToPosition(
                Vehicle, _spawnPos.Position, 
                VehicleDrivingSpeed, VehDrivingFlags, 5f);

            _proc.SwapProcesses(DispatchFromSpawnPoint, WaitForArrival);
        }

        private void WaitForArrival()
        {
            if (Vehicle.Position.DistanceTo(_destPoint.Position) <= 10f && 
                Vehicle.Speed == 0f)
            {
                _proc.SwapProcesses(WaitForArrival, PostArrival);
            }
        }

        protected abstract void PostArrival();

        protected void BackToVehicle()
        {
            PedWorker.Tasks.GoToOffsetFromEntity(Vehicle, 0.1f, 0f, 1f);
            PedDriver.Tasks.GoToOffsetFromEntity(Vehicle, 0.1f, 0f, 1f);

            _proc.DeactivateProcess(BackToVehicle);
            _proc.ActivateProcess(CheckIfPedDriverCloseToVeh);
            _proc.ActivateProcess(CheckIfPedWorkerCloseToVeh);
        }

        private void CheckIfPedDriverCloseToVeh()
        {
            if (Vector3.Distance(PedDriver.Position, Vehicle.Position) <= 5f)
            {
                PedDriver.Tasks.EnterVehicle(Vehicle, -1);
                _proc.SwapProcesses(CheckIfPedDriverCloseToVeh, CheckIfPedsAreInVeh);
            }
        }

        private void CheckIfPedWorkerCloseToVeh()
        {
            if (Vector3.Distance(PedWorker.Position, Vehicle.Position) <= 5f)
            {
                PedWorker.Tasks.EnterVehicle(Vehicle, 0);
                _proc.SwapProcesses(CheckIfPedDriverCloseToVeh, CheckIfPedsAreInVeh);
            }
        }

        private void CheckIfPedsAreInVeh()
        {
            if (PedDriver.IsInVehicle(Vehicle, false) && 
                PedWorker.IsInVehicle(Vehicle, false))
            {
                _proc.SwapProcesses(CheckIfPedsAreInVeh, DriveBackToSpawn);
            }
        }

        protected void DriveBackToSpawn()
        {
            Vehicle.GetAttachedBlip().Delete();

            PedDriver.Tasks.DriveToPosition(
                _spawnPos.Position, VehicleDrivingSpeed, VehDrivingFlags);

            _proc.SwapProcesses(DriveBackToSpawn, CheckIfCanBeDisposed);
        }

        private void CheckIfCanBeDisposed()
        {
            if (Vector3.Distance(PlayerPos, Vehicle.Position) >= DisposeDistance ||
                Vector3.Distance(Vehicle.Position, _spawnPos.Position) <= 10f)
            {
                _proc.DeactivateProcess(CheckIfCanBeDisposed);
                InternalDispose();
            }
        }

        private void InternalDispose()
        {
            if (_blipVeh) _blipVeh.Delete();
            if (PedDriver) PedDriver.Dismiss();
            if (PedWorker) PedWorker.Dismiss();
            if (Vehicle) Vehicle.Dismiss();

            Dispose();
        }

        public abstract void Dispose();
    }
}
