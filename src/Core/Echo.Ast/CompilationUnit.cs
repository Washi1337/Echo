using Echo.ControlFlow;
using Echo.Core.Code;

namespace Echo.Ast
{
    /// <summary>
    /// Represents the root of the syntax tree
    /// </summary>
    public class CompilationUnit<TInstruction> : ControlFlowGraph<AstNodeBase<TInstruction>>
    {
        /// <summary>
        /// Creates a new instance of <see cref="CompilationUnit{TInstruction}"/>
        /// </summary>
        public CompilationUnit(IInstructionSetArchitecture<TInstruction> isa)
            : base(new AstInstructionSetArchitectureDecorator<TInstruction>(isa)) { }
    }
}