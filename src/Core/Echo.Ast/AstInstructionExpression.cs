using System.Collections.Generic;

namespace Echo.Ast
{
    /// <summary>
    /// Represents an instruction in an AST
    /// </summary>
    /// <typeparam name="TInstruction">The type of the instruction</typeparam>
    public sealed class AstInstructionExpression<TInstruction> : AstExpressionBase
    {
        /// <summary>
        /// Creates a new instruction expression
        /// </summary>
        /// <param name="id"><inheritdoc cref="AstNodeBase(long)"/></param>
        /// <param name="instruction">The instruction</param>
        /// <param name="arguments">Arguments of the expression</param>
        public AstInstructionExpression(long id, TInstruction instruction, IList<AstExpressionBase> arguments)
            : base(id)
        {
            Instruction = instruction;
            Arguments = arguments;
        }
        
        /// <summary>
        /// The instruction that the node represents
        /// </summary>
        public TInstruction Instruction
        {
            get;
        }
        
        /// <summary>
        /// The expression's arguments
        /// </summary>
        public IList<AstExpressionBase> Arguments
        {
            get;
        }
        
        /// <inheritdoc />
        public override IEnumerable<AstNodeBase> GetChildren()
        {
            return Arguments;
        }
    }
}