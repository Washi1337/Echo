using System;
using Echo.Ast.Helpers;
using Echo.Core.Code;

namespace Echo.Ast
{
    /// <summary>
    /// Provides a decorator around <typeparamref name="TInstruction"/> for the AST
    /// </summary>
    /// <typeparam name="TInstruction">The instruction</typeparam>
    public class AstArchitecture<TInstruction>
        : IInstructionSetArchitecture<Statement<TInstruction>>
    {
        private readonly FlowControlDeterminer<TInstruction> _flowControlDeterminer;

        /// <summary>
        /// Create a new decorator around the <paramref name="isa"/>
        /// </summary>
        /// <param name="isa">The <see cref="IInstructionSetArchitecture{TInstruction}"/> to decorate</param>
        public AstArchitecture(IInstructionSetArchitecture<TInstruction> isa) =>
            _flowControlDeterminer = new FlowControlDeterminer<TInstruction>(isa);

        /// <inheritdoc />
        public long GetOffset(in Statement<TInstruction> instruction) => throw new NotSupportedException();

        /// <inheritdoc />
        public int GetSize(in Statement<TInstruction> instruction) => 1;

        /// <inheritdoc />
        public InstructionFlowControl GetFlowControl(in Statement<TInstruction> instruction) =>
            instruction.Accept(_flowControlDeterminer, null);
        
        /// <inheritdoc />
        public int GetStackPushCount(in Statement<TInstruction> instruction) => 0;

        /// <inheritdoc />
        public int GetStackPopCount(in Statement<TInstruction> instruction) => 0;

        /// <inheritdoc />
        public int GetReadVariablesCount(in Statement<TInstruction> instruction)
        {
            var visitor = new ReadVariableFinderWalker<TInstruction>();
            instruction.Accept(visitor, null);

            return visitor.Count;
        }

        /// <inheritdoc />
        public int GetReadVariables(in Statement<TInstruction> instruction, Span<IVariable> variablesBuffer)
        {
            var visitor = new ReadVariableFinderWalker<TInstruction>();
            instruction.Accept(visitor, null);
            
            visitor.Variables.CopyTo(variablesBuffer);
            return visitor.Count;
        }

        /// <inheritdoc />
        public int GetWrittenVariablesCount(in Statement<TInstruction> instruction)
        {
            var visitor = new WrittenVariableFinderWalker<TInstruction>();
            instruction.Accept(visitor, null);

            return visitor.Count;
        }

        /// <inheritdoc />
        public int GetWrittenVariables(in Statement<TInstruction> instruction, Span<IVariable> variablesBuffer)
        {
            var visitor = new WrittenVariableFinderWalker<TInstruction>();
            instruction.Accept(visitor, null);
            
            visitor.Variables.CopyTo(variablesBuffer);
            return visitor.Count;
        }
    }
}