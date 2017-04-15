using System.Collections.Generic;
using Rage;
using System.Windows.Forms;
using System.Media;

namespace LtFlash.Common.EvidenceLibrary
{
    public interface IDialog
    {
        bool HasEnded { get; }
        void StartDialog();
        void StartDialog(Ped p1, Ped p2);
    }

    public interface ICollectable
    {
        bool IsCollected { get; }
    }

    public interface IEvidence : ICollectable
    {
        string Id { get; }
        string Description { get; }
        bool Checked { get; }
        bool IsImportant { get; }
        List<ETraces> Traces { get; }
        float DistanceEvidenceClose { get; set; }
        Vector3 Position { get; }
        string TextInteractWithEvidence { get; }
        bool PlaySoundPlayerNearby { set; }
        SoundPlayer SoundPlayerNearby { get; set; }
        bool CanBeInspected { get; set; }
        void Interact();
        void Dismiss();


        Keys KeyInteract { get; set; }
        Keys KeyCollect { get; set; }
        Keys KeyLeave { get; set; }
    }
}
