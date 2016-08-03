using System;
using System.Xml.Serialization;
using System.Windows.Forms;
using Rage;

namespace LtFlash.Common.Serialization
{
    public enum EAction
    {
        Action_1,
        Action_2,
        Action_3,
        Action_4,
        Action_5,
    }

    [XmlType(TypeName = "Control")]
    [Serializable]
    public class ControlsSerializeAdapter
    {
        public EAction Action;
        public Keys Key;
        public Keys Modifier;
        public ControllerButtons ControllerBtn;

        public ControlsSerializeAdapter(
            EAction action, 
            Keys key, Keys modifier, ControllerButtons ctrlButton)
        {
            Action = action;
            Key = key;
            Modifier = modifier;
            ControllerBtn = ctrlButton;
        }
    }

    [XmlType(TypeName = "Control")]
    [Serializable]
    public class ControlsSerializeAdapter<T>
    {
        public T Action;
        public Keys Key;
        public Keys Modifier;
        public ControllerButtons ControllerBtn;

        public ControlsSerializeAdapter(
            T action, 
            Keys key, Keys modifier, ControllerButtons ctrlButton) 
        {
            Action = action;
            Key = key;
            Modifier = modifier;
            ControllerBtn = ctrlButton;
        }
    }
}
