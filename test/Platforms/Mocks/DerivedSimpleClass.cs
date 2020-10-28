using System;

namespace Mocks
{
    public class DerivedSimpleClass : SimpleClass
    {
        public override void VirtualInstanceMethod()
        {
            Console.WriteLine("Overridden.");
            base.VirtualInstanceMethod();
        }
    }
}