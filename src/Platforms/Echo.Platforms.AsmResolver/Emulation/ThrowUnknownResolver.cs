using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation
{
    /// <summary>
    /// Provides an implementation of the <see cref="IUnknownResolver"/> that throws upon resolving an unknown value. 
    /// </summary>
    public class ThrowUnknownResolver : IUnknownResolver
    {
        /// <summary>
        /// Gets the default instance of the <see cref="ThrowUnknownResolver"/> class.
        /// </summary>
        public static ThrowUnknownResolver Instance
        {
            get;
        } = new();
        
        /// <inheritdoc />
        public virtual bool ResolveBranchCondition(
            CilExecutionContext context,
            CilInstruction instruction,
            StackSlot argument)
        {
            throw new CilEmulatorException($"Branch condition for {instruction} evaluated in an unknown boolean value.");
        }
        
        /// <inheritdoc />
        public virtual bool ResolveBranchCondition(
            CilExecutionContext context,
            CilInstruction instruction,
            StackSlot argument1,
            StackSlot argument2)
        {
            throw new CilEmulatorException($"Branch condition for {instruction} evaluated in an unknown boolean value.");
        }
        
        /// <inheritdoc />
        public virtual uint? ResolveSwitchCondition(
            CilExecutionContext context,
            CilInstruction instruction,
            StackSlot argument)
        {
            throw new CilEmulatorException($"Switch index for {instruction} evaluated in an unknown index value.");
        }
        
        /// <inheritdoc />
        public virtual long? ResolveSourcePointer(
            CilExecutionContext context,
            CilInstruction instruction,
            StackSlot address)
        {
            throw new CilEmulatorException("Attempted to read memory at an unknown pointer.");
        }
        
        /// <inheritdoc />
        public virtual long? ResolveDestinationPointer(
            CilExecutionContext context,
            CilInstruction instruction,
            StackSlot address)
        {
            throw new CilEmulatorException("Attempted to write memory at an unknown pointer.");
        }

        /// <inheritdoc />
        public virtual uint ResolveBlockSize(
            CilExecutionContext context,
            CilInstruction instruction,
            StackSlot size)
        {
            throw new CilEmulatorException("Attempted to allocate or copy an unknown amount of bytes.");
        }
        
        /// <inheritdoc />
        public virtual long? ResolveArrayIndex(
            CilExecutionContext context,
            CilInstruction instruction,
            long arrayAddress,
            StackSlot index)
        {
            throw new CilEmulatorException("Attempted to access an array element with an unknown index.");
        }

        /// <inheritdoc />
        public virtual IMethodDescriptor? ResolveMethod(
            CilExecutionContext context, 
            CilInstruction instruction, 
            IList<BitVector> arguments)
        {
            throw new CilEmulatorException($"Attempted to devirtualize method call {instruction} on an unknown object instance.");
        }
    }
}