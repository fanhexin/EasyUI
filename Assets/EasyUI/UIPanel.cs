using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace EasyUI
{
    [RequireComponent(typeof(GraphicRaycaster), typeof(RectTransform))]
    public class UIPanel : MonoBehaviour
    {
        protected static SafeArea _safeArea;
        public class SafeArea 
        {
            public float topOffset { get; internal set; }
            public float bottomOffset { get; internal set; }
            public float leftOffset { get; internal set; }
            public float rightOffset { get; internal set; }
        }
        
        [Serializable]
        public class NotchAdapter
        {
            [SerializeField] RectTransform _target;
            [SerializeField] bool _adjustTopOffset;
            [SerializeField] bool _adjustBottomOffset;
            [SerializeField] bool _adjustLeftOffset;
            [SerializeField] bool _adjustRightOffset;

            public void Apply(int direction)
            {
                if (_target == null)
                {
                    return;
                }
                
                if (_adjustTopOffset)
                {
                    _target.offsetMax -= _safeArea.topOffset * direction * Vector2.up;
                }

                if (_adjustBottomOffset)
                {
                    _target.offsetMin += _safeArea.bottomOffset * direction * Vector2.up;
                }

                if (_adjustLeftOffset)
                {
                    _target.offsetMin += _safeArea.leftOffset * direction * Vector2.right;
                }

                if (_adjustRightOffset)
                {
                    _target.offsetMax -= _safeArea.rightOffset * direction * Vector2.right;
                }    
            }
        }
        
        [SerializeField] BindingTransition[] _bindingTransitions;
        [SerializeField] NotchAdapter[] _notchAdapters;
        [SerializeField, Tooltip("OnExit时是否重置异形屏适配效果")] bool _resetNotchOnExit;

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

        public UIStack uiStack { get; internal set; }
        
        public RectTransform rectTransform { get; private set; }
        Animator _animator;
        UniTaskCompletionSource _enterTask;
        UniTaskCompletionSource _exitTask;

        protected virtual void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            _animator = GetComponent<Animator>();
            foreach (BindingTransition transition in _bindingTransitions)
            {
                transition.triggerBtn.onClick.AddListener(() => uiStack.DoTransition(transition));
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
            AdaptNotch();
            await OnEnter();
            _endEnterSubject?.OnNext(Unit.Default);
        }

        /// <summary>
        /// 异形屏适配
        /// </summary>
        /// <param name="direction">取1或-1</param>
        void AdaptNotch(int direction = 1)
        {
            if (_notchAdapters == null)
            {
                return;
            }
            
            foreach (NotchAdapter adapter in _notchAdapters)
            {
                adapter.Apply(direction);
            }
        }

        public async UniTask Exit()
        {
            if (_safeArea == null)
            {
                InitSafeArea(uiStack.transform);    
            }
            
            _beginExitSubject?.OnNext(Unit.Default);
            await OnExit();
            // 还原异形屏适配效果，避免Panel实例重用时不断OnEnter适配过头
            if (_resetNotchOnExit)
            {
                AdaptNotch(-1);
            }
            _endExitSubject?.OnNext(Unit.Default);
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
            _enterTask = PlayAnimation(Settings.instance.animatorEnterTriggerName, anim => anim.PlayEnterAnim);
            return _enterTask?.Task ?? UniTask.CompletedTask;
        }

        protected virtual UniTask OnExit()
        {
            _exitTask = PlayAnimation(Settings.instance.animatorExitTriggerName, anim => anim.PlayExitAnim);
            return _exitTask?.Task ?? UniTask.CompletedTask;
        }

        UniTaskCompletionSource PlayAnimation(string animatorTriggerName, Func<DefaultAnimation, Action<UIPanel, Action>> tweenAnimFn)
        {
            UniTaskCompletionSource utcs = null;    
            
            if (_animator != null && 
                _animator.parameterCount > 0 &&
                _animator.parameters.Any(x => 
                    x.name == animatorTriggerName && x.type == AnimatorControllerParameterType.Trigger))
            {
                utcs = new UniTaskCompletionSource();
                _animator.SetTrigger(animatorTriggerName);
            }
            else if (uiStack.defaultAnimation != null)
            {
                utcs = new UniTaskCompletionSource();
                tweenAnimFn(uiStack.defaultAnimation)(this, () => utcs.TrySetResult());
            }

            return utcs;
        }

        protected virtual UniTask OnEnterBackground(UIPanel pushPanel)
        {
            return UniTask.CompletedTask;
        }

        protected virtual UniTask OnEnterForeground(UIPanel popPanel)
        {
            return UniTask.CompletedTask;    
        }

        public void TransferParameter<T>(T arg = default)
        {
            if (!(this is IParameterReceiver<T> receiver)) return;
            receiver.InputParameter(arg);
        }
        
        public async UniTask<T> ReturnValue<T>()
        {
            await onBeginExit.First();
            if (this is IReturnValueProvider<T> provider)
            {
                return provider.returnValue;
            }
            
            throw new Exception("Need implement IReturnValueProvider!");
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
        
        UIPanel GetUnderPanel(int index)
        {
            if (--index < 0)
            {
                return null;
            }

            var underPanel = uiStack.transform
                .GetChild(index)
                .GetComponent<UIPanel>();
            if (underPanel != null)
            {
                return underPanel;
            }

            return GetUnderPanel(index);
        }

        /// <summary>
        /// 获取被当前界面盖住的界面
        /// </summary>
        public UIPanel underPanel => GetUnderPanel(transform.GetSiblingIndex());

        public static void InitSafeArea(Transform canvasTransform)
        {
            _safeArea = new SafeArea
            {
                topOffset = (Screen.height - Screen.safeArea.yMax) / canvasTransform.localScale.y,
                bottomOffset = Screen.safeArea.yMin / canvasTransform.localScale.y,
                leftOffset = Screen.safeArea.xMin / canvasTransform.localScale.x,
                rightOffset = (Screen.width - Screen.safeArea.xMax)  / canvasTransform.localScale.x
            };
        }
    }
}