using System.Collections.Generic;
using Echo.ControlFlow;

namespace Echo.Ast.Construction;

/// <summary>
/// Represents a single node that is in the process of being lifted to its AST representation.
/// </summary>
/// <typeparam name="TInstruction"></typeparam>
internal sealed class LiftedNode<TInstruction>
{
    /// <summary>
    /// Creates a new lifted node based on the provided base node.
    /// </summary>
    /// <param name="original">The node.</param>
    public LiftedNode(ControlFlowNode<TInstruction> original)
    {
        Original = original;
        Transformed = new ControlFlowNode<Statement<TInstruction>>(original.Offset);
    }

    /// <summary>
    /// Gets the original control flow node that this lifted node was based on.
    /// </summary>
    public ControlFlowNode<TInstruction> Original { get; }

    /// <summary>
    /// Gets the new node that is being produced.
    /// </summary>
    public ControlFlowNode<Statement<TInstruction>> Transformed { get; }

    /// <summary>
    /// Gets an ordered list of stack input variables (and their data sources) defined by this lifted block.
    /// Variables are in push-order, that is, the last variable in this list represents the top-most stack value.
    /// </summary>
    public List<PhiStatement<TInstruction>> StackInputs { get; } = new();

    /// <summary>
    /// Gets a collection of synthetic intermediate stack variables defined by this lifted block.
    /// </summary>
    public List<SyntheticVariable> StackIntermediates { get; } = new();

    /// <summary>
    /// Gets an ordered list of synthetic output stack variables this lifted block produces.
    /// Variables are in push-order, that is, the last variable in this list represents the top-most stack value.
    /// </summary>
    public List<SyntheticVariable> StackOutputs { get; } = new();

    /// <summary>
    /// Defines a new synthetic stack input.
    /// </summary>
    /// <returns>The variable representing the stack input.</returns>
    public SyntheticVariable DeclareStackInput()
    {
        var result = new SyntheticVariable(Original.Offset, StackInputs.Count, SyntheticVariableKind.StackIn);
        StackInputs.Insert(0, new PhiStatement<TInstruction>(result));
        return result;
    }

    /// <summary>
    /// Defines a new synthetic stack intermediate variable.
    /// </summary>
    /// <returns>The variable representing the stack value.</returns>
    public SyntheticVariable DeclareStackIntermediate()
    {
        var result = new SyntheticVariable(Original.Offset, StackIntermediates.Count, SyntheticVariableKind.StackIntermediate);
        StackIntermediates.Add(result);
        return result;
    }

    /// <summary>
    /// Defines a new synthetic stack output variable.
    /// </summary>
    /// <returns>The variable representing the stack value.</returns>
    public SyntheticVariable DeclareStackOutput()
    {
        var result = new SyntheticVariable(Original.Offset, StackOutputs.Count, SyntheticVariableKind.StackOut);
        StackOutputs.Add(result);
        return result;
    }
}