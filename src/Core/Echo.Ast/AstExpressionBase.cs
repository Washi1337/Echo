using System.Collections.Generic;

namespace Echo.Ast
{
    /// <summary>
    /// A base type for expression nodes in an AST
    /// </summary>
    public abstract class AstExpressionBase : AstNodeBase
    {
        /// <inheritdoc />
        public override IDictionary<string, object> UserData
        {
            get;
        } = new Dictionary<string, object>();
    }
}