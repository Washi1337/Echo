using System.Collections.Generic;

namespace Echo.ControlFlow.Construction
{
    /// <summary>
    /// Provides members for resolving the static successors of a single instruction. That is, resolve any successor
    /// that is encoded within an instruction either explicitly or implicitly.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instruction to resolve the successors from.</typeparam>
    /// <remarks>
    /// <para>
    /// This interface is meant for components within the Echo project that require information about the successors
    /// of an individual instruction. These are typically control flow graph builders, such as the
    /// <see cref="StaticFlowGraphBuilder{TInstruction}"/> class.
    /// </para>
    ///
    /// <para>
    /// Successors are either directly encoded within the instruction (e.g. as an operand),
    /// or implied by the default flow control of the provided instruction:
    /// <list type="bullet">
    ///     <item>
    ///         <description>
    ///             For a typical instruction, the method simply returns a collection with only a reference to the
    ///             fall through instruction that appears right after it in the sequence.
    ///         </description>
    ///     </item> 
    ///     <item>
    ///         <description>
    ///             For branching instructions, however, this method returns a collection of all branch targets,
    ///             as well as any potential fall through successors if the branching instruction is conditional.
    ///         </description>
    ///     </item>
    /// </list>
    /// </para>
    ///
    /// <para>
    /// This interface provides members for extracting these successors from the provided instruction.
    /// </para>
    /// </remarks>
    public interface IStaticSuccessorResolver<TInstruction>
        where TInstruction : notnull
    {
        /// <summary>
        /// Gets a collection of references that represent the successors of the provided instruction.
        /// </summary>
        /// <param name="instruction">The instruction to resolve the successors from.</param>
        /// <param name="successorsBuffer">The buffer to add the successors into.</param>
        void GetSuccessors(in TInstruction instruction, IList<SuccessorInfo> successorsBuffer);
    }
}