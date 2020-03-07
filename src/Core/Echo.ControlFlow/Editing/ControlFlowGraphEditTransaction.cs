using System;
using System.Collections.Generic;

namespace Echo.ControlFlow.Editing
{
    public class ControlFlowGraphEditTransaction<TInstruction> 
    {
        private readonly List<IControlFlowGraphEditAction<TInstruction>> _actions = new List<IControlFlowGraphEditAction<TInstruction>>();

        public ControlFlowGraphEditTransaction(ControlFlowGraph<TInstruction> graph)
        {
            Graph = graph ?? throw new ArgumentNullException(nameof(graph));
        }
        
        public ControlFlowGraph<TInstruction> Graph
        {
            get;
        }

        public IReadOnlyList<IControlFlowGraphEditAction<TInstruction>> Actions => _actions;
        
        public void Apply()
        {
            var context = new ControlFlowGraphEditContext<TInstruction>(Graph);
            int index = 0;
            
            try
            {
                for (; index < _actions.Count; index++)
                {
                    var edit = _actions[index];
                    edit.Apply(context);
                }
            }
            catch
            {
                index--;
                for (; index >= 0; index--)
                    _actions[index].Revert(context);
                throw;
            }
        }

        public void EnqueueAction(IControlFlowGraphEditAction<TInstruction> action)
        {
            _actions.Add(action);
        }
    }
}