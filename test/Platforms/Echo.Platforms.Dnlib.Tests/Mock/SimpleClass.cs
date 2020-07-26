// ReSharper disable UnassignedField.Global

using System;

namespace Echo.Platforms.Dnlib.Tests.Mock
{
    public class SimpleClass
    {
        public static int StaticIntField;
        public static string StaticStringField;
        
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

        public static string If(int argument)
        {
            return argument >= 18 ? "Adult" : "Child";
        }

        public static string Switch(int argument)
        {
            return argument switch
            {
                0 => "Red",
                1 => "Orange",
                2 => "Green",
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}