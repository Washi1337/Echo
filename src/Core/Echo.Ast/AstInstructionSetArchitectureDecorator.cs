using System;
using Echo.Core.Code;
using Echo.Core.Graphing.Analysis.Traversal;

namespace Echo.Ast
{
    /// <summary>
    /// Provides a decorator around <typeparamref name="TInstruction"/> for the AST
    /// </summary>
    /// <typeparam name="TInstruction">The instruction</typeparam>
    public class AstInstructionSetArchitectureDecorator<TInstruction>
        : IInstructionSetArchitecture<AstStatementBase<TInstruction>>
    {
        private readonly IInstructionSetArchitecture<TInstruction> _isa;

        /// <summary>
        /// Create a new decorator around the <paramref name="isa"/>
        /// </summary>
        /// <param name="isa">The <see cref="IInstructionSetArchitecture{TInstruction}"/> to decorate</param>
        public AstInstructionSetArchitectureDecorator(IInstructionSetArchitecture<TInstruction> isa)
        {
            _isa = isa;
        }

        /// <inheritdoc />
        public long GetOffset(in AstStatementBase<TInstruction> instruction) => instruction.Id;

        /// <inheritdoc />
        public int GetSize(in AstStatementBase<TInstruction> instruction) => 1;

        /// <inheritdoc />
        public InstructionFlowControl GetFlowControl(in AstStatementBase<TInstruction> instruction) => 0;
        
        /// <inheritdoc />
        public int GetStackPushCount(in AstStatementBase<TInstruction> instruction) => 0;

        /// <inheritdoc />
        public int GetStackPopCount(in AstStatementBase<TInstruction> instruction) => 0;

        /// <inheritdoc />
        public int GetReadVariablesCount(in AstStatementBase<TInstruction> instruction)
        {
            var visitor = new ReadVariableFinderVisitor<TInstruction>();
            instruction.Accept(visitor, null);

            return visitor.Count;
        }

        /// <inheritdoc />
        public int GetReadVariables(in AstStatementBase<TInstruction> instruction, Span<IVariable> variablesBuffer)
        {
            var visitor = new ReadVariableFinderVisitor<TInstruction>();
            instruction.Accept(visitor, null);
            
            visitor.Variables.CopyTo(variablesBuffer);
            return visitor.Count;
        }

        /// <inheritdoc />
        public int GetWrittenVariablesCount(in AstStatementBase<TInstruction> instruction)
        {
            var visitor = new WrittenVariableFinderVisitor<TInstruction>();
            instruction.Accept(visitor, null);

            return visitor.Count;
        }

        /// <inheritdoc />
        public int GetWrittenVariables(in AstStatementBase<TInstruction> instruction, Span<IVariable> variablesBuffer)
        {
            var visitor = new WrittenVariableFinderVisitor<TInstruction>();
            instruction.Accept(visitor, null);
            
            visitor.Variables.CopyTo(variablesBuffer);
            return visitor.Count;
        }
    }
}