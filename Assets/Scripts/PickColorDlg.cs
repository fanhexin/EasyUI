using System.Linq;
using Cysharp.Threading.Tasks;
using EasyUI;
using UnityEngine;
using UnityEngine.UI;

public class PickColorDlg : DialogPanel, IReturnValueProvider<Color>
{
    public Color returnValue { get; private set; }

    protected override UniTask OnExit()
    {
        var toggle = GetComponent<ToggleGroup>().ActiveToggles().First();
        if (toggle.name.Contains("Red"))
        {
            returnValue = Color.red;    
        }
        else if (toggle.name.Contains("Yellow"))
        {
            returnValue = Color.yellow;
        }
        else if (toggle.name.Contains("Blue"))
        {
            returnValue = Color.blue;
        }
        return base.OnExit();
    }
}
