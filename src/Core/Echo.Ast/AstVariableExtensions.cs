using System.Collections.Generic;
using Echo.Code;

namespace Echo.Ast;

/// <summary>
/// Provides a mechanism for cross-referencing variables in a compilation unit.
/// </summary>
public static class AstVariableExtensions
{
    /// <summary>
    /// Gets all expressions in the compilation unit that reference the provided variable.
    /// </summary>
    /// <param name="self">The variable to cross-reference.</param>
    /// <param name="unit">The compilation unit to cross-reference in.</param>
    /// <returns>The expressions referencing the variable.</returns>
    public static IReadOnlyList<VariableExpression<TInstruction>> GetIsUsedBy<TInstruction>(
        this IVariable self,
        CompilationUnit<TInstruction> unit)
        where TInstruction : notnull
    {
        return unit.GetVariableUses(self);
    } 
    
    /// <summary>
    /// Gets all statements in the compilation unit that write to the provided variable.
    /// </summary>
    /// <param name="self">The variable to cross-reference.</param>
    /// <param name="unit">The compilation unit to cross-reference in.</param>
    /// <returns>The statements writing to the variable.</returns>
    public static IReadOnlyList<Statement<TInstruction>> GetIsWrittenBy<TInstruction>(
        this IVariable self,
        CompilationUnit<TInstruction> unit)
        where TInstruction : notnull
    {
        return unit.GetVariableWrites(self);
    } 
}