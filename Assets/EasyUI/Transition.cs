using System;
using UniRx.Async;
using UnityEngine;
using UnityEngine.UI;

namespace EasyUI
{
    [Serializable]
    public class Transition
    {
        public enum Operation
        {
            Push,
            Replace
        }
        
        [SerializeField, PanelName] string _destPanelName;
        [SerializeField] bool _disableUnderPanel;
        [SerializeField] Operation _operation;

        public string destPanelName => _destPanelName;
        public bool disableUnderPanel => _disableUnderPanel;
        public Operation operation => _operation;
    }

    [Serializable]
    public class BindingTransition : Transition
    {
        [SerializeField] Button _triggerBtn;

        public Button triggerBtn => _triggerBtn;
    }
}