using Rage;

namespace LtFlash.Common.EvidenceLibrary.Evidence
{
    public class FirstOfficer : Witness
    {
        protected override string TextInteractWithEvidence
            => $"Press ~y~{KeyInteract}~s~ to talk to the first officer at scene.";

        protected override string TextWhileInspecting => string.Empty;

        public FirstOfficer(
            string id, string description, 
            SpawnPoint spawn, string[] dialog, string model = "s_m_y_cop_01")
            : base(id, description, spawn, model, dialog, Vector3.Zero)
        {
        }

        protected override void WaitForFurtherInstruction()
        {
        }
    }
}
