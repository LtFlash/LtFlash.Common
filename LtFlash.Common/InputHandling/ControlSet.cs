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

        private string CTAG = "~g~";
        private const string RTAG = "~s~"; //color reset

        public ControlSet(Keys key, Keys mod, ControllerButtons ctrlBtn)
        {
            Key = key;
            Modifier = mod;
            ControllerBtn = ctrlBtn;

            Description = GetDescription();
        }

        public ControlSet(
            Keys key, Keys mod, ControllerButtons ctrlBtn, 
            string colorTagOfDescription) : this(key, mod, ctrlBtn)
        {
            CTAG = colorTagOfDescription;
        }

        private string GetDescription()
        {
            string result;

            result = Modifier == Keys.None ?
                $"{CTAG}{Key.ToString()}{RTAG}" :
                $"{CTAG}{Modifier.ToString()}{RTAG} + {CTAG}{Key.ToString()}{RTAG}";


            if (Game.IsControllerConnected && ControllerBtn != ControllerButtons.None)
            {
                result += $" or {CTAG}{ControllerBtn.ToString()}{RTAG}";
            }

            return result;
        }

        private bool _IsActive()
        {
            return AreKeyboardControlsActive() || AreControllerControlsActive();
        }

        private bool AreKeyboardControlsActive()
        {
            return Modifier == Keys.None ? 
                Game.IsKeyDown(Key) : 
                Game.IsKeyDownRightNow(Modifier) && Game.IsKeyDown(Key);
        }

        private bool AreControllerControlsActive()
        {
            if (!Game.IsControllerConnected || 
                ControllerBtn == ControllerButtons.None) return false;

            return Game.IsControllerButtonDown(ControllerBtn);
        }
    }
}
