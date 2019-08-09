using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniRx;
using UniRx.Async;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EasyUI
{
    [RequireComponent(typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster))]
    public class UIStack : MonoBehaviour
    {
        [SerializeField] PanelFactory _panelFactory;
        [SerializeField] DefaultAnimation _defaultAnimation;

        public PanelFactory panelFactory => _panelFactory;
        public DefaultAnimation defaultAnimation => _defaultAnimation;
        
        Subject<UIPanel> _beginPushSubject;
        public IObservable<UIPanel> onBeginPush => _beginPushSubject ?? (_beginPushSubject = new Subject<UIPanel>());

        Subject<UIPanel> _endPushSubject;
        public IObservable<UIPanel> onEndPush => _endPushSubject ?? (_endPushSubject = new Subject<UIPanel>());

        Subject<UIPanel> _beginPopSubject;
        public IObservable<UIPanel> onBeginPop => _beginPopSubject ?? (_beginPopSubject = new Subject<UIPanel>());

        Subject<UIPanel> _endPopSubject;
        public IObservable<UIPanel> onEndPop => _endPopSubject ?? (_endPopSubject = new Subject<UIPanel>());

        public UIPanel topPanel => _panels.Count > 0 ? _panels.Peek() : null;
        
        readonly Stack<UIPanel> _panels = new Stack<UIPanel>();
        readonly Lazy<Queue<UIPanel>> _needPushPanels = new Lazy<Queue<UIPanel>>(() => new Queue<UIPanel>());
        readonly Lazy<Queue<int>> _needPopNums = new Lazy<Queue<int>>(() => new Queue<int>());

        bool _isPushing;
        bool _isPoping;
        EventSystem _eventSystem;

        bool interactable
        {
            set => _eventSystem.gameObject.SetActive(value);
        }

#if UNITY_EDITOR
        void Reset()
        {
            _panelFactory = EditorUtil.FindAssets<PanelFactory>().FirstOrDefault();
            if (_panelFactory == null)
            {
                throw new Exception("PanelFactory's implementation not found!");
            }

            _defaultAnimation = EditorUtil.FindAssets<DefaultAnimation>().FirstOrDefault();
        }
#endif        
        
        void Awake()
        {
            _eventSystem = FindObjectOfType<EventSystem>();
        }

        async void Start()
        {
            UIPanel[] panels = GetComponentsInChildren<UIPanel>();
            foreach (UIPanel panel in panels)
            {
                await Push(panel);
            }
        }

        public async UniTask Push(UIPanel panel, bool disableUnderPanel = true)
        {
            if (_isPushing)
            {
                panel.gameObject.SetActive(false);
                _needPushPanels.Value.Enqueue(panel);
                return;
            }
            
            await InternalPush(panel, disableUnderPanel);

            if (_needPushPanels.Value.Count == 0)
            {
                return;
            }
            
            UIPanel uiPanel = _needPushPanels.Value.Dequeue();
            uiPanel.gameObject.SetActive(true);
            await Push(uiPanel);
        }

        async Task InternalPush(UIPanel panel, bool disableUnderPanel)
        {
            _isPushing = true;
            interactable = false;
            _beginPushSubject?.OnNext(panel);
            panel.uiStack = this;
            panel.transform.SetParent(transform, false);
            if (_panels.Count > 0)
            {
                await _panels.Peek().EnterBackground(panel);
            }

            await panel.Enter();
            
            if (_panels.Count > 0)
            {
                _panels.Peek().gameObject.SetActive(!disableUnderPanel);
            }
            
            _panels.Push(panel);
            _endPushSubject?.OnNext(panel);
            interactable = true;
            _isPushing = false;
        }

        public async UniTask Pop(int num = 1)
        {
            if (num > _panels.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (_isPoping)
            {
                _needPopNums.Value.Enqueue(num);
                return;
            }

            await InternalPop(num);

            if (_needPopNums.Value.Count == 0)
            {
                return;
            }
            
            await Pop(_needPopNums.Value.Dequeue());
        }

        async Task InternalPop(int num)
        {
            _isPoping = true;
            interactable = false;
            for (int i = 0; i < num; i++)
            {
                UIPanel panel = _panels.Pop();
                _beginPopSubject?.OnNext(panel);
                
                UIPanel underPanel = null;
                if (_panels.Count > 0)
                {
                    underPanel = _panels.Peek();
                    underPanel.gameObject.SetActive(true);
                }
                
                await panel.Exit();
                if (underPanel != null)
                {
                    await underPanel.EnterForeground(panel);
                }

                panel.uiStack = null;
                _endPopSubject?.OnNext(panel);
            }

            interactable = true;
            _isPoping = false;
        }

        public async UniTask Replace(UIPanel panel)
        {
            await Pop();
            await Push(panel);
        }
    }
}
