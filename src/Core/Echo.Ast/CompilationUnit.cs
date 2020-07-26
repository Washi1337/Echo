using System.Collections.Generic;
using Echo.ControlFlow;

namespace Echo.Ast
{
    /// <summary>
    /// Represents the root of the syntax tree
    /// </summary>
    public class CompilationUnit : ControlFlowGraph<AstNodeBase>
    {
        /// <summary>
        /// Creates a new instance of <see cref="CompilationUnit"/>
        /// </summary>
        public CompilationUnit(long id)
            : base(id) { }

        /// <summary>
        /// All the children of the <see cref="CompilationUnit"/>
        /// </summary>
        public ICollection<AstNodeBase> Children
        {
            get;
        } = new List<AstNodeBase>();

        public override IEnumerable<AstNodeBase> GetChildren()
        {
            return Children;
        }
    }
}