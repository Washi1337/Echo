using System;
using System.Collections.Generic;
using AsmResolver.DotNet;
using Echo.Platforms.AsmResolver.Emulation.Invocation;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Tests.Mock
{
    public sealed class HookedMethodInvoker : IMethodInvoker
    {
        private readonly IMethodInvoker _original;

        public HookedMethodInvoker(IMethodInvoker original)
        {
            _original = original ?? throw new ArgumentNullException(nameof(original));
        }

        public IMethodDescriptor LastInvokedMethod
        {
            get;
            private set;
        }
            
        public ICliValue Invoke(IMethodDescriptor method, IEnumerable<ICliValue> arguments)
        {
            LastInvokedMethod = method;
            return _original.Invoke(method, arguments);
        }
    }
}