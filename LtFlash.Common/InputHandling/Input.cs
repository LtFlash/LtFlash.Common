using System;
using System.Collections.Generic;
using LtFlash.Common.Serialization;

namespace LtFlash.Common.InputHandling
{
    internal class Input<TEnum> where TEnum : struct, IConvertible
    {
        public ControlSet this[TEnum id]
        {
            get
            {
                return Controls[id];
            }
        }

        private Dictionary<TEnum, ControlSet> Controls 
            = new Dictionary<TEnum, ControlSet>();

        private string _path;

        public Input(string pathToLoadFrom) : this()
        {
            Controls = Serializer.DeserializeControls<TEnum>(pathToLoadFrom);
            _path = pathToLoadFrom;
        }

        public Input()
        {
            if (!typeof(TEnum).IsEnum)
            {
                throw new ArgumentException(
                    $"{nameof(Input<TEnum>)}: TEnum must be an enumerated type");
            }
        }

        public void SaveConfig()
        {
            SaveConfig(_path);
        }

        public void SaveConfig(string pathToSaveTo)
        {
            Serializer.SerializeControls(Controls, pathToSaveTo);
        }

        public bool GetControlStatus(TEnum action)
        {
            return Controls[action].IsActive;
        }

        public string GetControlDescription(TEnum action)
        {
            return Controls[action].Description;
        }

        public void AddContolSet(TEnum action, ControlSet ctrlSet)
        {
            Controls.Add(action, ctrlSet);
        }
    }
}