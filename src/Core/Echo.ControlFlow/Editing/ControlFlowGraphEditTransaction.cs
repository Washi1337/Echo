using System;
using System.Collections;
using System.Collections.Generic;

namespace Echo.ControlFlow.Editing
{
    /// <summary>
    /// Represents a sequence of edits to be applied to a control flow graph.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions stored in the control flow graph.</typeparam>
    public class ControlFlowGraphEditTransaction<TInstruction> : IEnumerable<IControlFlowGraphEditAction<TInstruction>>
    {
        private readonly List<IControlFlowGraphEditAction<TInstruction>> _actions = new List<IControlFlowGraphEditAction<TInstruction>>();

        /// <summary>
        /// Gets the number of actions that will be performed.
        /// </summary>
        public int Count => _actions.Count;
        
        /// <summary>
        /// Gets a value indicating whether all the edits were applied successfully.
        /// </summary>
        public bool IsCompleted
        {
            get;
            private set;
        }
        
        /// <summary>
        /// Adds an edit action to the end of the sequence.
        /// </summary>
        /// <param name="action">The action to add.</param>
        public void EnqueueAction(IControlFlowGraphEditAction<TInstruction> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            _actions.Add(action);
        }

        /// <summary>
        /// Applies all edits to the control flow graph.
        /// </summary>
        /// <param name="graph">The graph to apply the transaction on.</param>
        /// <exception cref="InvalidOperationException">Occurs when the transaction was already applied.</exception>
        /// <remarks>
        /// When an exception occurs within one of the edits, all edits previously applied will be reverted. 
        /// </remarks>
        public void Apply(ControlFlowGraph<TInstruction> graph)
        {
            if (IsCompleted)
                throw new InvalidOperationException("The transaction is already applied.");
            
            var context = new ControlFlowGraphEditContext<TInstruction>(graph);
            int index = 0;
            
            try
            {
                for (; index < _actions.Count; index++)
                {
                    var edit = _actions[index];
                    edit.Apply(context);
                }

                IsCompleted = true;
            }
            catch
            {
                index--;
                for (; index >= 0; index--)
                    _actions[index].Revert(context);
                throw;
            }
        }

        /// <inheritdoc />
        public IEnumerator<IControlFlowGraphEditAction<TInstruction>> GetEnumerator() => _actions.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}