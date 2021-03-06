﻿using System.Windows.Forms;
using Rage;
using LtFlash.Common.EvidenceLibrary.BaseClasses;
using System;

namespace LtFlash.Common.EvidenceLibrary.Evidence
{
    public class Object : EvidenceObject
    {
        //PUBLIC
        public string TextHelpWhileExamining { get; set; }

        public override string TextInteractWithEvidence
            => $"Press ~y~{KeyInteract} ~s~to examine the object.";
        protected override string TextWhileInspecting
            => $@"Press ~y~{keyRotate} ~s~to flip the object.~n~Press ~y~{KeyCollect} ~s~to include the item to the evidence.~n~Press ~y~{KeyLeave} ~s~to leave the object.";
        //PROTECTED
        protected Keys keyRotate = Keys.R;

        public Object(
            string id, string description, 
            Model model, Vector3 position) 
            : base(id, description, model, position)
        {
        }

        private enum EStages
        {
            InterpolateCam,
            ManipulateItem,
        }
        private EStages stage = EStages.InterpolateCam;

        protected override void Process()
        {
            switch (stage)
            {
                case EStages.InterpolateCam:

                    Vector3 camPos = new Vector3(Position.X, Position.Y, Position.Z + 0.25f);

                    FocusCamOnObjectWithInterpolation(camPos, @object);
                    Checked = true;
                    stage = EStages.ManipulateItem;

                    break;
                case EStages.ManipulateItem:

                    //Game.DisplayHelp(TextHelpWhileExamining);
                    Game.DisplayHelp(TextWhileInspecting);

                    if (Game.IsKeyDown(keyRotate))
                    {
                        @object.SetRotationRoll(MathHelper.RotateHeading(@object.Rotation.Roll, 180));
                    }
                    else if (Game.IsKeyDown(KeyCollect))
                    {
                        SetEvidenceCollected();

                        InterpolateCameraBack();
                        stage = EStages.InterpolateCam;
                    }
                    else if(Game.IsKeyDown(KeyLeave))
                    {
                        SetEvidenceLeft();

                        InterpolateCameraBack();
                        stage = EStages.InterpolateCam;
                    }

                    break;

                default:
                    break;
            }
        }
        
        protected override void DisplayInfoEvidenceCollected()
        {
            Game.DisplayNotification($"Object: ~y~{Description}~w~ has been added to the evidence.");
        }

        protected override void End()
        {
            if(@object) @object.Delete();
            RemoveBlip();
        }
    }
}
