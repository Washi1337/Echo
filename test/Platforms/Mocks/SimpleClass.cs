// ReSharper disable UnassignedField.Global
using static InlineIL.IL.Emit;

using System;
using InlineIL;

namespace Mocks
{
    public class SimpleClass
    {
        public static int StaticIntField;
        public static string StaticStringField;
        public static object StaticObjectField;
        
        public int IntField;
        public string StringField;
        public SimpleClass SimpleClassField;

        public void InstanceMethod()
        {
        }

        public virtual void VirtualInstanceMethod()
        {
        }

        public static void HelloWorld()
        {
            Console.WriteLine("Hello, world!");
        }

        /// <remarks>Implemented in IL because Release builds don't converge to a single <c>ret</c>.</remarks>
        public static string If(int argument)
        {
            Ldarg_0();
            Ldc_I4_S(18);
            Bge_S("BranchAdult");

            Ldstr("Child");
            Br_S("Ret");

            IL.MarkLabel("BranchAdult");
            Ldstr("Adult");

            IL.MarkLabel("Ret");
            return IL.Return<string>();
        }

        /// <remarks>Implemented in IL because Debug builds insert arbitrary <c>if (true) {}</c> blocks.</remarks>
        public static string SwitchColor(int argument)
        {
            IL.DeclareLocals(new LocalVar(typeof(string)));

            Ldarg_0();
            Switch("0_Red", "1_Orange", "2_Green");
            Br_S("Default");

            IL.MarkLabel("0_Red");
            Ldstr("Red");
            Stloc_0();
            Br_S("End");

            IL.MarkLabel("1_Orange");
            Ldstr("Orange");
            Stloc_0();
            Br_S("End");

            IL.MarkLabel("2_Green");
            Ldstr("Green");
            Stloc_0();
            Br_S("End");

            IL.MarkLabel("Default");
            Newobj(MethodRef.Constructor(typeof(ArgumentOutOfRangeException)));
            Throw();

            IL.MarkLabel("End");
            Ldloc_0();
            return IL.Return<string>();
        }
    }
}