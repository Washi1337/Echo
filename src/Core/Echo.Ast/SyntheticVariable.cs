using System;
using Echo.Code;

namespace Echo.Ast;

/// <summary>
/// Represents a synthetic variable that was introduced after lifting a sequence of instructions into its AST
/// representation.
/// </summary>
public class SyntheticVariable : IVariable
{
    /// <summary>
    /// Creates a new synthetic variable.
    /// </summary>
    /// <param name="offset">The offset at which the synthetic variable was created.</param>
    /// <param name="index">The synthetic stack or variable slot index.</param>
    /// <param name="kind">The type of synthetic variable.</param>
    public SyntheticVariable(long offset, int index, SyntheticVariableKind kind)
    {
        Offset = offset;
        Index = index;
        Kind = kind;
        Name = GenerateName();
    }

    /// <summary>
    /// Gets the offset at which the synthetic variable was created.
    /// </summary>
    public long Offset
    {
        get;
    }

    /// <summary>
    /// Gets the synthetic stack or variable slot index. The exact semantics of the index depend on the value
    /// of <see cref="Kind"/>.
    /// </summary>
    public int Index
    {
        get;
    }

    /// <summary>
    /// Gets the type of the synthetic variable.
    /// </summary>
    public SyntheticVariableKind Kind
    {
        get;
    }

    /// <summary>
    /// Gets the name of the synthetic variable.
    /// </summary>
    public string Name
    {
        get;
    }
    
    /// <summary>
    /// Gets or sets additional user data associated to the synthetic variable.
    /// </summary>
    public object? UserData
    {
        get;
        set;
    }

    private string GenerateName()
    {
        string kind = Kind switch
        {
            SyntheticVariableKind.StackIn => "in",
            SyntheticVariableKind.StackIntermediate => "tmp",
            SyntheticVariableKind.StackOut => "out",
            _ => throw new ArgumentOutOfRangeException()
        };

        return $"{kind}_{Offset:X4}_{Index}";
    }

    /// <inheritdoc />
    public override string ToString() => Name;
}