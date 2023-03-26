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

        public static void ValueTypeArgument(SimpleStruct s, int x)
        {
            Console.WriteLine(s);
            Console.WriteLine(x);
        }

        public static void RefTypeArgument(object s, int x)
        {
            Console.WriteLine(s);
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

        public static int SimpleTest()
        {
            int x = 0x1337; 
            x += DangerousMethod(10);
            x &= 0xFF00;
            Console.WriteLine(x >= 0x1000 ? "x >= 0x1000" : "x < 0x1000");
            return x;
        }

        public static int DangerousMethod(int y)
        {
            return y + 100;
        }
    }
}