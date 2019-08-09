using System;
using System.Threading.Tasks;
using EasyUI;
using UniRx.Async;
using UnityEngine;
using UnityEngine.UI;

public class TestPanel : UIPanel, IParameterReceiver<Color>
{
    [SerializeField] Transition _toGreenPanel;
    [SerializeField] Image _bkgImg;

    protected override async UniTask OnEnter()
    {
        await base.OnEnter();
        if (_toGreenPanel != null && !string.IsNullOrEmpty(_toGreenPanel.destPanelName))
        {
            DelayTransition();
        }
    }

    async Task DelayTransition()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(3));
        DoTransition(_toGreenPanel, Color.green);
    }

    public void InputParameter(Color bgColor)
    {
        _bkgImg.color = bgColor;
    }
}
