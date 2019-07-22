using System.Collections.Generic;
using System.Linq;

namespace Echo.ControlFlow.Specialized
{
    public class ControlFlowGraph<TInstruction> : Graph<BasicBlock<TInstruction>>
    {
        /// <summary>
        /// Gets an ordered collection of all exception handlers that are defined in the control flow graph.
        /// </summary>
        /// <remarks>
        /// If two exception handlers are nested (i.e. they overlap in the try segments), the one that occurs first in
        /// this collection is the enclosing exception handler.
        /// </remarks>
        public IList<ExceptionHandler<TInstruction>> ExceptionHandlers
        {
            get;
        } = new List<ExceptionHandler<TInstruction>>();

        public Node<BasicBlock<TInstruction>> GetNodeByOffset(long offset)
        {
            // TODO: use something more efficient than a linear search.
            return Nodes.First(n => n.Contents.Offset == offset);
        }
    }
}