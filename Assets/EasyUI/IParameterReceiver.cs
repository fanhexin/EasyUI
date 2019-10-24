namespace EasyUI
{
    public interface IParameterReceiver<in T>
    {
        void InputParameter(T arg);
    }
}