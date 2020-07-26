using System;

namespace Echo.Platforms.AsmResolver.Tests.Mock
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