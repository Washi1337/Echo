namespace Mocks
{
    public class SimpleGenericClass<T, U>
    {
        public T MethodUsingGenericParameter(T t)
        {
            return t;
        }
    }
}