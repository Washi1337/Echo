using Echo.Code;
using Echo.ControlFlow;
using Echo.ControlFlow.Blocks;

namespace Echo.Ast.Construction;

/// <summary>
/// Provides utility extensions for the construction of new AST control flow graphs. 
/// </summary>
public static class AstBuilder
{
    /// <summary>
    /// Lifts the instructions in every node of the provided control flow graph to expressions and statements. 
    /// </summary>
    /// <param name="cfg">The provided control flow graph to lift.</param>
    /// <param name="classifier">The object responsible for determining whether an instruction is pure or not.</param>
    /// <typeparam name="TInstruction">The type of instructions stored in the input graph.</typeparam>
    /// <returns>The lifted graph.</returns>
    public static ControlFlowGraph<Statement<TInstruction>> Lift<TInstruction>(
        this ControlFlowGraph<TInstruction> cfg,
        IPurityClassifier<TInstruction> classifier)
    {
        return ControlFlowGraphLifter<TInstruction>.Lift(cfg, classifier);
    }

    /// <summary>
    /// Lifts all instructions in every node of the provided control flow graph to expressions and statements, and
    /// constructs a rooted AST of all the resulting blocks.
    /// </summary>
    /// <param name="self">The control flow graph to lift.</param>
    /// <param name="classifier">The object responsible for determining whether an instruction is pure or not.</param>
    /// <typeparam name="TInstruction">The type of instructions stored in the input graph.</typeparam>
    /// <returns>The constructed compilation unit.</returns>
    public static CompilationUnit<TInstruction> ToCompilationUnit<TInstruction>(
        this ControlFlowGraph<TInstruction> self,
        IPurityClassifier<TInstruction> classifier)
    {
        return new CompilationUnitBuilder<TInstruction>().Construct(self.Lift(classifier));
    }

    /// <summary>
    /// Constructs a rooted AST of all the blocks containing expressions and statements.
    /// </summary>
    /// <param name="self">The control flow graph to transform.</param>
    /// <typeparam name="TInstruction">The type of instructions stored in the input graph.</typeparam>
    /// <returns>The constructed compilation unit.</returns>
    public static CompilationUnit<TInstruction> ToCompilationUnit<TInstruction>(
        this ControlFlowGraph<Statement<TInstruction>> self)
    {
        return new CompilationUnitBuilder<TInstruction>().Construct(self);
    }

    /// <summary>
    /// Converts a block tree to a rooted AST.
    /// </summary>
    /// <param name="self">The root scope to transform.</param>
    /// <typeparam name="TInstruction">The type of instructions stored in the input graph.</typeparam>
    /// <returns>The constructed compilation unit.</returns>
    public static CompilationUnit<TInstruction> ToCompilationUnit<TInstruction>(
        this ScopeBlock<Statement<TInstruction>> self)
    {
        return new CompilationUnitBuilder<TInstruction>().Construct(self);
    }
    
}