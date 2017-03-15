using System.Collections.Generic;
using System.Linq;
using Rage;
using LtFlash.Common.Processes;

namespace LtFlash.Common.EvidenceLibrary.Evidence
{
    public class EvidenceController
    {
        public List<IEvidence> Evidence { get; private set; } = new List<IEvidence>();

        private ProcessHost proc = new ProcessHost();
        private const int INFO_INTERACT_TIME = 100;
        private IEvidence closest;
        private bool inspecting;
        private bool canPlaySound;

        public EvidenceController()
        {
            proc.ActivateProcess(Process);
            proc.Start();
        }

        public void AddEvidence(IEvidence evidence)
        {
            evidence.CanBeInspected = false;
            evidence.PlaySoundPlayerNearby = false;
            Evidence.Add(evidence);
        }
        private void Process()
        {
            if(!inspecting)
            {
                if (Evidence.Count < 1) return;

                RemoveCollectedEvidence();

                closest = GetClosestEvidence();

                if (closest == null) return;

                if (!IsEvidenceWithinDetectionDistance(closest))
                {
                    canPlaySound = true;
                    return;
                }

                PlaySoundEvidenceNearby(closest);

                Game.DisplayHelp(closest.TextInteractWithEvidence, INFO_INTERACT_TIME);

                if (!Game.IsKeyDown(closest.KeyInteract))
                {
                    closest.PlaySoundPlayerNearby = false;
                    return;
                }

                closest.Interact();
                inspecting = true;
            }
            else
            {
                if(DoesPlayerQuitInspecting(closest)) inspecting = false;
            }
        }

        private bool DoesPlayerQuitInspecting(IEvidence ev)
            => Game.IsKeyDown(closest.KeyCollect) ||
               Game.IsKeyDown(closest.KeyLeave);

        private void PlaySoundEvidenceNearby(IEvidence e)
        {
            if(canPlaySound)
            {
                canPlaySound = false;
                e.SoundPlayerNearby.Play();
            }
        }

        private void RemoveCollectedEvidence()
            => Evidence.RemoveAll(e => e.IsCollected);

        private IEvidence GetClosestEvidence()
            => Evidence.OrderBy(e => DistToPlayer(e.Position)).FirstOrDefault();

        private bool IsEvidenceWithinDetectionDistance(IEvidence e)
            => DistToPlayer(e.Position) <= e.DistanceEvidenceClose;

        private float DistToPlayer(Vector3 pos)
            => Vector3.Distance(pos, Game.LocalPlayer.Character.Position);

        public void Dispose() => proc.Stop();
    }
}
