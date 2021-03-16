using UniRx;
using UniRx.Async;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace EasyUI
{
    public class DialogPanel : UIPanel
    {
        [SerializeField] bool _tapMaskClose;

        bool _beCovered;
        bool _beDisabled;

        protected Image GetMask()
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

        protected override async UniTask OnEnter()
        {
            Image mask;
            if (uiStack.topPanel is DialogPanel panel)
            {
                mask = panel.GetMask();
                mask.rectTransform.SetSiblingIndex(panel.transform.GetSiblingIndex());
            }
            else
            {
                mask = CreateMask();
                uiStack.defaultAnimation?.PlayDialogMaskEnterAnim(mask);
            }

            if (_tapMaskClose)
            {
                mask.OnPointerClickAsObservable()
                    .Subscribe(_ =>
                    {
                        if (uiStack.topPanel == this)
                            Close();
                    }).AddTo(this);
            }

            await base.OnEnter();
        }

        protected override async UniTask OnExit()
        {
            var m = GetMask();
            if (uiStack.topPanel is DialogPanel panel)
            {
                m.rectTransform.SetSiblingIndex(panel.transform.GetSiblingIndex());
            }
            else
            {
                if (uiStack.defaultAnimation == null)
                {
                    Destroy(m.gameObject); 
                }
                else
                {
                    uiStack.defaultAnimation.PlayDialogMaskExitAnim(m, () => Destroy(m.gameObject));
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
            rectTrans.localScale = Vector3.one;
            rectTrans.SetSiblingIndex(transform.GetSiblingIndex());
            
            Image img = go.AddComponent<Image>();
            img.color = Settings.instance.dialogBkgColor;
            return img;
        }

        protected override UniTask OnEnterBackground(UIPanel pushPanel)
        {
            _beCovered = true;
            return base.OnEnterBackground(pushPanel);
        }

        protected override UniTask OnEnterForeground(UIPanel popPanel)
        {
            _beCovered = false;
            return base.OnEnterForeground(popPanel);
        }

        protected virtual void OnEnable()
        {
            if (!_beDisabled)
            {
                return;
            }

            _beDisabled = false;
            SetMaskAndUnderPanelActive(true);
        }

        protected virtual void OnDisable()
        {
            if (!_beCovered)
            {
                return;
            }
            
            _beDisabled = true;
            SetMaskAndUnderPanelActive(false);
        }

        void SetMaskAndUnderPanelActive(bool b)
        {
            int index = transform.GetSiblingIndex();
            uiStack.transform.GetChild(index - 1).gameObject.SetActive(b);
            
            index -= 2;
            if (index < 0)
            {
                return;
            }
            uiStack.transform.GetChild(index).gameObject.SetActive(b);
        }
    }
}