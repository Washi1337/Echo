using System;
using Echo.Ast.Helpers;
using Echo.Core.Code;

namespace Echo.Ast
{
    /// <summary>
    /// Provides a decorator around <typeparamref name="TInstruction"/> for the AST
    /// </summary>
    /// <typeparam name="TInstruction">The instruction</typeparam>
    public class AstInstructionSetArchitectureDecorator<TInstruction>
        : IInstructionSetArchitecture<StatementBase<TInstruction>>
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
        public long GetOffset(in StatementBase<TInstruction> instruction) => instruction.Id;

        /// <inheritdoc />
        public int GetSize(in StatementBase<TInstruction> instruction) => 1;

        /// <inheritdoc />
        // TODO: Return proper flow
        public InstructionFlowControl GetFlowControl(in StatementBase<TInstruction> instruction) => 0;
        
        /// <inheritdoc />
        public int GetStackPushCount(in StatementBase<TInstruction> instruction) => 0;

        /// <inheritdoc />
        public int GetStackPopCount(in StatementBase<TInstruction> instruction) => 0;

        /// <inheritdoc />
        public int GetReadVariablesCount(in StatementBase<TInstruction> instruction)
        {
            var visitor = new ReadVariableFinderVisitor<TInstruction>();
            instruction.Accept(visitor, null);

            return visitor.Count;
        }

        /// <inheritdoc />
        public int GetReadVariables(in StatementBase<TInstruction> instruction, Span<IVariable> variablesBuffer)
        {
            var visitor = new ReadVariableFinderVisitor<TInstruction>();
            instruction.Accept(visitor, null);
            
            visitor.Variables.CopyTo(variablesBuffer);
            return visitor.Count;
        }

        /// <inheritdoc />
        public int GetWrittenVariablesCount(in StatementBase<TInstruction> instruction)
        {
            var visitor = new WrittenVariableFinderVisitor<TInstruction>();
            instruction.Accept(visitor, null);

            return visitor.Count;
        }

        /// <inheritdoc />
        public int GetWrittenVariables(in StatementBase<TInstruction> instruction, Span<IVariable> variablesBuffer)
        {
            var visitor = new WrittenVariableFinderVisitor<TInstruction>();
            instruction.Accept(visitor, null);
            
            visitor.Variables.CopyTo(variablesBuffer);
            return visitor.Count;
        }
    }
}