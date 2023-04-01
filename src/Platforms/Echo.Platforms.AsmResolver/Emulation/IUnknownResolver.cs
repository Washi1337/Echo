using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation
{
    /// <summary>
    /// Provides methods for resolving unknown values in critical points of the emulation process.
    /// </summary>
    public interface IUnknownResolver
    {
        /// <summary>
        /// Resolves an unknown condition value of a unary conditional branch instruction.
        /// </summary>
        /// <param name="context">The context in which the instruction is executed in.</param>
        /// <param name="instruction">The branch instruction that is being executed.</param>
        /// <param name="argument">The condition to be resolved.</param>
        /// <returns><c>true</c> if the branch should be taken, <c>false</c> otherwise.</returns>
        bool ResolveBranchCondition(CilExecutionContext context, CilInstruction instruction, StackSlot argument);
        
        /// <summary>
        /// Resolves an unknown condition value of a binary conditional branch instruction.
        /// </summary>
        /// <param name="context">The context in which the instruction is executed in.</param>
        /// <param name="instruction">The branch instruction that is being executed.</param>
        /// <param name="argument1">The first argument.</param>
        /// <param name="argument2">The second argument.</param>
        /// <returns><c>true</c> if the branch should be taken, <c>false</c> otherwise.</returns>
        bool ResolveBranchCondition(CilExecutionContext context, CilInstruction instruction, StackSlot argument1, StackSlot argument2);
        
        /// <summary>
        /// Resolves an unknown index value of a switch instruction.
        /// </summary>
        /// <param name="context">The context in which the instruction is executed in.</param>
        /// <param name="instruction">The switch instruction that is being executed.</param>
        /// <param name="argument">The switch index to resolve.</param>
        /// <returns>The resolved index to jump to, or <c>null</c> to skip the switch instruction.</returns>
        uint? ResolveSwitchCondition(CilExecutionContext context, CilInstruction instruction, StackSlot argument);

        /// <summary>
        /// Resolves an unknown source address to a memory block or object to read data from.
        /// </summary>
        /// <param name="context">The context in which the instruction is executed in.</param>
        /// <param name="instruction">The switch instruction that is being executed.</param>
        /// <param name="address">The address to resolve.</param>
        /// <returns>The resolved address, or <c>null</c> to treat it as an unknown value that is processed successfully.</returns>
        long? ResolveSourcePointer(CilExecutionContext context, CilInstruction instruction, StackSlot address);
        
        /// <summary>
        /// Resolves an unknown destination address to a memory block or object to write data to.
        /// </summary>
        /// <param name="context">The context in which the instruction is executed in.</param>
        /// <param name="instruction">The switch instruction that is being executed.</param>
        /// <param name="address">The address to resolve.</param>
        /// <returns>The resolved address, or <c>null</c> to treat it as an unknown value that is processed successfully.</returns>
        long? ResolveDestinationPointer(CilExecutionContext context, CilInstruction instruction, StackSlot address);
        
        /// <summary>
        /// Resolves an unknown size for a memory block.
        /// </summary>
        /// <param name="context">The context in which the instruction is executed in.</param>
        /// <param name="instruction">The switch instruction that is being executed.</param>
        /// <param name="size">The size to resolve.</param>
        /// <returns>The resolved size, or <c>null</c> to treat it as an unknown value that is processed successfully.</returns>
        uint ResolveBlockSize(CilExecutionContext context, CilInstruction instruction, StackSlot size);

        /// <summary>
        /// Resolves an unknown index of an element within an array.
        /// </summary>
        /// <param name="context">The context in which the instruction is executed in.</param>
        /// <param name="instruction">The switch instruction that is being executed.</param>
        /// <param name="arrayAddress">The address of the array.</param>
        /// <param name="index">The index to resolve.</param>
        /// <returns>The resolved index, or <c>null</c> to treat it as an unknown value that is processed successfully.</returns>
        long? ResolveArrayIndex(CilExecutionContext context, CilInstruction instruction, long arrayAddress, StackSlot index);
    }
}