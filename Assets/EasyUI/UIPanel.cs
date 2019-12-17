using System;
using UniRx;
using UniRx.Async;
using UnityEngine;
using UnityEngine.UI;

namespace EasyUI
{
    [RequireComponent(typeof(GraphicRaycaster))]
    public class UIPanel : MonoBehaviour
    {
        [SerializeField] BindingTransition[] _bindingTransitions;

        Subject<Unit> _beginEnterSubject;
        public IObservable<Unit> onBeginEnter => _beginEnterSubject ?? (_beginEnterSubject = new Subject<Unit>());

        Subject<Unit> _endEnterSubject;
        public IObservable<Unit> onEndEnter => _endEnterSubject ?? (_endEnterSubject = new Subject<Unit>());

        Subject<Unit> _beginExitSubject;
        public IObservable<Unit> onBeginExit => _beginExitSubject ?? (_beginExitSubject = new Subject<Unit>());

        Subject<Unit> _endExitSubject;
        public IObservable<Unit> onEndExit => _endExitSubject ?? (_endExitSubject = new Subject<Unit>());

        Subject<Unit> _beginEnterBackgroundSubject;
        public IObservable<Unit> onBeginEnterBackground => _beginEnterBackgroundSubject ?? (_beginEnterBackgroundSubject = new Subject<Unit>());

        Subject<Unit> _endEnterBackgroundSubject;
        public IObservable<Unit> onEndEnterBackground => _endEnterBackgroundSubject ?? (_endEnterBackgroundSubject = new Subject<Unit>());

        Subject<Unit> _beginEnterForegroundSubject;
        public IObservable<Unit> onBeginEnterForeground => _beginEnterForegroundSubject ?? (_beginEnterForegroundSubject = new Subject<Unit>());

        Subject<Unit> _endEnterForegroundSubject;
        public IObservable<Unit> onEndEnterForeground => _endEnterForegroundSubject ?? (_endEnterForegroundSubject = new Subject<Unit>());
        
        public UIStack uiStack { get; set; }
        
        Animator _animator;
        UniTaskCompletionSource _enterTask;
        UniTaskCompletionSource _exitTask;

        protected virtual void Awake()
        {
            _animator = GetComponent<Animator>();
            foreach (BindingTransition transition in _bindingTransitions)
            {
                transition.triggerBtn.onClick.AddListener(() => DoTransition(transition));
            }
        }
        
#if UNITY_EDITOR
        void Reset()
        {
            var rectTrans = GetComponent<RectTransform>();
            rectTrans.anchorMin = Vector2.zero;
            rectTrans.anchorMax = Vector2.one;
            
            rectTrans.offsetMin = Vector2.zero;
            rectTrans.offsetMax = Vector2.zero;
        }
#endif

        public void Close()
        {
            uiStack?.Pop();
        }

        public async UniTask Enter()
        {
            _beginEnterSubject?.OnNext(Unit.Default);
            await OnEnter();
            _endEnterSubject?.OnNext(Unit.Default);
        }

        public async UniTask Exit()
        {
            _beginExitSubject?.OnNext(Unit.Default);
            await OnExit();
            _endExitSubject?.OnNext(Unit.Default);
            uiStack.panelFactory.RecyclePanel(this);
        }

        public async UniTask EnterBackground(UIPanel pushPanel)
        {
            _beginEnterBackgroundSubject?.OnNext(Unit.Default);
            await OnEnterBackground(pushPanel);
            _endEnterBackgroundSubject?.OnNext(Unit.Default);
        }

        public async UniTask EnterForeground(UIPanel popPanel)
        {
            _beginEnterForegroundSubject?.OnNext(Unit.Default);
            await OnEnterForeground(popPanel);
            _endEnterForegroundSubject?.OnNext(Unit.Default);
        }

        protected virtual UniTask OnEnter()
        {
            if (_enterTask == null)
            {
                _enterTask = new UniTaskCompletionSource();
            }
            
            if (_animator != null)
            {
                _animator.SetTrigger(Settings.instance.animatorEnterTriggerName);
                return _enterTask.Task;
            }

            if (uiStack.defaultAnimation != null)
            {
                uiStack.defaultAnimation.PlayEnterAnim(this, () => _enterTask.TrySetResult());
                return _enterTask.Task;
            }
            
            return UniTask.CompletedTask;
        }

        protected virtual async UniTask OnExit()
        {
            if (_exitTask == null)
            {
                _exitTask = new UniTaskCompletionSource();
            }
            
            if (_animator != null)
            {
                _animator.SetTrigger(Settings.instance.animatorExitTriggerName);
                await _exitTask.Task;
                return;
            }

            if (uiStack.defaultAnimation != null)
            {
                uiStack.defaultAnimation.PlayExitAnim(this, () => _exitTask.TrySetResult());
                await _exitTask.Task;
            }
        }

        protected virtual UniTask OnEnterBackground(UIPanel pushPanel)
        {
            return UniTask.CompletedTask;
        }

        protected virtual UniTask OnEnterForeground(UIPanel popPanel)
        {
            return UniTask.CompletedTask;    
        }

        void TransferParameter<T>(T arg = default)
        {
            if (!(this is IParameterReceiver<T> receiver)) return;
            receiver.InputParameter(arg);
        }

        UniTask<T> ReturnValue<T>()
        {
            if (this is IReturnValueProvider<T> provider)
            {
                return provider.returnValue;
            }
            
            throw new Exception("Need implement IReturnValueProvider!");
        }
        
        /// <summary>
        /// 带输入参数的Transition
        /// </summary>
        /// <param name="transition"></param>
        /// <param name="arg">输入参数值</param>
        /// <typeparam name="T">输入参数类型</typeparam>
        /// <returns></returns>
        public async UniTask<UIPanel> DoTransition<T>(Transition transition, T arg = default)
        {
            var panel = await uiStack.panelFactory.CreatePanelAsync(transition.destPanelName);
            panel.TransferParameter(arg);
            await DoOperation(panel, transition);
            return panel;
        }

        public async UniTask<UIPanel> DoTransition(Transition transition)
        {
            var panel = await uiStack.panelFactory.CreatePanelAsync(transition.destPanelName);
            await DoOperation(panel, transition);
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

        async UniTask DoOperation(UIPanel panel, Transition transition)
        {
            if (transition.operation == Transition.Operation.Push)
            {
                await uiStack.Push(panel, transition.disableUnderPanel);
            }
            else if (transition.operation == Transition.Operation.Replace)
            {
                await uiStack.Replace(panel);
            }
        }

        // 作为Animation event被调用
        void OnEnterAnimFinish()
        {
            _enterTask?.TrySetResult();
        }

        // 作为Animation event被调用
        void OnExitAnimFinish()
        {
            _exitTask?.TrySetResult();
        }
    }
}