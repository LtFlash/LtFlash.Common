using System.Collections.Generic;
using LtFlash.Common.Serialization;

namespace LtFlash.Common.InputHandling
{
    internal class Input<T>
    {
        private Dictionary<T, ControlSet> Controls 
            = new Dictionary<T, ControlSet>();

        public Input(string path)
        {
            Controls = Serializer.DeserializeControls<T>(path);
        }

        public void SaveConfig(string path)
        {
            Serializer.SerializeControls(Controls, path);
        }

        public bool GetControlStatus(T action)
        {
            return Controls[action].IsActive;
        }

        public string GetControlDescription(T action)
        {
            return Controls[action].Description;
        }
    }
}