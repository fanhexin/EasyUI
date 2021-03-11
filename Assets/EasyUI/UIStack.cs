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
        readonly Lazy<Queue<(UIPanel, bool)>> _needPushPanels = 
            new Lazy<Queue<(UIPanel, bool)>>(() => new Queue<(UIPanel, bool)>());
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
        
        async void Awake()
        {
            _eventSystem = FindObjectOfType<EventSystem>();
            if (_isPushing)
            {
                return;
            }
            
            UIPanel[] panels = GetComponentsInChildren<UIPanel>();
            foreach (UIPanel panel in panels)
            {
                await Push(panel);
            }
        }

        async UniTask Push(UIPanel panel, bool disableUnderPanel = true)
        {
            if (_isPushing)
            {
                panel.gameObject.SetActive(false);
                _needPushPanels.Value.Enqueue((panel, disableUnderPanel));
                return;
            }
            
            await InternalPush(panel, disableUnderPanel);

            if (_needPushPanels.Value.Count == 0)
            {
                return;
            }
            
            var (uiPanel, ifDisableUnderPanel) = _needPushPanels.Value.Dequeue();
            uiPanel.gameObject.SetActive(true);
            await Push(uiPanel, ifDisableUnderPanel);
        }

        async Task InternalPush(UIPanel panel, bool disableUnderPanel, bool underPanelEnterBackground = true)
        {
            _beginPushSubject?.OnNext(panel);
            _isPushing = true;
            interactable = false;
            panel.uiStack = this;
            panel.transform.SetParent(transform, false);
            if (_panels.Count > 0 && underPanelEnterBackground)
            {
                await _panels.Peek().EnterBackground(panel);
            }

            await panel.Enter();
            
            if (_panels.Count > 0)
            {
                _panels.Peek().gameObject.SetActive(!disableUnderPanel);
            }
            
            _panels.Push(panel);
            interactable = true;
            _isPushing = false;
            _endPushSubject?.OnNext(panel);
        }

        public async UniTask Pop(int num = 1, bool disableUnderPanel = false)
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

            await InternalPop(num, disableUnderPanel);

            if (_needPopNums.Value.Count == 0)
            {
                return;
            }
            
            await Pop(_needPopNums.Value.Dequeue(), disableUnderPanel);
        }

        async Task InternalPop(int num = 1, bool disableUnderPanel = false, bool underPanelEnterForeground = true)
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
                    underPanel.gameObject.SetActive(!disableUnderPanel);
                }
                
                await panel.Exit();
                _panelFactory.RecyclePanel(panel);
                if (underPanel != null && underPanelEnterForeground)
                {
                    await underPanel.EnterForeground(panel);
                }

                _endPopSubject?.OnNext(panel);
            }

            interactable = true;
            _isPoping = false;
        }

        async UniTask Replace(UIPanel panel, bool disableUnderPanel = true)
        {
            InternalPop(1, disableUnderPanel, false);
            await InternalPush(panel, disableUnderPanel, false);
        }

        /// <summary>
        /// 带输入参数的Transition
        /// </summary>
        /// <param name="transition"></param>
        /// <param name="arg">输入参数值</param>
        /// <typeparam name="T">输入参数类型</typeparam>
        /// <returns></returns>
        public UniTask<UIPanel> DoTransition<T>(Transition transition, T arg = default)
        {
            return DoTransition(transition.destPanelName, transition.operation, transition.disableUnderPanel, arg);
        }

        public async UniTask<UIPanel> DoTransition<T>(string panelName,
            Transition.Operation operation = Transition.Operation.Push, 
            bool disableUnderPanel = true, 
            T arg = default)
        {
            // 在Factory创建Panel之前就要禁用触控，否则快速点击时可能会创建多个相同Panel
            interactable = false;
            var panel = await _panelFactory.CreatePanelAsync(panelName);
            panel.TransferParameter(arg);
            await DoOperation(panel, operation, disableUnderPanel);
            return panel;
        }

        public UniTask<UIPanel> DoTransition(Transition transition)
        {
            return DoTransition(transition.destPanelName, transition.operation, transition.disableUnderPanel);
        }

        public async UniTask<UIPanel> DoTransition(string panelName,
            Transition.Operation operation = Transition.Operation.Push,
            bool disableUnderPanel = true)
        {
            interactable = false;
            var panel = await _panelFactory.CreatePanelAsync(panelName);
            await DoOperation(panel, operation, disableUnderPanel);
            return panel;
        }

        /// <summary>
        /// 带返回值的Transition
        /// </summary>
        /// <param name="transition"></param>
        /// <typeparam name="R">Panel返回值类型</typeparam>
        /// <returns></returns>
        public async UniTask<R> DoTransition<R>(Transition transition)
        {
            var panel = await DoTransition(transition);
            return await panel.ReturnValue<R>();
        }

        /// <summary>
        /// 带输入参数和返回值的Transition
        /// </summary>
        /// <param name="transition"></param>
        /// <param name="arg">输入参数值</param>
        /// <typeparam name="T">输入参数类型</typeparam>
        /// <typeparam name="R">Panel返回值类型</typeparam>
        /// <returns></returns>
        public async UniTask<R> DoTransition<T, R>(Transition transition, T arg = default)
        {
            var panel = await DoTransition(transition, arg);
            return await panel.ReturnValue<R>();
        }

        async UniTask DoOperation(UIPanel panel, Transition.Operation operation, bool disableUnderPanel)
        {
            if (operation == Transition.Operation.Push)
            {
                await Push(panel, disableUnderPanel);
            }
            else if (operation == Transition.Operation.Replace)
            {
                await Replace(panel, disableUnderPanel);
            }
        }
    }
}
