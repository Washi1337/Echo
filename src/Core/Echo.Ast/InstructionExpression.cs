using System.Collections.Generic;
using Echo.Graphing;

namespace Echo.Ast
{
    /// <summary>
    /// Represents an instruction in the AST
    /// </summary>
    public sealed class InstructionExpression<TInstruction> : Expression<TInstruction>
    {
        /// <summary>
        /// Creates a new instruction expression node with no arguments
        /// </summary>
        /// <param name="instruction">The instruction</param>
        public InstructionExpression(TInstruction instruction)
        {
            Instruction = instruction;
            Arguments = new TreeNodeCollection<InstructionExpression<TInstruction>, Expression<TInstruction>>(this);
        }

        /// <summary>
        /// Creates a new instruction expression node
        /// </summary>
        /// <param name="instruction">The instruction</param>
        /// <param name="arguments">The parameters to the <paramref name="instruction" /></param>
        public InstructionExpression(TInstruction instruction, params Expression<TInstruction>[] arguments)
            : this(instruction, arguments as IEnumerable<Expression<TInstruction>>)
        {
        }

        /// <summary>
        /// Creates a new instruction expression node
        /// </summary>
        /// <param name="instruction">The instruction</param>
        /// <param name="arguments">The parameters to <paramref name="instruction" /></param>
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
        }

        /// <inheritdoc />
        public override IEnumerable<TreeNodeBase> GetChildren() => Arguments;

        /// <inheritdoc />
        protected internal override void OnAttach(CompilationUnit<TInstruction> newRoot)
        {
            for (int i = 0; i < Arguments.Count; i++)
                Arguments[i].OnAttach(newRoot);
        }

        /// <inheritdoc />
        protected internal override void OnDetach(CompilationUnit<TInstruction> oldRoot)
        {
            for (int i = 0; i < Arguments.Count; i++)
                Arguments[i].OnDetach(oldRoot);
        }

        /// <inheritdoc />
        public override void Accept(IAstNodeVisitor<TInstruction> visitor) 
            => visitor.Visit(this);

        /// <inheritdoc />
        public override void Accept<TState>(IAstNodeVisitor<TInstruction, TState> visitor, TState state) =>
            visitor.Visit(this, state);

        /// <inheritdoc />
        public override TOut Accept<TState, TOut>(IAstNodeVisitor<TInstruction, TState, TOut> visitor, TState state) =>
            visitor.Visit(this, state);

        /// <summary>
        /// Modifies the current <see cref="InstructionExpression{TInstruction}"/> to have the <paramref name="instruction"/>
        /// </summary>
        /// <param name="instruction">The instruction</param>
        /// <returns>The same <see cref="Instruction"/> instance but with the new <paramref name="instruction"/></returns>
        public InstructionExpression<TInstruction> WithInstruction(TInstruction instruction)
        {
            Instruction = instruction;
            return this;
        }

        /// <summary>
        /// Modifies the current <see cref="InstructionExpression{TInstruction}"/>'s <paramref name="arguments"/>
        /// </summary>
        /// <param name="arguments">The arguments</param>
        /// <returns>The same <see cref="InstructionExpression{TInstruction}"/> instance but with the new <paramref name="arguments"/></returns>
        public InstructionExpression<TInstruction> WithArguments(params Expression<TInstruction>[] arguments) =>
            WithArguments(arguments as IEnumerable<Expression<TInstruction>>);
        
        /// <summary>
        /// Modifies the current <see cref="InstructionExpression{TInstruction}"/>'s <paramref name="arguments"/>
        /// </summary>
        /// <param name="arguments">The arguments</param>
        /// <returns>The same <see cref="InstructionExpression{TInstruction}"/> instance but with the new <paramref name="arguments"/></returns>
        public InstructionExpression<TInstruction> WithArguments(IEnumerable<Expression<TInstruction>> arguments)
        {
            Arguments.Clear();
            
            foreach (var argument in arguments)
                Arguments.Add(argument);

            return this;
        }
    }
}