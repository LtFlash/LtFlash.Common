using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;

namespace LtFlash.Common.EvidenceLibrary.Services
{
    class Coroner_ : ServiceBase
    {
        private Ped body;

        private static SpawnPoint _coronersOffice = new SpawnPoint(270.346252f, new Vector3(218.361008f, -1381.16431f, 30.1247978f));

        public Coroner_(
            Ped body, 
            SpawnPoint dispatchTo, 
            string[] dialogue,
            bool spawnAtScene = false) : 
            base(
                GetVehModel(), 
                GetPedModel(), 
                GetPedModel(), 
                spawnAtScene ? dispatchTo : _coronersOffice, 
                dispatchTo, 
                dialogue)
        {
            this.body = body;
        }

        private static string GetPedModel()
        {
            string[] _pedModels = new string[]
            {
                "s_m_m_paramedic_01",
            };
            return _pedModels[MathHelper.GetRandomInteger(_pedModels.Length)];
        }

        private static string GetVehModel()
        {
            string[] _meVan = new string[]
            {
                "burrito3",
                "youga",
            };
            return _meVan[MathHelper.GetRandomInteger(_meVan.Length)];
        }

        protected override void PostSpawn()
        {
        }

        protected override void PostArrival()
        {
        }

        private void GoToBody()
        {
            PedWorker.Tasks.GoToOffsetFromEntity(body, 1f, 0f, 1f);
            PedDriver.Tasks.GoToOffsetFromEntity(body, 4f, 8f, 1f);
        }

        public override void Dispose()
        {
        }
    }
}
