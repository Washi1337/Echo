// ReSharper disable UnassignedField.Global

namespace Echo.Platforms.AsmResolver.Tests.Mock
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
    }
}