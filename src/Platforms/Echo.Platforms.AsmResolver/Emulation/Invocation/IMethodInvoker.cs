using System.Collections.Generic;
using AsmResolver.DotNet;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Invocation
{
    /// <summary>
    /// Provides members for calling methods within a CIL virtual machine.
    /// </summary>
    public interface IMethodInvoker
    {
        /// <summary>
        /// Invokes a method definition and returns the result.
        /// </summary>
        /// <param name="method">The method to invoke.</param>
        /// <param name="arguments">The arguments passed onto the method.</param>
        /// <returns>
        /// The return value of the method, or <c>null</c> if the method returned <see cref="System.Void"/>.
        /// </returns>
        ICliValue Invoke(MethodDefinition method, IEnumerable<ICliValue> arguments);
    }
}