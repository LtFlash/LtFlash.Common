using Rage;
using System;
using System.Media;
using Rage.Native;
using System.Timers;
using System.Drawing;
using Forms = System.Windows.Forms;

namespace LtFlash.Common.ScriptManager.Scripts
{
    public abstract class CalloutScriptBase : ScriptBase, IScript
    {
        //PUBLIC
        public float DistanceSoundPlayerClosingIn { get; set; } = 80f;
        public bool PlaySoundPlayerClosingIn
        {
            set
            {
                if (value) ActivateStage(PlaySoundWhenPlayerNearby);
                else DeactivateStage(PlaySoundWhenPlayerNearby);
            }
        }
        public SoundPlayer SoundPlayerClosingIn
            { set { soundPlayerClosingIn = value; } }

        //PROTECTED
        protected Forms.Keys KeyAcceptCallout { get; set; } = Forms.Keys.Y;
        protected double Timeout
            { set { callNotAcceptedTimer.Interval = value; } }

        //PRIVATE
        private const double TIME_CALL_NOT_ACCEPTED = 10000;
        private const float RADAR_ZOOM_LEVEL = 200f;
        private const float BLIP_ALPHA = 0.3f;

        private Color blipAreaColor = Color.Blue;
        private SoundPlayer soundPlayerClosingIn 
            = new SoundPlayer(Properties.Resources.CaseApproach);
        private Timer callNotAcceptedTimer = new Timer(TIME_CALL_NOT_ACCEPTED);
        private bool timeElapsed;
        private bool zoomOutMinimap;
        private Blip blipArea;
        private Blip blipRoute;
        private float blipRouteRadius;
        private Vector3 blipRoutePosition;
        private Vector3 callPosition;

        public CalloutScriptBase()
        {
            RegisterStages();
        }

        private void RegisterStages()
        {
            AddStage(InternalInitialize);
            AddStage(WaitForAcceptKey);
            AddStage(InternalAccepted);
            AddStage(Process);
            AddStage(InternalNotAccepted);
            AddStage(InternalEnd);
            AddStage(RemoveAreaWhenClose);

            AddStage(PlaySoundWhenPlayerNearby);

            ActivateStage(InternalInitialize);
        }

        private void PlaySoundWhenPlayerNearby()
        {
            if (Vector3.Distance(PlayerPos, callPosition)
                <= DistanceSoundPlayerClosingIn)
            {
                soundPlayerClosingIn.Play();
                DeactivateStage(PlaySoundWhenPlayerNearby);
            }
        }

        private void InternalInitialize()
        {
            if (!Initialize())
            {
                SwapStages(InternalInitialize, InternalEnd);
                return;
            }

            callNotAcceptedTimer.Start();
            callNotAcceptedTimer.Elapsed += (s, args) => timeElapsed = true;

            SwapStages(InternalInitialize, WaitForAcceptKey);
        }


        protected void ShowAreaBlip(
            Vector3 position, float radius, 
            bool zoomOutMinimap = true, bool flashMinimap = true)
        {
            blipArea = new Blip(position, radius);
            blipArea.Color = blipAreaColor;
            blipArea.Alpha = BLIP_ALPHA; //max val == 1f

            callPosition = position;

            this.zoomOutMinimap = zoomOutMinimap;
            if (flashMinimap) FlashMinimap();
        }

        protected void ShowAreaWithRoute(
            Vector3 position, float radius, 
            Color color)
        {
            blipRoute = new Blip(position, radius);
            blipRoute.Alpha = BLIP_ALPHA;
            blipRoute.Color = color;
            blipRoute.EnableRoute(color);

            blipRouteRadius = radius;
            blipRoutePosition = position;

            ActivateStage(RemoveAreaWhenClose);
        }

        protected void RemoveAreaBlipWithRoute()
        {
            if (blipRoute.Exists()) blipRoute.Delete();
        }

        private void RemoveAreaWhenClose()
        {
            if(PlayerPos.DistanceTo(blipRoutePosition) <= blipRouteRadius)
            {
                RemoveAreaBlipWithRoute();
                DeactivateStage(RemoveAreaWhenClose);
            }
        }

        private void SetMinimapZoom(int zoomLevel)
            => NativeFunction.Natives.SetRadarZoom(zoomLevel);

        private void FlashMinimap()
            => NativeFunction.Natives.FlashMinimapDisplay();

        private void RemoveAreaBlip()
        {
            if (blipArea.Exists()) blipArea.Delete();
            SetMinimapZoom(0);
        }

        protected void DisplayCalloutInfo(string text)
            => Game.DisplayNotification(text);

        public void DisplayCalloutInfo(
            string textureDictionaryName, string textureName,
            string title, string subtitle, string text)
            => Game.DisplayNotification(textureDictionaryName, textureName, title, subtitle, text);

        private void WaitForAcceptKey()
        {
            if(Game.IsKeyDown(KeyAcceptCallout))
            {
                RemoveAreaBlip();
                SwapStages(WaitForAcceptKey, InternalAccepted);
                return;
            }

            if(blipArea.Exists() && zoomOutMinimap)
            {
                Game.SetRadarZoomLevelThisFrame(RADAR_ZOOM_LEVEL);
            }
            
            if(timeElapsed)
            {
                RemoveAreaBlip();
                SwapStages(WaitForAcceptKey, InternalNotAccepted);
                callNotAcceptedTimer.Dispose();
            }
        }

        private void InternalAccepted()
            => SwapStages(InternalAccepted, (Accepted() ? (Action)Process : InternalEnd));


        private void InternalNotAccepted()
        {
            NotAccepted();
            SetScriptFinished(false);
        }

        private void InternalEnd()
        {
            soundPlayerClosingIn.Dispose();
            RemoveAreaBlipWithRoute();
            RemoveAreaBlip();
            End();
            Stop(); 
        }

        public void SetScriptFinished(bool completed)
        {
            HasFinished = true;
            Completed = completed;
            InternalEnd();
        }

        protected abstract bool Accepted();
        protected abstract void NotAccepted();
    }
}
