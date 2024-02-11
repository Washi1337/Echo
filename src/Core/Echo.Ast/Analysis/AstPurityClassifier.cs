using Echo.Code;

namespace Echo.Ast.Analysis;

/// <summary>
/// Provides a wrapper around a <see cref="IPurityClassifier{TInstruction}"/> that is able to classify statements
/// and expressions with <typeparamref name="TInstruction"/> instructions by purity. 
/// </summary>
/// <typeparam name="TInstruction">The type of instructions the statements store.</typeparam>
public class AstPurityClassifier<TInstruction> : IPurityClassifier<Statement<TInstruction>>
{
    /// <summary>
    /// Creates a new instance of the <see cref="AstPurityClassifier{TInstruction}"/> class.
    /// </summary>
    /// <param name="baseClassifier">The base classifier to use for classifying individual instructions in the AST.</param>
    public AstPurityClassifier(IPurityClassifier<TInstruction> baseClassifier)
    {
        BaseClassifier = baseClassifier;
    }

    /// <summary>
    /// Gets the base classifier to use for classifying individual instructions in the AST.
    /// </summary>
    public IPurityClassifier<TInstruction> BaseClassifier
    {
        get;
    }

    /// <inheritdoc />
    public Trilean IsPure(in Statement<TInstruction> instruction)
    {
        return instruction.Accept(AstPurityVisitor<TInstruction>.Instance, BaseClassifier);
    }
}