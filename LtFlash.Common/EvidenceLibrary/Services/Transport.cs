using Rage;
using LtFlash.Common.EvidenceLibrary.Resources;

//TODO:
// - nullref handling
//CHANGES:
// + Added Dispatch(), Dispose(), properties

namespace LtFlash.Common.EvidenceLibrary.Services
{
    public class Transport
    {
        public VehicleDrivingFlags DrivingFlagsArrival { get; set; } = VehicleDrivingFlags.StopAtDestination | VehicleDrivingFlags.Emergency;
        public VehicleDrivingFlags DrivingFlagsDeparture { get; set; } = VehicleDrivingFlags.Normal;
        public float SpeedArrival { get; set; } = 10f;
        public float SpeedDeparture { get; set; } = 10f;
        public string VehicleModel { get; set; } = MathHelper.Choose(vehModels);
        public string CopDriverModel { get; set; } = MathHelper.Choose(copModels);

        private Vehicle veh;
        private Blip blipVeh;
        private Ped copDriver;
        private Ped ped;

        private SpawnPoint vehSpawn;
        private Vector3 pickupPos;

        private readonly static string[] vehModels =
        {
            "POLICE",
            "POLICE2",
            "POLICE3",
        };
        private readonly static string[] copModels =
        {
            "s_m_y_cop_01",
        };

        private bool canRun = true;
        private const float DIST_AT_SCENE = 5.5f;
        private const float DIST_DISPOSAL = 200f;
        private const float BLIP_VEH_SCALE = 0.5f;

        private readonly Processes.ProcessHost ph = new Processes.ProcessHost();

        public Transport(
            Ped ped, Vector3 pickupPos, 
            EPoliceStations dispatchFrom = EPoliceStations.Closest) : 
            this(
                ped, pickupPos, 
                
                dispatchFrom == EPoliceStations.Closest ? 
                PoliceStations.GetClosestPoliceStationSpawn(pickupPos) : 
                PoliceStations.GetPoliceStationSpawn(dispatchFrom))
        {
        }

        public Transport(Ped ped, Vector3 pickupPos, SpawnPoint dispatchFrom)
        {
            this.ped = ped;
            vehSpawn = dispatchFrom;
            this.pickupPos = pickupPos;
        }

        public void Dispatch()
        {
            ph[CreateEntities] = true;
            ph.Start();
        }

        private void CreateEntities()
        {
            veh = new Vehicle(VehicleModel, vehSpawn.Position);
            veh.Heading = vehSpawn.Heading;
            veh.IsInvincible = true;
            veh.MakePersistent();

            blipVeh = new Blip(veh);
            blipVeh.Scale = BLIP_VEH_SCALE;
            blipVeh.Color = System.Drawing.Color.Blue;

            copDriver = new Ped(CopDriverModel, veh.Position.Around(2f), 0f);
            copDriver.WarpIntoVehicle(veh, -1);
            copDriver.KeepTasks = true;
            copDriver.BlockPermanentEvents = true;

            copDriver.Tasks.DriveToPosition(pickupPos, SpeedArrival, DrivingFlagsArrival);

            ph.SwapProcesses(CreateEntities, WaitForArrival);
        }

        private void WaitForArrival()
        {
            if (Vector3.Distance(veh.Position, pickupPos) <= DIST_AT_SCENE)
            {
                ph.SwapProcesses(WaitForArrival, SetPedTaskEnter);
            }
        }

        private void SetPedTaskEnter()
        {
            ped.Tasks.ClearImmediately();

            ped.Tasks.GoToOffsetFromEntity(veh, 3f, 90f, 1f);

            while (Vector3.Distance(ped.Position, veh.Position) > 7f)
            {
                GameFiber.Yield();
            }

            ped.Tasks.ClearImmediately();
            ped.Tasks.EnterVehicle(veh, 2);

            ph.SwapProcesses(SetPedTaskEnter, WaitForPed);
        }

        private void WaitForPed()
        {
            if (ped.IsInVehicle(veh, false))
            {
                if (blipVeh) blipVeh.Delete();
                copDriver.Tasks.DriveToPosition(vehSpawn.Position, SpeedDeparture, DrivingFlagsDeparture);
                ph.SwapProcesses(WaitForPed, CanBeDisposed);
            }
        }

        private void CanBeDisposed()
        {
            if (Vector3.Distance(veh.Position, Game.LocalPlayer.Character.Position) >= DIST_DISPOSAL)
            {
                Dispose();
                ph[CanBeDisposed] = false;
            }
        }

        public void Dispose()
        {
            if (ped.Exists()) ped.Dismiss();
            if (copDriver.Exists()) copDriver.Dismiss();
            if (veh.Exists()) veh.Dismiss();
            if (blipVeh) blipVeh.Delete();
            canRun = false;
            ph.Stop();
        }
    }
}
