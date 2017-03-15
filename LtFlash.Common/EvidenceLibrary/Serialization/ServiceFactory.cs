using LtFlash.Common.EvidenceLibrary.Services;

namespace LtFlash.Common.EvidenceLibrary.Serialization
{
    public class ServiceFactory
    {
        public static EMS CreateEMS(Rage.Ped patient, string[] dialog, EMSData ed)
        {
            var e = new Services.EMS(patient, ed.DispatchTo, dialog, ed.TransportToHospital, ed.SpawnAtScene, ed.DispatchFrom);
            ApplyServiceProperties(e, ed);
            return e;
        }

        public static Coroner CreateCoroner(Rage.Ped victim, string[] dialog, CoronerData cd)
        {
            var c = new Coroner(victim, cd.DispatchTo, dialog, cd.SpawnAtScene);
            ApplyServiceProperties(c, cd);
            return c;
        }

        public static void ApplyServiceProperties(ServiceBase s, ServiceData data)
        {
            if (data.DisposeDistance.HasValue) s.DisposeDistance = data.DisposeDistance.Value;
            if (!string.IsNullOrEmpty(data.MsgIsCollected)) s.MsgIsCollected = data.MsgIsCollected;
            if (data.VehDrivingFlags.HasValue) s.VehDrivingFlags = data.VehDrivingFlags.Value;
            if (data.VehicleDrivingSpeed.HasValue) s.VehicleDrivingSpeed = data.VehicleDrivingSpeed.Value;
        }
    }
}
