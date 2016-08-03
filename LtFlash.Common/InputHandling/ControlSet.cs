using System.Windows.Forms;
using Rage;

namespace LtFlash.Common.InputHandling
{
    public class ControlSet
    {
        public Keys Key { get; private set; }
        public Keys Modifier { get; private set; }
        public ControllerButtons ControllerBtn { get; private set; }

        public bool IsActive { get { return _IsActive(); } }
        public string Description { get; private set; }

        private const string CTAG = "~g~";
        private const string RTAG = "~s~"; //color reset

        public ControlSet(Keys key, Keys mod, ControllerButtons ctrlBtn)
        {
            Key = key;
            Modifier = mod;
            ControllerBtn = ctrlBtn;

            Description = GetDescription();
        }

        private string GetDescription()
        {
            string result;

            if (Modifier == Keys.None)
            {
                result = Key.ToString();
            }
            else
            {
                //TODO: add color tags
                result = Modifier.ToString() + " + " + Key.ToString();
            }
            //ControllerButtons.RightShoulder
            if (Game.IsControllerConnected && ControllerBtn != ControllerButtons.None)
            {
                result += "~s~ or ~g~" + ControllerBtn.ToString();
            }

            return result;
        }

        private bool _IsActive()
        {
            return AreKeyboardControlsActive() || AreControllerControlsActive();
        }

        private bool AreKeyboardControlsActive()
        {
            if (Modifier == Keys.None)
            {
                if (Game.IsKeyDown(Key))
                    return true;
            }
            else
            {
                if (Game.IsKeyDownRightNow(Modifier) &&
                    Game.IsKeyDown(Key))
                    return true;
            }

            return false;
        }

        private bool AreControllerControlsActive()
        {
            if (!Game.IsControllerConnected ||
                ControllerBtn == ControllerButtons.None)
                return false;

            if (Game.IsControllerButtonDown(ControllerBtn))
                return true;

            return false;
        }
    }
}
