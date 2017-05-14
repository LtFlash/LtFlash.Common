using Rage;

namespace LtFlash.Common.EvidenceLibrary.Serialization
{
    public class ServiceData : IIdentifiable
    {
        public string ID { get; set; }
        public string VehModel;
        public SpawnPoint DispatchFrom;
        public SpawnPoint DispatchTo;
        public string PedClientID;

        public string MsgIsCollected { get; set; }
        public bool ShouldSerializeMsgIsCollected() => !string.IsNullOrEmpty(MsgIsCollected);

        public float? DisposeDistance { get; set; }
        public bool ShouldSerializeDisposeDistance() => DisposeDistance.HasValue;

        public float? VehicleDrivingSpeed { get; set; }
        public bool ShouldSerializeVehicleDrivingSpeed() => VehicleDrivingSpeed.HasValue;

        public VehicleDrivingFlags? VehDrivingFlags { get; set; }
        public bool ShouldSerializeVehDrivingFlags() => VehDrivingFlags.HasValue;

        public ServiceData()
        {

        }
    }

    public class TalkableServiceData : ServiceData
    {
        public string DialogID;
        public string ReportID;
        public bool SpawnAtScene;

        public TalkableServiceData() 
        {

        }
    }

    public class EMSData : TalkableServiceData
    {
        public EHospitals DispatchFromHospital;
        public bool TransportToHospital;

        public EMSData()
        {
                
        }
    }

    public class CoronerData : TalkableServiceData
    {
        public CoronerData()
        {
        }
    }

    public class TransportData : ServiceData
    {
        public Vector3 PickupPos;
        public EPoliceStations DispatchFromStation;

        public TransportData()
        {
        }
    }
}
