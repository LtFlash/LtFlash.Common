﻿using System.Collections.Generic;
using Rage;
using System.Linq;

namespace EvidenceLibrary
{
    internal static class Hospitals
    {
        private static readonly Dictionary<EHospitals, SpawnPoint> _hospitals = new Dictionary<EHospitals, SpawnPoint>
        {
            [EHospitals.CentralLSMC] = new SpawnPoint(225.987839f, new Vector3(342.7821f,-1470.94885f,28.855814f)),
            [EHospitals.MountZonahMC] = new SpawnPoint(20.5770454f, new Vector3(-486.764832f, -287.240051f, 34.97564f)),
            [EHospitals.PillbohHillMC] = new SpawnPoint(68.97214f, new Vector3(284.949524f, -570.552f, 42.6409836f)),
        };

        public static SpawnPoint GetClosestHospitalSpawn(Vector3 pos)
        {
            return _hospitals.Values.OrderBy(s => Vector3.Distance(pos, s.Position)).First();
        }

        public static SpawnPoint GetHospitalSpawn(EHospitals station)
        {
            return _hospitals[station];
        }
    }
}
