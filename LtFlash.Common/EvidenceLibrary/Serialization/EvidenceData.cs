using System;
using Rage;

namespace LtFlash.Common.EvidenceLibrary.Serialization
{
    public interface IIdentifiable
    {
        string ID { get; }
    }

    public class EvidenceData : IIdentifiable
    {
        public string ID { get; set; }
        public string Description;
        public bool IsImportant;
        public SpawnPoint Spawn;
        public float? ActivationDistance;
        public bool CanBeInspected;

        public string AdditionalTextWhileInspecting;
        public bool ShouldSerializeAdditionalTextWhileInspecting()
            => !string.IsNullOrEmpty(AdditionalTextWhileInspecting);

        public bool? PlaySoundImportantCollected;
        public bool ShouldSerializePlaySoundImportantCollected()
            => PlaySoundImportantCollected.HasValue;

        public bool? PlaySoundNearby;
        //public Keys KeyInteract;
        //public Keys KeyCollect;
        //public Keys KeyLeave;
        public float? DistEvidenceClose;
        public bool ShouldSerializeDistEvidenceClose() => DistEvidenceClose.HasValue;

        public ETraces[] Traces { get; set; }

        public EvidenceData()
        {
        }

        public EvidenceData(EvidenceData c) : this()
        {
            ID = c.ID;
            Description = c.Description;
            IsImportant = c.IsImportant;
            Spawn = c.Spawn;
            ActivationDistance = c.ActivationDistance;
            CanBeInspected = c.CanBeInspected;
            AdditionalTextWhileInspecting = c.AdditionalTextWhileInspecting;
            PlaySoundImportantCollected = c.PlaySoundImportantCollected;
            PlaySoundNearby = c.PlaySoundNearby;
            DistEvidenceClose = c.DistEvidenceClose;
            Traces = c.Traces;
        }
    }

    public class EvidenceObjectData : EvidenceData
    {
        public string Model;
        public EvidenceObjectData(EvidenceObjectData e) : base(e)
        {
            Model = e.Model;
        }
    }

    public class ObjectData : EvidenceObjectData
    {
        public string TextHelpWhileExamining { get; set; }
        public ObjectData(ObjectData o) : base(o)
        {
            TextHelpWhileExamining = o.TextHelpWhileExamining;
        }
    }

    public class EvidencePedData : EvidenceData
    {
        public string Model;
        public EvidencePedData(EvidencePedData e) : base(e)
        {
            Model = e.Model;
        }
    }

    public class DeadBodyData : EvidencePedData
    {
        public DeadBodyData(DeadBodyData d) : base(d) 
        {

        }
    }

    public class WitnessData : EvidencePedData
    {
        public bool IsCompliant { get; set; } //true -> create transport
        public string DialogID { get; set; }
        public string DialogRefuseTransportID { get; set; }
        public Vector3 PickupPos { get; set; }
        public WitnessData(WitnessData w) : base(w)
        {
            IsCompliant = w.IsCompliant;
            DialogID = w.DialogID;
            DialogRefuseTransportID = w.DialogRefuseTransportID;
            PickupPos = w.PickupPos;
        }
    }

    public class FirstOfficerData : WitnessData
    {
        public FirstOfficerData(FirstOfficerData o) : base(o)
        {

        }
    }

    public class DialogData : IIdentifiable
    {
        public string ID { get; set; }
        public string[] Dialog;
    }
}
