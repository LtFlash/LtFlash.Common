using LtFlash.Common.Processes;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Media;
using System.Windows.Forms;

namespace LtFlash.Common.EvidenceLibrary.BaseClasses
{
    public abstract class EvidenceBase : IHandleable
    {
        //PUBLIC
        public string Id { get; private set; }
        public string Description { get; private set; }
        public abstract Vector3 Position { get; }
        public bool Collected
        {
            get
            {
                return collected;
            }
            protected set
            {
                collected = value;
                if (IsImportant && PlaySoundImportantEvidenceCollected)
                    soundImportantEvidenceCollected.Play();
            }
        }
        public bool Checked { get; protected set; }
        public bool IsImportant { get; set; }
        public List<ETraces> Traces { get; } = new List<ETraces>();
        public float ActivationDistance
        {
            get { return DistanceEvidenceClose; }
            set { DistanceEvidenceClose = value; }
        }
        public bool CanBeInspected { get; set; } = true;
        public virtual bool IsPlayerClose
            => Vector3.Distance(PlayerPos, Position) <= DistanceEvidenceClose;

        
        public string AdditionalTextWhileInspecting { get; set; } = string.Empty;

        public bool PlaySoundPlayerNearby
        {
            set
            {
                if (value) ActivateStage(PlaySoundEvidenceNearby);
                else DeactivateStage(PlaySoundEvidenceNearby);
            }
        }
        public bool PlaySoundImportantEvidenceCollected { get; set; } = true;
         
        public SoundPlayer SoundPlayerNearby
            { set { soundEvidenceNearby = value; } }
        public SoundPlayer SoundImportantEvidenceCollected
            { set { soundImportantEvidenceCollected = value; } }

        public Blip Blip { get; set; }

        public Keys KeyInteract { get; set; } = Keys.I;
        public Keys KeyCollect { get; set; } = Keys.C;
        public Keys KeyLeave { get; set; } = Keys.L;

        public abstract PoolHandle Handle { get; }

        //PROTECTED
        protected abstract string TextInteractWithEvidence { get; }
        protected abstract string TextWhileInspecting { get; }

        protected Vector3 PlayerPos => Game.LocalPlayer.Character.Position;
        protected abstract Entity EvidenceEntity { get; }
        protected float DistanceEvidenceClose { get; set; } = 3f;

        //PRIVATE
        private SoundPlayer soundEvidenceNearby 
            = new SoundPlayer(Properties.Resources.EvidenceNearby);

        private SoundPlayer soundImportantEvidenceCollected 
            = new SoundPlayer(Properties.Resources.ImportantEvidenceCollected);

        private bool collected;
        private bool canRun = true;
        private Camera camera;
        private Camera gameCam;
        private bool prevState_CanBeActivated; // to play sounds
        private bool currState_IsPlayerClose;

        private const int CAM_INTERPOLATION_TIME = 3000;
        private const int SCREEN_FADE_TIME = 1000;
        private const int INFO_INTERACT_TIME = 100;

        private ProcessHost ProcHost = new ProcessHost();

        public EvidenceBase(string id, string description)
        {
            Id = id;
            Description = description;

            RegisterStages();
            ProcHost.Start();
        }

        private void RegisterStages()
        {
            AddStage(AwayOrClose);
            AddStage(Process);
            AddStage(InternalEnd);

            AddStage(PlaySoundEvidenceNearby);
            ActivateStage(PlaySoundEvidenceNearby);

            ActivateStage(AwayOrClose);
        }
        
        private void PlaySoundEvidenceNearby()
        {
            if (!CanBeInspected) return;

            currState_IsPlayerClose = IsPlayerClose;
            if(currState_IsPlayerClose != prevState_CanBeActivated)
            {
                prevState_CanBeActivated = currState_IsPlayerClose;
                if (currState_IsPlayerClose) soundEvidenceNearby.Play();
            }
        }

        public void CreateBlip(
            Color color, BlipSprite sprite = BlipSprite.Health,
            float scale = 0.25f)
        {
            RemoveBlip();

            Blip = new Blip(EvidenceEntity);
            Blip.Sprite = sprite;
            Blip.Color = color;
            Blip.Scale = scale;
        }

        public void RemoveBlip()
        {
            if (Blip.Exists()) Blip.Delete();
        }

        protected void AddStage(Action stage) => ProcHost.AddProcess(stage);

        protected void ActivateStage(Action stage) 
            => ProcHost.ActivateProcess(stage);

        protected void DeactivateStage(Action stage)
            => ProcHost.DeactivateProcess(stage);

        protected void SwapStages(Action toDisable, Action toEnable)
            => ProcHost.SwapProcesses(toDisable, toEnable);

        //protected - to SwapStage from derived classes
        protected void AwayOrClose()
        {
            if (!IsPlayerClose) return;
            if (!CanBeInspected) return;

            DisplayInfoInteractWithEvidence();

            if(Game.IsKeyDown(KeyInteract)) SwapStages(AwayOrClose, Process);
        }

        private void DisplayInfoInteractWithEvidence()
            => Game.DisplayHelp(TextInteractWithEvidence, INFO_INTERACT_TIME);

        protected abstract void Process();

        protected void SetEvidenceCollected()
        {
            Collected = true;
            DisplayInfoEvidenceCollected();
            SwapStages(Process, InternalEnd);
        }

        protected abstract void DisplayInfoEvidenceCollected();

        protected void SetEvidenceLeft() => SwapStages(Process, AwayOrClose);

        private void InternalEnd()
        {
            End();

            canRun = false;
            if (Blip.Exists()) Blip.Delete();
            soundEvidenceNearby.Dispose();
            soundImportantEvidenceCollected.Dispose();
        }

        protected abstract void End();

        protected void FocusCamOnObjectWithInterpolation(Vector3 camPos, Entity pointAt)
        {
            camera = new Camera(false);

            camera.Position = camPos;
            camera.PointAtEntity(pointAt, Vector3.Zero, false);

            gameCam = RetrieveGameCam();
            gameCam.Active = true;
            CamInterpolate(gameCam, camera, CAM_INTERPOLATION_TIME, true, true, true);
            camera.Active = true;

            SetLocalPlayerPropertiesWhileCamOn(true);
        }

        private void SetLocalPlayerPropertiesWhileCamOn(bool on)
        {
            NativeFunction.Natives.FreezeEntityPosition(Game.LocalPlayer.Character, on);

            Game.LocalPlayer.Character.IsInvincible = on;
        }

        protected void InterpolateCameraBack()
        {
            if (gameCam == null || camera == null) return;

            CamInterpolate(camera, gameCam, CAM_INTERPOLATION_TIME, true, true, true);

            camera.Active = false;
            camera.Delete();
            camera = null;

            gameCam.Delete();
            gameCam = null;

            SetLocalPlayerPropertiesWhileCamOn(false);
        }

        protected void DisableCustomCam()
        {
            Game.FadeScreenOut(SCREEN_FADE_TIME);

            if (camera.Exists())
            {
                camera.Active = false;
                camera.Delete();
            }

            Game.FadeScreenIn(SCREEN_FADE_TIME);

            SetLocalPlayerPropertiesWhileCamOn(false);
        }

        private Camera RetrieveGameCam()
        {
            Camera gamecam = new Camera(false);
            gamecam.FOV = NativeFunction.Natives.GET_GAMEPLAY_CAM_FOV<float>();
            gamecam.Position = NativeFunction.Natives.GET_GAMEPLAY_CAM_COORD<Vector3>();
            Vector3 rot = NativeFunction.Natives.GET_GAMEPLAY_CAM_ROT<Vector3>(0);
            //doesn't work with Rotator as a return val
            var rot1 = new Rotator(rot.X, rot.Y, rot.Z);
            gamecam.Rotation = rot1;

            gamecam.Heading = NativeFunction.Natives.GetGameplayCamRelativeHeading<float>();

            return gamecam;
        }

        private void CamInterpolate(
            Camera camfrom, Camera camto, 
            int totaltime, 
            bool easeLocation, bool easeRotation, bool waitForCompletion, 
            float x = 0f, float y = 0f, float z = 0f)
        {
            NativeFunction.Natives.SET_CAM_ACTIVE_WITH_INTERP(
                camto, camfrom, 
                totaltime, easeLocation, easeRotation);

            if (waitForCompletion) GameFiber.Sleep(totaltime);
        }

        protected string GetTextWhileInspectingWithAdditional()
            => AdditionalTextWhileInspecting == string.Empty ? 
            TextWhileInspecting :
            TextWhileInspecting + "~n~" + AdditionalTextWhileInspecting;

        public virtual void Dismiss()
        {
            RemoveBlip();
            canRun = false;
            ProcHost.Stop();
            Logging.Logger.LogDebug(nameof(EvidenceBase), nameof(Dismiss), "invoked");
        }

        public abstract bool IsValid();

        public bool Equals(IHandleable other) => ReferenceEquals(other, this);
    }
}
