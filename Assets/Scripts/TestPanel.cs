using System;
using System.Threading.Tasks;
using EasyUI;
using UniRx.Async;
using UnityEngine;
using UnityEngine.UI;

public class TestPanel : UIPanel, IParameterReceiver<Color>
{
    [SerializeField] Transition _toGreenPanel;
    [SerializeField] Transition _toPickColorDlg;
    [SerializeField] Image _bkgImg;
    [SerializeField] Button _toPickColorDlgBtn;

    protected override async UniTask OnEnter()
    {
        await base.OnEnter();
        if (_toGreenPanel != null && !string.IsNullOrEmpty(_toGreenPanel.destPanelName))
        {
            DelayTransition();
        }

        if (_toPickColorDlg != null)
        {
            _toPickColorDlgBtn.onClick.AddListener(async () =>
            {
                Color color = await uiStack.DoTransition<Color>(_toPickColorDlg);
                _bkgImg.color = color;
            });
        }
    }

    async Task DelayTransition()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(3));
        uiStack.DoTransition(_toGreenPanel, Color.green);
    }

    public void InputParameter(Color bgColor)
    {
        _bkgImg.color = bgColor;
    }
}
