using Echo.ControlFlow;
using Echo.Core.Code;

namespace Echo.Ast
{
    /// <summary>
    /// Transforms a <see cref="ControlFlowGraph{TInstruction}"/> into an Ast
    /// </summary>
    public sealed class AstParser<TInstruction>
    {
        private readonly ControlFlowGraph<TInstruction> _controlFlowGraph;
        private readonly IInstructionSetArchitecture<AstStatementBase<TInstruction>> _architecture;
        
        /// <summary>
        /// Creates a new Ast parser with the given <see cref="ControlFlowGraph{TInstruction}"/>
        /// </summary>
        /// <param name="controlFlowGraph">The <see cref="ControlFlowGraph{TInstruction}"/> to parse</param>
        public AstParser(ControlFlowGraph<TInstruction> controlFlowGraph)
        {
            _controlFlowGraph = controlFlowGraph;
            _architecture = new AstInstructionSetArchitectureDecorator<TInstruction>(_controlFlowGraph.Architecture);
        }

        /// <summary>
        /// Parses the given <see cref="ControlFlowGraph{TInstruction}"/>
        /// </summary>
        /// <returns>A <see cref="CompilationUnit{TInstruction}"/> representing the Ast</returns>
        public CompilationUnit<TInstruction> Parse()
        {
            var root = new CompilationUnit<TInstruction>(_architecture);
            return root;
        }
    }
}