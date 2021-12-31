using System;
using InlineIL;

namespace Mocks
{
    // ReSharper disable once MemberCanBePrivate.Global
    public class TestClass
    {
        public static void StaticMethod(int i) => Console.WriteLine(i);
        public void InstanceMethod(int i) => Console.WriteLine(i);

        public static string GetConstantString() => "Hello, world!";
        public static string GetIsEvenString(int i) => i % 2 == 0 ? "even" : "odd";

        public static void ExceptionHandler()
        {
            try
            {
                Console.WriteLine("Password:");
                if (Console.ReadLine() == "MyPassword")
                {
                    Console.WriteLine("Amazing");
                }
                else
                {
                    Console.WriteLine("Nope");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static void NoLocalsNoArguments()
        {
        }

        public static void SingleArgument(int x)
        {
            Console.WriteLine(x);
        }

        public static void MultipleArguments(int x, int y, int z)
        {
            Console.WriteLine(x);
            Console.WriteLine(y);
            Console.WriteLine(z);
        }

        public static void SingleLocal()
        {
            IL.DeclareLocals(new LocalVar(new TypeRef(typeof(int))));
            IL.Emit.Ldc_I4_0();
            IL.Emit.Stloc_0();
            IL.Emit.Ret();
        }

        public static void MultipleLocals()
        {
            IL.DeclareLocals(
                new LocalVar(new TypeRef(typeof(int))),
                new LocalVar(new TypeRef(typeof(int))),
                new LocalVar(new TypeRef(typeof(int)))
            );
            IL.Emit.Ret();
        }

        public static void MultipleLocalsNoInit()
        {
            IL.DeclareLocals(
                false,
                new LocalVar(new TypeRef(typeof(int))),
                new LocalVar(new TypeRef(typeof(int))),
                new LocalVar(new TypeRef(typeof(int)))
            );
            IL.Emit.Ret();
        }

        public static void MultipleLocalsMultipleArguments(int a, int b, int c)
        {
            Console.WriteLine(a);
            Console.WriteLine(b);
            Console.WriteLine(c);

            IL.DeclareLocals(
                new LocalVar(new TypeRef(typeof(int))),
                new LocalVar(new TypeRef(typeof(int))),
                new LocalVar(new TypeRef(typeof(int)))
            );
            IL.Emit.Ret();
        }

    }
}