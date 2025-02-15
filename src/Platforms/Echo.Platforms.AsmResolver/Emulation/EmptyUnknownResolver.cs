using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation
{
    /// <summary>
    /// Provides an implementation of the <see cref="IUnknownResolver"/> that reports back to the emulator that
    /// it should treat every unknown pointer or array access as an operation that is successful.
    /// </summary>
    public class EmptyUnknownResolver : IUnknownResolver
    {
        /// <summary>
        /// Gets the default instance of the <see cref="EmptyUnknownResolver"/> class.
        /// </summary>
        public static EmptyUnknownResolver Instance
        {
            get;
        } = new();

        /// <inheritdoc />
        public virtual bool ResolveBranchCondition(
            CilExecutionContext context, 
            CilInstruction instruction, 
            StackSlot argument)
        {
            return false;
        }

        /// <inheritdoc />
        public virtual bool ResolveBranchCondition(
            CilExecutionContext context, 
            CilInstruction instruction, 
            StackSlot argument1, 
            StackSlot argument2)
        {
            return false;
        }

        /// <inheritdoc />
        public virtual uint? ResolveSwitchCondition(
            CilExecutionContext context, 
            CilInstruction instruction,
            StackSlot argument)
        {
            return null;
        }

        /// <inheritdoc />
        public virtual long? ResolveSourcePointer(
            CilExecutionContext context,
            CilInstruction instruction,
            StackSlot address)
        {
            return null;
        }

        /// <inheritdoc />
        public virtual long? ResolveDestinationPointer(
            CilExecutionContext context, 
            CilInstruction instruction, 
            StackSlot address)
        {
            return null;
        }

        /// <inheritdoc />
        public virtual uint ResolveBlockSize(
            CilExecutionContext context,
            CilInstruction instruction, 
            StackSlot size)
        {
            return 0;
        }

        /// <inheritdoc />
        public virtual long? ResolveArrayIndex(
            CilExecutionContext context, 
            CilInstruction instruction, 
            long arrayAddress, 
            StackSlot index)
        {
            return null;
        }

        /// <inheritdoc />
        public virtual IMethodDescriptor? ResolveMethod(
            CilExecutionContext context, 
            CilInstruction instruction, 
            IList<BitVector> arguments)
        {
            return null;
        }

        /// <inheritdoc />
        public virtual IMethodDescriptor? ResolveDelegateTarget(
            CilExecutionContext context,
            ObjectHandle delegateObject,
            IList<BitVector> arguments)
        {
            return null;
        }

        /// <inheritdoc />
        public virtual bool ResolveExceptionFilter(
            CilExecutionContext context, 
            CilInstruction instruction,
            StackSlot conclusion)
        {
            return false;
        }
    }
}