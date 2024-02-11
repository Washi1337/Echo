namespace Echo.Ast;

/// <summary>
/// Provides members describing all possible synthetic variable kinds.
/// </summary>
public enum SyntheticVariableKind
{
    /// <summary>
    /// Indicates the variable represents a stack input of a basic block.
    /// </summary>
    StackIn,
    
    /// <summary>
    /// Indicates the variable represents an intermediate stack value that was pushed onto the stack in the middle
    /// of a basic block.
    /// </summary>
    StackIntermediate,
    
    /// <summary>
    /// Indicates the variable represents a stack output of a basic block.
    /// </summary>
    StackOut,
}