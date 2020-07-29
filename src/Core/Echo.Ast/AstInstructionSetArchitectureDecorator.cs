using System;
using Echo.Core.Code;
using Echo.Core.Graphing.Analysis.Traversal;

namespace Echo.Ast
{
    /// <summary>
    /// Provides a decorator around <typeparamref name="TInstruction"/> for the AST
    /// </summary>
    /// <typeparam name="TInstruction">The instruction</typeparam>
    public class AstInstructionSetArchitectureDecorator<TInstruction> : IInstructionSetArchitecture<AstNodeBase<TInstruction>>
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
        public long GetOffset(in AstNodeBase<TInstruction> instruction) => instruction.Id;

        /// <inheritdoc />
        public int GetSize(in AstNodeBase<TInstruction> instruction) => 1;

        /// <inheritdoc />
        public InstructionFlowControl GetFlowControl(in AstNodeBase<TInstruction> instruction) => 0;
        
        /// <inheritdoc />
        public int GetStackPushCount(in AstNodeBase<TInstruction> instruction) => 0;

        /// <inheritdoc />
        public int GetStackPopCount(in AstNodeBase<TInstruction> instruction) => 0;

        /// <inheritdoc />
        public int GetReadVariablesCount(in AstNodeBase<TInstruction> instruction)
        {
            var visitor = new UniqueVariableFinderVisitor<TInstruction>();
            instruction.Accept(visitor, null);

            return visitor.Count;
        }

        /// <inheritdoc />
        public int GetReadVariables(in AstNodeBase<TInstruction> instruction, Span<IVariable> variablesBuffer)
        {
            var visitor = new UniqueVariableFinderVisitor<TInstruction>();
            instruction.Accept(visitor, null);
            
            visitor.Variables.CopyTo(variablesBuffer);
            return visitor.Count;
        }

        /// <inheritdoc />
        public int GetWrittenVariablesCount(in AstNodeBase<TInstruction> instruction) =>
            throw new NotImplementedException();

        /// <inheritdoc />
        public int GetWrittenVariables(in AstNodeBase<TInstruction> instruction, Span<IVariable> variablesBuffer) =>
            throw new NotImplementedException();
    }
}