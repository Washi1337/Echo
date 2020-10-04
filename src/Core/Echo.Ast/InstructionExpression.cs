using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow.Serialization.Dot;
using Echo.Core.Graphing;

namespace Echo.Ast
{
    /// <summary>
    /// Represents an instruction in the AST
    /// </summary>
    public sealed class InstructionExpression<TInstruction> : Expression<TInstruction>
    {
        /// <summary>
        /// Creates a new instruction expression node
        /// </summary>
        /// <param name="instruction">The instruction</param>
        /// <param name="arguments">The parameters to this instruction</param>
        public InstructionExpression(TInstruction instruction, IEnumerable<Expression<TInstruction>> arguments)
        {
            Instruction = instruction;
            Arguments = new TreeNodeCollection<InstructionExpression<TInstruction>, Expression<TInstruction>>(this);
            
            foreach (var argument in arguments)
                Arguments.Add(argument);
        }

        /// <summary>
        /// The instruction that the AST node represents
        /// </summary>
        public TInstruction Instruction
        {
            get;
            private set;
        }

        /// <summary>
        /// The arguments to this <see cref="InstructionExpression{TInstruction}"/>
        /// </summary>
        public IList<Expression<TInstruction>> Arguments
        {
            get;
            private set;
        }

        /// <inheritdoc />
        public override IEnumerable<TreeNodeBase> GetChildren() => Arguments;

        /// <inheritdoc />
        public override void Accept<TState>(IAstNodeVisitor<TInstruction, TState> visitor, TState state) =>
            visitor.Visit(this, state);

        /// <inheritdoc />
        public override TOut Accept<TState, TOut>(IAstNodeVisitor<TInstruction, TState, TOut> visitor, TState state) =>
            visitor.Visit(this, state);

        public InstructionExpression<TInstruction> WithInstruction(TInstruction instruction)
        {
            Instruction = instruction;
            return this;
        }

        public InstructionExpression<TInstruction> WithArguments(params Expression<TInstruction>[] arguments) =>
            WithArguments(arguments as IEnumerable<Expression<TInstruction>>);
        
        public InstructionExpression<TInstruction> WithArguments(IEnumerable<Expression<TInstruction>> arguments)
        {
            var collection = new TreeNodeCollection<InstructionExpression<TInstruction>, Expression<TInstruction>>(this);
            foreach (var argument in arguments)
                collection.Add(argument);

            Arguments = collection;
            return this;
        }

        /// <inheritdoc />
        public override string ToString() => $"{Instruction}({string.Join(", ", Arguments)})";

        internal override string Format(IInstructionFormatter<TInstruction> instructionFormatter) =>
            $"{instructionFormatter.Format(Instruction)}({string.Join(", ", Arguments)})";
    }
}
