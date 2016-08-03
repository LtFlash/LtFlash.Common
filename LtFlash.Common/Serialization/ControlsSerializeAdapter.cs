using System;
using System.Xml.Serialization;
using System.Windows.Forms;
using Rage;

namespace LtFlash.Common.Serialization
{
    [XmlType(TypeName = "Control")]
    [Serializable]
    public class ControlsSerializeAdapter<TEnum>
    {
        public TEnum Action;
        public Keys Key;
        public Keys Modifier;
        public ControllerButtons ControllerBtn;

        public ControlsSerializeAdapter(
            TEnum action, 
            Keys key, Keys modifier, ControllerButtons ctrlButton) 
        {
            Action = action;
            Key = key;
            Modifier = modifier;
            ControllerBtn = ctrlButton;
        }
    }
}
