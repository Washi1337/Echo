using Echo.Code;
using Echo.Graphing;

namespace Echo.Ast
{
    /// <summary>
    /// Represents a single node in an Abstract Syntax Tree (AST).
    /// </summary>
    public abstract class AstNode<TInstruction> : TreeNodeBase
    {
        /// <summary>
        /// Gets the direct parent of the AST node. 
        /// </summary>
        public new AstNode<TInstruction>? Parent => base.Parent as AstNode<TInstruction>;
        
        /// <summary>
        /// Gets or sets the original address range this AST node mapped to in the raw disassembly of the code
        /// (if available).
        /// </summary>
        public AddressRange? OriginalRange
        {
            get;
            set;
        }

        /// <summary>
        /// Obtains the parent compilation unit the AST is added to (if available).
        /// </summary>
        /// <returns>The compilation unit, or <c>null</c> if the node is detached from any compilation unit.</returns>
        public CompilationUnit<TInstruction>? GetParentCompilationUnit()
        {
            var current = this;
            while (current is not CompilationUnit<TInstruction> unit && current.Parent is not null)
                current = current.Parent;
            return current as CompilationUnit<TInstruction>;
        }
        
        /// <summary>
        /// Determines whether the AST node consists of only pure expressions that do not affect state.
        /// </summary>
        /// <param name="classifier">The object responsible for classifying individual instructions for purity</param>
        /// <returns>
        /// <c>true</c> if the node is fully pure, <c>false</c> if fully impure, and <c>Unknown</c> if purity could
        /// not be determined for (parts of) the AST node.
        /// </returns>
        public Trilean IsPure(IPurityClassifier<TInstruction> classifier)
        {
            return Accept(Analysis.AstPurityVisitor<TInstruction>.Instance, classifier);
        }

        /// <summary>
        /// Called when the node was added to a root compilation unit.
        /// </summary>
        /// <param name="newRoot">The new root compilation unit of the node.</param>
        protected internal abstract void OnAttach(CompilationUnit<TInstruction> newRoot);
        

        /// <summary>
        /// Called when the node was removed from a root compilation unit.
        /// </summary>
        /// <param name="oldRoot">The old root compilation unit the node was removed from.</param>
        protected internal abstract void OnDetach(CompilationUnit<TInstruction> oldRoot);

        /// <inheritdoc />
        protected override void OnParentChanged(TreeNodeBase? old)
        {
            base.OnParentChanged(old);

            var oldRoot = (old as AstNode<TInstruction>)?.GetParentCompilationUnit();
            var newRoot = GetParentCompilationUnit();

            if (oldRoot != newRoot)
            {
                if (oldRoot is not null)
                    OnDetach(oldRoot);

                if (newRoot is not null)
                    OnAttach(newRoot);
            }
        }
        
        /// <summary>
        /// Implements the visitor pattern
        /// </summary>
        public abstract void Accept(IAstNodeVisitor<TInstruction> visitor);
        
        /// <summary>
        /// Implements the visitor pattern
        /// </summary>
        public abstract void Accept<TState>(IAstNodeVisitor<TInstruction, TState> visitor, TState state);

        /// <summary>
        /// Implements the visitor pattern
        /// </summary>
        public abstract TOut Accept<TState, TOut>(IAstNodeVisitor<TInstruction, TState, TOut> visitor, TState state);

        /// <inheritdoc />
        public override string ToString() => new AstFormatter<TInstruction>().Format(this);
    }
}