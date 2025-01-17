using System;
using System.Collections.Generic;
using System.Linq;
using Echo.Code;
using Echo.Graphing;

namespace Echo.Ast;

/// <summary>
/// Represents a single compilation unit of code. This is the root node of any AST.
/// </summary>
/// <typeparam name="TInstruction">The type of instructions to store in the AST.</typeparam>
public class CompilationUnit<TInstruction> : AstNode<TInstruction>
    where TInstruction : notnull
{
    private readonly Dictionary<IVariable, List<VariableExpression<TInstruction>>> _variableUses = new();
    private readonly Dictionary<IVariable, List<Statement<TInstruction>>> _variableWrites = new();
    private readonly HashSet<IVariable> _variables = new();
        
    private BlockStatement<TInstruction> _root = null!;

    /// <summary>
    /// Creates a new empty compilation unit.
    /// </summary>
    public CompilationUnit()
    {
        Root = new BlockStatement<TInstruction>();
    }
    
    /// <summary>
    /// Gets the root scope of the compilation unit.
    /// </summary>
    public BlockStatement<TInstruction> Root
    {
        get => _root;
        internal set => UpdateChildNotNull(ref _root, value);
    }

    /// <summary>
    /// Gets a collection of all referenced variables in the compilation unit.
    /// </summary>
    public IReadOnlyCollection<IVariable> Variables => _variables;

    /// <summary>
    /// Gets all expressions in the compilation unit that reference the provided variable.
    /// </summary>
    /// <param name="variable">The variable to cross-reference.</param>
    /// <returns>The expressions referencing the variable.</returns>
    public IReadOnlyList<VariableExpression<TInstruction>> GetVariableUses(IVariable variable)
    {
        return !_variableUses.TryGetValue(variable, out var uses)
            ? Array.Empty<VariableExpression<TInstruction>>()
            : uses.ToArray(); // Clone to prevent list being modified after returning.
    }

    /// <summary>
    /// Gets all statements in the compilation unit that write to the provided variable.
    /// </summary>
    /// <param name="variable">The variable to cross-reference.</param>
    /// <returns>The statements writing to the variable.</returns>
    public IReadOnlyList<Statement<TInstruction>> GetVariableWrites(IVariable variable)
    {
        return !_variableWrites.TryGetValue(variable, out var writes) 
            ? Array.Empty<Statement<TInstruction>>()
            : writes.ToArray(); // Clone to prevent list being modified after returning.
    }

    internal void RegisterVariableUse(VariableExpression<TInstruction> expression)
    {
        if (!_variableUses.TryGetValue(expression.Variable, out var uses))
        {
            uses = new List<VariableExpression<TInstruction>>();
            _variableUses.Add(expression.Variable, uses);
        }
        
        uses.Add(expression);
        _variables.Add(expression.Variable);
    }

    internal void UnregisterVariableUse(VariableExpression<TInstruction> expression)
    {
        if (_variableUses.TryGetValue(expression.Variable, out var uses))
        {
            uses.Remove(expression);
            if (uses.Count == 0)
            {
                _variableUses.Remove(expression.Variable);
                if (!_variableWrites.ContainsKey(expression.Variable))
                    _variables.Remove(expression.Variable);
            }
        }
    }

    internal void RegisterVariableWrite(IVariable variable, Statement<TInstruction> statement)
    {
        if (!_variableWrites.TryGetValue(variable, out var writes))
        {
            writes = new List<Statement<TInstruction>>();
            _variableWrites.Add(variable, writes);
        }

        writes.Add(statement);
        _variables.Add(variable);
    }

    internal void UnregisterVariableWrite(IVariable variable, Statement<TInstruction> statement)
    {
        if (_variableWrites.TryGetValue(variable, out var writes))
        {
            writes.Remove(statement);
            if (writes.Count == 0)
            {
                _variableWrites.Remove(variable);
                if (!_variableUses.ContainsKey(variable))
                    _variables.Remove(variable);
            }
        }
    }

    /// <inheritdoc />
    public override IEnumerable<TreeNodeBase> GetChildren() => new[] {Root};

    /// <inheritdoc />
    protected internal override void OnAttach(CompilationUnit<TInstruction> newRoot) => 
        throw new InvalidOperationException();

    /// <inheritdoc />
    protected internal override void OnDetach(CompilationUnit<TInstruction> oldRoot) => 
        throw new InvalidOperationException();

    /// <inheritdoc />
    public override void Accept(IAstNodeVisitor<TInstruction> visitor) => visitor.Visit(this);

    /// <inheritdoc />
    public override void Accept<TState>(IAstNodeVisitor<TInstruction, TState> visitor, TState state) =>
        visitor.Visit(this, state);

    /// <inheritdoc />
    public override TOut Accept<TState, TOut>(IAstNodeVisitor<TInstruction, TState, TOut> visitor, TState state) =>
        visitor.Visit(this, state);
}