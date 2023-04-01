using System;
using AsmResolver.PE.DotNet.Cil;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch
{
    /// <summary>
    /// Marks the class as a default operation code handler for a <see cref="CilDispatcher"/>. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DispatcherTableEntryAttribute : Attribute
    {
        /// <summary>
        /// Marks the class as a default handler for a <see cref="CilDispatcher"/> for the provided operation codes.
        /// </summary>
        /// <param name="opCodes">The opcodes the class handles.</param>
        public DispatcherTableEntryAttribute(params CilCode[] opCodes)
        {
            OpCodes = opCodes;
        }

        /// <summary>
        /// Gets a collection of opcodes this class handles.
        /// </summary>
        public CilCode[] OpCodes
        {
            get;
        }
    }
}