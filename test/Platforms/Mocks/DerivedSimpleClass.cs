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

        /// <inheritdoc />
        public override void VirtualParameterizedInstanceMethod(int a, string b)
        {
            Console.WriteLine("Overridden.");
            base.VirtualParameterizedInstanceMethod(a, b);
        }
    }
}