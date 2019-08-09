using System;
using DG.Tweening;
using EasyUI;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "DefaultTweenAnimation", menuName = "EasyUI/DefaultTweenAnimation")]
public class DefaultTweenAnimation : DefaultAnimation
{
    [SerializeField] float _duration = 0.6f;
    
    public override void PlayEnterAnim(UIPanel panel, Action complete = null)
    {
        EnterAnim(panel.transform, _duration, complete);
    }

    public override void PlayExitAnim(UIPanel panel, Action complete = null)
    {
        ExitAnim(panel.transform, _duration, complete);
    }

    public override void PlayDialogMaskEnterAnim(Image mask, Action complete = null)
    {
        EnterAnim(mask.transform, _duration, complete);
    }

    public override void PlayDialogMaskExitAnim(Image mask, Action complete = null)
    {
        ExitAnim(mask.transform, _duration, complete);
    }

    void EnterAnim(Transform transform, float duration, Action complete)
    {
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, duration).OnComplete(() => complete?.Invoke());
    }

    void ExitAnim(Transform transform, float duration, Action complete)
    {
        transform.localScale = Vector3.one;
        transform.DOScale(Vector3.zero, duration).OnComplete(() => complete?.Invoke());
    }
}
