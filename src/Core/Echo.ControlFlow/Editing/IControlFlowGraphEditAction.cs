using System;

namespace Echo.ControlFlow.Editing
{
    /// <summary>
    /// Represents a reversible action that modifies a control flow graph. 
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions stored in the control flow graph.</typeparam>
    public interface IControlFlowGraphEditAction<TInstruction>
    {
        /// <summary>
        /// Applies the edit.
        /// </summary>
        /// <param name="context">The workspace, including the graph to edit, to use.</param>
        /// <exception cref="InvalidOperationException">
        /// Occurs when the edit was already applied.
        /// </exception>
        /// <remarks>
        /// This method should only be called once. Calling this method a second time should happen after a call to
        /// <see cref="Revert"/> was made.
        /// </remarks>
        void Apply(ControlFlowGraphEditContext<TInstruction> context);

        /// <summary>
        /// Reverts the edit.
        /// </summary>
        /// <param name="context">The workspace, including the graph to edit, to use.</param>
        /// <exception cref="InvalidOperationException">
        /// Occurs when the edit was not applied yet.
        /// </exception>
        /// <remarks>
        /// This method should only be called after the <see cref="Apply"/> method was called.
        /// </remarks>
        void Revert(ControlFlowGraphEditContext<TInstruction> context);
    }
}