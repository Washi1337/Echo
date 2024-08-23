using System.Collections.Generic;

namespace Echo.Code;

/// <summary>
/// Provides convenience extensions for the <see cref="IArchitecture{TInstruction}"/> interface.
/// </summary>
public static class ArchitectureExtensions
{
    /// <summary>
    /// Gets a collection of variables that an instruction reads from.
    /// </summary>
    /// <param name="self">The architecture.</param>
    /// <param name="instruction">The instruction to get the variables from.</param>
    /// <returns>The list of variables.</returns>
    public static IList<IVariable> GetReadVariables<TInstruction>(
        this IArchitecture<TInstruction> self,
        in TInstruction instruction) 
        where TInstruction : notnull
    {
        var result = new List<IVariable>();
        self.GetReadVariables(in instruction, result);
        return result;
    }
    
    /// <summary>
    /// Gets a collection of variables that an instruction writes to.
    /// </summary>
    /// <param name="self">The architecture.</param>
    /// <param name="instruction">The instruction to get the variables from.</param>
    /// <returns>The list of variables.</returns>
    public static IList<IVariable> GetWrittenVariables<TInstruction>(
        this IArchitecture<TInstruction> self,
        in TInstruction instruction) 
        where TInstruction : notnull
    {
        var result = new List<IVariable>();
        self.GetWrittenVariables(in instruction, result);
        return result;
    }
}