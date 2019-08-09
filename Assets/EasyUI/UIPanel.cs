using System;
using System.Linq;
using UniRx;
using UniRx.Async;
using UnityEngine;
using UnityEngine.UI;

namespace EasyUI
{
    [RequireComponent(typeof(GraphicRaycaster))]
    public abstract class UIPanel : MonoBehaviour
    {
        [SerializeField] PanelFactory _panelFactory;
        [SerializeField] protected DefaultAnimation _defaultAnimation;
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
            _panelFactory = EditorUtil.FindAssets<PanelFactory>().FirstOrDefault();
            if (_panelFactory == null)
            {
                throw new Exception("AbstractPanelFactory's implementation not found!");
            }

            _defaultAnimation = EditorUtil.FindAssets<DefaultAnimation>().FirstOrDefault();

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
            _panelFactory.RecyclePanel(this);
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
                _animator.SetTrigger("panel_enter");
                return _enterTask.Task;
            }

            if (_defaultAnimation != null)
            {
                _defaultAnimation.PlayEnterAnim(this, () => _enterTask.TrySetResult());
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
                _animator.SetTrigger("panel_exit");
                
                await _exitTask.Task;
                return;
            }

            if (_defaultAnimation != null)
            {
                _defaultAnimation.PlayExitAnim(this, () => _exitTask.TrySetResult());
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

        public async UniTask DoTransition(Transition transition)
        {
            var panel = await _panelFactory.CreatePanelAsync(transition.destPanelName);
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