using System.Windows.Forms;
using Rage;
using System.Xml.Serialization;
using System.Xml;
using System;

namespace LtFlash.Common.InputHandling
{
    public class ControlSet
    {
        //PUBLIC
        public Keys Key { get; set; }
        public Keys Modifier { get; set; }
        public ControllerButtons ControllerBtn { get; set; }

        public bool IsActive => _IsActive();
        public string Description { get; private set; }
        public string ColorTag { set => CTAG = value; }

        //PRIVATE
        private string CTAG = "~g~";
        private const string RTAG = "~s~"; //color reset

        public ControlSet() { }

        public ControlSet(Keys key, Keys modifier, ControllerButtons ctrlBtn)
        {
            Key = key;
            Modifier = modifier;
            ControllerBtn = ctrlBtn;

            Description = GetDescription();
        }

        public ControlSet(
            Keys key, Keys mod, ControllerButtons ctrlBtn, 
            string colorTagOfDescription) : 
            this(key, mod, ctrlBtn)
        {
            CTAG = colorTagOfDescription;
        }

        private string GetDescription()
        {
            string result = Modifier == Keys.None ?
                $"{CTAG}{Key.ToString()}{RTAG}" :
                $"{CTAG}{Modifier.ToString()}{RTAG} + {CTAG}{Key.ToString()}{RTAG}";


            if (Game.IsControllerConnected && ControllerBtn != ControllerButtons.None)
            {
                result += $" or {CTAG}{ControllerBtn.ToString()}{RTAG}";
            }

            return result;
        }

        private bool _IsActive()
            => AreKeyboardControlsActive() || AreControllerControlsActive();

        private bool AreKeyboardControlsActive()
            => Modifier == Keys.None ? Game.IsKeyDown(Key) : 
            Game.IsKeyDownRightNow(Modifier) && Game.IsKeyDown(Key);

        private bool AreControllerControlsActive()
        {
            if (!Game.IsControllerConnected || 
                ControllerBtn == ControllerButtons.None) return false;

            return Game.IsControllerButtonDown(ControllerBtn);
        }

        public static implicit operator bool(ControlSet ctrlSet) => ctrlSet.IsActive;
    }
}
