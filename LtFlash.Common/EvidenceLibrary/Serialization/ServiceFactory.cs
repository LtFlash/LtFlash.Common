using LtFlash.Common.EvidenceLibrary.Services;

namespace LtFlash.Common.EvidenceLibrary.Serialization
{
    public class ServiceFactory
    {
        public static EMS CreateEMS(Rage.Ped patient, string[] dialog, EMSData ed)
        {
            var e = new EMS(patient, ed.DispatchTo, dialog, ed.TransportToHospital, ed.SpawnAtScene, ed.DispatchFromHospital);
            ApplyServiceProperties(e, ed);
            return e;
        }

        public static Coroner CreateCoroner(Rage.Ped victim, string[] dialog, CoronerData cd)
        {
            Coroner c;

            if (cd.DispatchFrom != SpawnPoint.Zero)
            {
                c = new Coroner(victim, cd.DispatchTo, cd.DispatchFrom, dialog, cd.SpawnAtScene);
            }
            else
            {
                c = new Coroner(victim, cd.DispatchTo, dialog, cd.SpawnAtScene);
            }

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
