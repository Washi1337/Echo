using System;
using AsmResolver.PE.DotNet.Cil;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch
{
    public class CilDispatchEventArgs : EventArgs
    {
        internal CilDispatchEventArgs()
        {
        }

        public CilDispatchEventArgs(CilExecutionContext context, CilInstruction instruction)
        {
            Context = context;
            Instruction = instruction;
        }

        public CilExecutionContext Context
        {
            get;
            internal set;
        }

        public CilInstruction Instruction
        {
            get;
            internal set;
        }

        public bool IsHandled
        {
            get;
            set;
        }

        public CilDispatchResult Result
        {
            get;
            set;
        }
    }
}