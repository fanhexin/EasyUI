using System.Collections.Generic;
using System.Linq;
using UniRx.Async;
using UnityEngine;
using EasyUI;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "PrefabPanelFactory", menuName = "EasyUI/PrefabPanelFactory")]
public class PrefabPanelFactory : PanelFactory
{
    [SerializeField] UIPanel[] _panels;

    Dictionary<string, UIPanel> _panelsDic;

    void OnEnable()
    {
#if UNITY_EDITOR            
        if (!EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }
#endif
        _panelsDic = _panels.ToDictionary(x => x.name);
    }

    public override async UniTask<UIPanel> CreatePanelAsync(string name)
    {
        if (!_panelsDic.TryGetValue(name, out var prefab))
        {
            return null;
        }

        return Instantiate(prefab);
    }

    public override void RecyclePanel(UIPanel panel)
    {
        Destroy(panel.gameObject);
    }
}
