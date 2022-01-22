using System;
using AsmResolver.PE.DotNet.Cil;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch
{
    public class DispatcherTableEntryAttribute : Attribute
    {
        public DispatcherTableEntryAttribute(params CilCode[] opCodes)
        {
            OpCodes = opCodes;
        }

        public CilCode[] OpCodes
        {
            get;
        }
    }
}