using System;

namespace Mocks
{
    // ReSharper disable once MemberCanBePrivate.Global
    public class TestClass
    {
        public static void StaticMethod(int i) => Console.WriteLine(i);
        public void InstanceMethod(int i) => Console.WriteLine(i);

        public static string GetConstantString() => "Hello, world!";
        public static string GetIsEvenString(int i) => i % 2 == 0 ? "even" : "odd";
    }
}