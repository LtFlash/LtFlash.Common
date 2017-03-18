using LtFlash.Common.EvidenceLibrary.BaseClasses;
using LtFlash.Common.EvidenceLibrary.Evidence;
using System;

namespace LtFlash.Common.EvidenceLibrary.Serialization
{
    public static class EvidenceFactory
    {
        public static DeadBody CreateDeadBody(DeadBodyData bd)
        {
            var b = new DeadBody(bd.ID, bd.Description, bd.Spawn, bd.Model);
            ApplyEvidenceProperies(b, bd);
            return b;
        }

        public static Witness CreateWitness(WitnessData wd, string[] dialog)
        {
            var wit = new Witness(wd.ID, wd.Description, wd.Spawn, wd.Model, dialog, wd.PickupPos);
            ApplyEvidenceProperies(wit, wd);
            ApplyWitnessProperties(wit, wd);
            return wit;
        }

        public static FirstOfficer CreateFirstOfficer(FirstOfficerData od, string[] dialog)
        {
            var o = new FirstOfficer(od.ID, od.Description, od.Spawn, dialog, od.Model);
            ApplyEvidenceProperies(o, od);
            return o;
        }

        public static Evidence.Object CreateEvidenceObject(ObjectData od)
        {
            var o = new Evidence.Object(od.ID, od.Description, od.Model, od.Spawn.Position);
            ApplyEvidenceProperies(o, od);
            ApplyObjectProperties(o, od);
            return o;
        }

        public static void ApplyEvidenceProperies(EvidenceBase evid, EvidenceData data)
        {
            if (data.ActivationDistance.HasValue) evid.ActivationDistance = data.ActivationDistance.Value;

            if (!string.IsNullOrEmpty(data.AdditionalTextWhileInspecting)) evid.AdditionalTextWhileInspecting = data.AdditionalTextWhileInspecting;

            evid.CanBeInspected = data.CanBeInspected;

            if (data.DistEvidenceClose.HasValue) evid.DistanceEvidenceClose = data.DistEvidenceClose.Value;

            evid.IsImportant = data.IsImportant;

            if (data.PlaySoundImportantCollected.HasValue) evid.PlaySoundImportantEvidenceCollected = data.PlaySoundImportantCollected.Value;

            if (data.PlaySoundNearby.HasValue) evid.PlaySoundPlayerNearby = data.PlaySoundNearby.Value;

            if (data.Traces?.Length > 0) Array.ForEach(data.Traces, t => evid.Traces.Add(t));
        }

        public static void ApplyWitnessProperties(Witness w, WitnessData data)
        {
            w.IsCompliant = data.IsCompliant;
        }

        public static void ApplyObjectProperties(Evidence.Object o, ObjectData data)
        {
            o.TextHelpWhileExamining = data.TextHelpWhileExamining;
        }
    }
}
