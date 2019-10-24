using UniRx.Async;

namespace EasyUI
{
    public interface IReturnValueProvider<T>
    {
        UniTask<T> returnValue { get; }
    }
}