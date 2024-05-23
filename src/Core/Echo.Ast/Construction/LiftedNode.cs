using System.Collections.Generic;
using Echo.Code;
using Echo.ControlFlow;

namespace Echo.Ast.Construction;

/// <summary>
/// Represents a single node that is in the process of being lifted to its AST representation.
/// </summary>
/// <typeparam name="TInstruction"></typeparam>
internal sealed class LiftedNode<TInstruction>
{
    private List<PhiStatement<TInstruction>>? _stackInputs;
    private Dictionary<IVariable, VariableExpression<TInstruction>>? _stackInputRefs;
    private List<SyntheticVariable>? _stackIntermediates;
    private List<SyntheticVariable>? _stackOutputs;

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
    public List<PhiStatement<TInstruction>> StackInputs => _stackInputs ??= new();

    /// <summary>
    /// Gets a collection of synthetic intermediate stack variables defined by this lifted block.
    /// </summary>
    public List<SyntheticVariable> StackIntermediates => _stackIntermediates ??= new();

    /// <summary>
    /// Gets an ordered list of synthetic output stack variables this lifted block produces.
    /// Variables are in push-order, that is, the last variable in this list represents the top-most stack value.
    /// </summary>
    public List<SyntheticVariable> StackOutputs => _stackOutputs ??= new();

    /// <summary>
    /// Gets a mapping from stack input variables to the expression the stack value is used.
    /// </summary>
    public Dictionary<IVariable, VariableExpression<TInstruction>> StackInputReferences => _stackInputRefs ??= new();
    
    /// <summary>
    /// Defines a new synthetic stack input.
    /// </summary>
    /// <returns>The variable representing the stack input.</returns>
    public SyntheticVariable DeclareStackInput()
    {
        var result = new SyntheticVariable(Original.Offset, StackInputs.Count, SyntheticVariableKind.StackIn);
        var phiStatement = new PhiStatement<TInstruction>(result)
        {
            OriginalRange = new AddressRange(Original.Offset, Original.Offset)
        };
        
        StackInputs.Insert(0, phiStatement);
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