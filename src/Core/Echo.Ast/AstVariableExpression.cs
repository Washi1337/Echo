using System;
using System.Collections.Generic;
using Echo.Core.Code;

namespace Echo.Ast
{
    /// <summary>
    /// Represents a variable in an AST
    /// </summary>
    public sealed class AstVariableExpression : AstExpressionBase
    {
        /// <summary>
        /// Creates a new variable expression
        /// </summary>
        /// <param name="id"><inheritdoc cref="AstNodeBase(long)"/></param>
        /// <param name="variable">The variable</param>
        public AstVariableExpression(long id, IVariable variable)
            : base(id)
        {
            Variable = variable;
        }

        /// <summary>
        /// The variable that the node represents
        /// </summary>
        public IVariable Variable
        {
            get;
        }

        /// <inheritdoc />
        public override IEnumerable<AstNodeBase> GetChildren()
        {
            return Array.Empty<AstNodeBase>();
        }
    }
}