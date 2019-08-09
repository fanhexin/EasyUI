using UniRx.Async;
using UnityEngine;
using UnityEngine.UI;

namespace EasyUI
{
    public class DialogPanel : UIPanel
    {
        Image mask
        {
            get
            {
                var item = uiStack.transform.GetChild(transform.GetSiblingIndex() - 1);
                if (item == null)
                {
                    return null;
                }

                if (item.name != "Mask")
                {
                    return null;
                }

                return item.GetComponent<Image>();
            }
        }

        protected override async UniTask OnEnter()
        {
            Image mask;
            if (uiStack.topPanel is DialogPanel panel)
            {
                mask = panel.mask;
                mask.rectTransform.SetSiblingIndex(panel.transform.GetSiblingIndex());
            }
            else
            {
                mask = CreateMask();
                _defaultAnimation?.PlayDialogMaskEnterAnim(mask);
            }

            await base.OnEnter();
        }

        protected override async UniTask OnExit()
        {
            var m = mask;
            if (uiStack.topPanel is DialogPanel panel)
            {
                m.rectTransform.SetSiblingIndex(panel.transform.GetSiblingIndex());
            }
            else
            {
                if (_defaultAnimation == null)
                {
                    Destroy(m.gameObject); 
                }
                else
                {
                    _defaultAnimation.PlayDialogMaskExitAnim(m, () => Destroy(m.gameObject));
                }
            }
            
            await base.OnExit();
        }

        protected virtual Image CreateMask()
        {
            var go = new GameObject("Mask");
            var rectTrans = go.AddComponent<RectTransform>();
            rectTrans.SetParent(uiStack.transform);
            rectTrans.anchorMin = Vector2.zero;
            rectTrans.anchorMax = Vector2.one;
            rectTrans.offsetMin = Vector2.zero;
            rectTrans.offsetMax = Vector2.zero;
            rectTrans.SetSiblingIndex(transform.GetSiblingIndex());
            
            Image img = go.AddComponent<Image>();
            img.color = Settings.instance.dialogBkgColor;
            return img;
        }
    }
}