using Echo.Code;
using Echo.ControlFlow;

namespace Echo.Ast.Construction;

/// <summary>
/// Provides utility extensions for the construction of new AST control flow graphs. 
/// </summary>
public static class AstBuilderExtensions
{
    /// <summary>
    /// Lifts the instructions in every node of the provided control flow graph to expressions and statements. 
    /// </summary>
    /// <param name="cfg">The provided control flow graph to lift.</param>
    /// <param name="classifier">The object responsible for determining whether an instruction is pure or not.</param>
    /// <typeparam name="TInstruction">The type of instructions stored in the input graph.</typeparam>
    /// <returns>The lifted graph.</returns>
    public static ControlFlowGraph<Statement<TInstruction>> ToAst<TInstruction>(
        this ControlFlowGraph<TInstruction> cfg,
        IPurityClassifier<TInstruction> classifier)
    {
        return AstBuilder<TInstruction>.Lift(cfg, classifier);
    }
}