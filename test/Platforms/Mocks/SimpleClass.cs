// ReSharper disable UnassignedField.Global
using static InlineIL.IL.Emit;

using System;
using InlineIL;
using System.Collections.Generic;

namespace Mocks
{
    public class SimpleClass
    {
        public static int StaticIntField;
        public static string StaticStringField;
        public static object StaticObjectField;
        public static Int16Enum StaticInt16Enum;
        public static Int32Enum StaticInt32Enum;

        public int IntField;
        public string StringField;
        public SimpleClass SimpleClassField;
        
        public void InstanceMethod()
        {
        }

        public virtual void VirtualInstanceMethod()
        {
        }

        public virtual int VirtualIntInstanceMethod() => 123;
        
        public virtual void VirtualParameterizedInstanceMethod(int a, string b)
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

        public static int Loop()
        {
            IL.DeclareLocals(new LocalVar(typeof(int)), new LocalVar(typeof(int)));
            
            Ldc_I4_0();
            Stloc_0();
            Ldc_I4_0();
            Stloc_1();
            Br_S("Comparison");
            
            IL.MarkLabel("Loop");
            
            Ldloc_0();
            Ldloc_1();
            Add();
            Stloc_1();
            
            Ldloc_0();
            Ldc_I4_1();
            Add();
            Stloc_0();
            
            IL.MarkLabel("Comparison");
            Ldloc_0();
            Ldc_I4_8();
            Ble_S("Loop");

            Ldloc_1();
            return IL.Return<int>();
        }

        public static T GenericMethod<T>(T value)
        {
            T ret = default(T);

            if (value is short)
            {
                ret = (T)(object)value;
            }
            else if (value is int)
            {
                ret = (T)(object)value;
            }
            else if (value is long)    
            {
                ret = (T)(object)value;
            }

            return ret;
        }

        public static string GenericToString<T>(T value) => value.ToString();
    }
}