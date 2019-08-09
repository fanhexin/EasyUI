using System;
using System.Threading.Tasks;
using EasyUI;
using UniRx.Async;
using UnityEngine;

public class TestPanel : UIPanel
{
    [SerializeField] Transition _toBluePanel;

    protected override async UniTask OnEnter()
    {
        await base.OnEnter();
        if (_toBluePanel != null && !string.IsNullOrEmpty(_toBluePanel.destPanelName))
        {
            DelayTransition();
        }
    }

    async Task DelayTransition()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(3));
        DoTransition(_toBluePanel);
    }
}
