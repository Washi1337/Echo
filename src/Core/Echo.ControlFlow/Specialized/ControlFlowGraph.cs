using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow.Specialized.Blocks;

namespace Echo.ControlFlow.Specialized
{
    /// <summary>
    /// Represents a control flow graph that encodes all possible execution paths of a chunk of code.
    /// </summary>
    /// <typeparam name="TInstruction"></typeparam>
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
    }
}