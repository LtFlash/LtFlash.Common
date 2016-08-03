using Rage;
using LtFlash.Common.EvidenceLibrary.BaseClasses;

namespace LtFlash.Common.EvidenceLibrary.Evidence
{
    public class DeadBody : EvidenceBody
    {
        public DeadBody(string id, string description, SpawnPoint spawn, Model model) : base(id, description, spawn, model)
        {

        }

        protected override void DisplayInfoEvidenceCollected()
        {
            Game.DisplayNotification("Body has beed inspected!");
        }
    }
}
