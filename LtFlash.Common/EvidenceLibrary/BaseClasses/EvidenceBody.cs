﻿using Rage;
using System.Windows.Forms;

namespace LtFlash.Common.EvidenceLibrary.BaseClasses
{
    public abstract class EvidenceBody : EvidencePed
    {
        public override string TextInteractWithEvidence
            => $"Press ~y~{KeyInteract}~s~ to inspect the body.";

        protected override string TextWhileInspecting
            => $"Press ~y~{KeyLeave}~s~ to quit inspecting the body.";
        protected Keys _keyRotate = Keys.R;
        
        public EvidenceBody(
            string id, string description, 
            SpawnPoint spawn, Model model) :
            base(id, description, spawn, model)
        {
            Ped.Kill();
        }

        private enum EState
        {
            InterpolateCam,
            InspectingEvidence,
            InterpolateCamBack,
        }
        private EState _state = EState.InterpolateCam;

        protected override void Process()
        {
            if (!Ped)
            {
                IsCollected = true;
                Dismiss();
            }

            switch (_state)
            {
                case EState.InterpolateCam:

                    Vector3 camPos = new Vector3(Position.X, Position.Y, Position.Z + 0.75f);
                    FocusCamOnObjectWithInterpolation(camPos, Ped);

                    _state = EState.InspectingEvidence;

                    break;

                case EState.InspectingEvidence:

                    Game.DisplayHelp(GetTextWhileInspectingWithAdditional(), 100);
                    
                    if (Game.IsKeyDown(KeyLeave))
                    {
                        _state = EState.InterpolateCamBack;
                    }

                    break;

                case EState.InterpolateCamBack:

                    InterpolateCameraBack();
                    Checked = true;

                    SwapStages(Process, AwayOrClose);

                    _state = EState.InterpolateCam;
                    break;

                default:
                    break;
            }
        }
    }
}
