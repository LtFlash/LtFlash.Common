using System;
using System.Collections.Generic;
using LtFlash.Common.Serialization;

namespace LtFlash.Common.InputHandling
{
    internal class Input<TEnum> where TEnum : struct, IConvertible
    {
        private Dictionary<TEnum, ControlSet> Controls 
            = new Dictionary<TEnum, ControlSet>();

        public Input(string path)
        {
            if (!typeof(TEnum).IsEnum)
            {
                throw new ArgumentException($"{nameof(Input<TEnum>)}: TEnum must be an enumerated type");
            }

            Controls = Serializer.DeserializeControls<TEnum>(path);
        }

        public void SaveConfig(string path)
        {
            Serializer.SerializeControls(Controls, path);
        }

        public bool GetControlStatus(TEnum action)
        {
            return Controls[action].IsActive;
        }

        public string GetControlDescription(TEnum action)
        {
            return Controls[action].Description;
        }
    }
}