using UniRx.Async;
using UnityEngine;

namespace EasyUI
{
    public abstract class PanelFactory : ScriptableObject
    {
        public abstract UniTask<UIPanel> CreatePanelAsync(string name);
        public abstract void RecyclePanel(UIPanel panel);
    }
}