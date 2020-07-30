using Echo.ControlFlow;
using Echo.Core.Code;

namespace Echo.Ast
{
    /// <summary>
    /// Represents the root of the syntax tree
    /// </summary>
    public class CompilationUnit<TInstruction> : ControlFlowGraph<AstStatementBase<TInstruction>>
    {
        /// <summary>
        /// Creates a new instance of <see cref="CompilationUnit{TInstruction}"/>
        /// </summary>
        public CompilationUnit(IInstructionSetArchitecture<AstStatementBase<TInstruction>> isa)
            : base(isa) { }
    }
}