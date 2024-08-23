using System;
using System.Collections.Generic;
using System.Linq;
using Echo.Code;
using Echo.ControlFlow;
using Echo.ControlFlow.Regions;

namespace Echo.Ast.Construction;

/// <summary>
/// Lifts every node in a control flow graph to its AST representation. 
/// </summary>
public sealed class ControlFlowGraphLifter<TInstruction>
    where TInstruction : notnull
{
    private readonly ControlFlowGraph<TInstruction> _original;
    private readonly ControlFlowGraph<Statement<TInstruction>> _lifted;
    private readonly IPurityClassifier<TInstruction> _purityClassifier;
    private readonly Dictionary<ControlFlowNode<TInstruction>, LiftedNode<TInstruction>> _liftedNodes = new();
    
    private ControlFlowGraphLifter(ControlFlowGraph<TInstruction> original, IPurityClassifier<TInstruction> purityClassifier)
    {
        _original = original;
        _purityClassifier = purityClassifier;
        _lifted = new ControlFlowGraph<Statement<TInstruction>>(original.Architecture.ToAst());
    }

    /// <summary>
    /// Transforms a control flow graph by lifting each basic block into an AST representation. 
    /// </summary>
    /// <param name="cfg">The control flow graph.</param>
    /// <param name="purityClassifier">A classifier used for determining whether expressions are pure or not.</param>
    /// <returns>The transformed control flow graph.</returns>
    public static ControlFlowGraph<Statement<TInstruction>> Lift(
        ControlFlowGraph<TInstruction> cfg,
        IPurityClassifier<TInstruction> purityClassifier)
    {
        var builder = new ControlFlowGraphLifter<TInstruction>(cfg, purityClassifier);
        builder.Run();
        return builder._lifted;
    }
        
    private void Run()
    {
        // Strategy:
        //
        // We lift each node in the control flow graph in isolation. Control flow nodes have the property that they 
        // are executed as a single unit, and as such, within a single node we can assume an isolated eval stack.
        // This avoids full data flow analysis (i.e., building a full DFG) to build ASTs, at the cost of a slight
        // chance of overproducing some synthetic variables to communicate non-zero stack deltas between cfg nodes.
        // In practice however this is much rarer to occur than not: Typically a block has a stack delta of 0.
        //
        // We only use SSA form and PHI nodes for synthetic stack variables. This is because many languages/platforms
        // allow for variables to be accessed by reference and thus it is not clear just from the access alone whether
        // it is just a read or also a write. We leave this decision up to the consumer of this API.
        
        LiftNodes();
        PopulatePhiStatements();
        InsertPhiStatements();
        AddEdges();
        TransformRegions();
    }

    private void LiftNodes()
    {
        foreach (var node in _original.Nodes)
        {
            var liftedNode = LiftNode(node);
            _liftedNodes.Add(node, liftedNode);
            _lifted.Nodes.Add(liftedNode.Transformed);
        }
    }

    private LiftedNode<TInstruction> LiftNode(ControlFlowNode<TInstruction> node)
    {
        var result = new LiftedNode<TInstruction>(node);

        // For each node we keep track of a local stack.
        var stack = new Stack<Expression<TInstruction>>();
        
        // Lift every instruction, processing stack states.
        for (int i = 0; i < node.Contents.Instructions.Count; i++)
            LiftInstruction(result, node.Contents.Instructions[i], stack);

        // Any values left on the stack we move into synthetic out variables.
        FlushStackAsOutput(result, stack);
        
        return result;
    }

    private void LiftInstruction(
        LiftedNode<TInstruction> node,
        in TInstruction instruction,
        Stack<Expression<TInstruction>> stack)
    {
        var architecture = _original.Architecture;
        long startOffset = architecture.GetOffset(instruction);

        // Wrap the instruction into an expression.
        var expression = Expression.Instruction(instruction);
        expression.OriginalRange = new AddressRange(
            startOffset,
            startOffset + architecture.GetSize(instruction)
        );
        
        // Determine the arguments.
        int popCount = architecture.GetStackPopCount(instruction);
        if (popCount == -1)
        {
            while (stack.Count > 0)
                node.Transformed.Contents.Instructions.Add(stack.Pop().ToStatement());
        }
        else
        {
            for (int i = 0; i < popCount; i++)
            {
                var argument = Pop(node, stack);
                expression.Arguments.Insert(0, argument);
                if (argument.OriginalRange is not null)
                    expression.OriginalRange = expression.OriginalRange.Value.Expand(argument.OriginalRange.Value);
            }
        }

        // Determine the produced values.
        switch (architecture.GetStackPushCount(instruction))
        {
            case 0:
                // No value produced means we are dealing with a new independent statement.
                
                if ((architecture.GetFlowControl(instruction) & (InstructionFlowControl.CanBranch | InstructionFlowControl.IsTerminator)) != 0)
                {
                    // If we are the final terminator or branch instruction at the end of the block, we need to flush
                    // any remaining values on the stack *before* the instruction statement to ensure the operations
                    // are evaluated before jumping to the next block.
                    FlushStackAsOutput(node, stack);
                }
                else
                {
                    // For any other case we may still need to flush the stack if the expression is potentially impure
                    // and the stack contains potentially impure items, to preserve order of impure operations.
                    FlushStackIfImpure(node, stack, expression);
                }

                // Wrap the expression into an independent statement and add it.
                node.Transformed.Contents.Instructions.Add(expression.ToStatement());
                break;

            case 1:
                // There is one value produced, push it on the local stack.
                stack.Push(expression);
                break;

            case var pushCount:
                // Multiple values are produced, move them into separate variables and push them on eval stack.
                
                // Ensure order of operations is preserved if expression is potentially impure.
                FlushStackIfImpure(node, stack, expression);
                 
                // Declare new intermediate variables.
                var variables = new IVariable[pushCount];
                for (int i = 0; i < pushCount; i++)
                    variables[i] = node.DeclareStackIntermediate();

                // Add the assignment statement.
                node.Transformed.Contents.Instructions.Add(Statement.Assignment(variables, expression));

                // Push the intermediate variables.
                foreach (var variable in variables)
                    stack.Push(variable.ToExpression<TInstruction>());

                break;
        }
    }

    private static Expression<TInstruction> Pop(LiftedNode<TInstruction> node, Stack<Expression<TInstruction>> stack)
    {
        // If there is something on the stack, we can pop it. Otherwise it is a stack-input for this basic block.
        if (stack.Count != 0)
            return stack.Pop();
        
        var variable = node.DeclareStackInput();
        var expression = variable.ToExpression<TInstruction>();
        node.StackInputReferences.Add(variable, expression);
        return expression;
    }

    private static void FlushStackAsOutput(LiftedNode<TInstruction> node, Stack<Expression<TInstruction>> stack)
    {
        FlushStackInternal(node, stack, static (n, value) =>
        {
            if (value is VariableExpression<TInstruction> {Variable: SyntheticVariable variable})
            {
                // If this output expression is already variable expression, we do not need to allocate a new variable
                // and can instead just promote the variable to a stack output variable (inlining).
                n.StackOutputs.Add(variable);
            }
            else
            {
                // Otherwise, declare and assign the value to a new stack output variable.
                variable = n.DeclareStackOutput();
                n.Transformed.Contents.Instructions.Add(Statement.Assignment(variable, value));
            }

            return variable;
        });
    }

    private static void FlushStackAndPush(LiftedNode<TInstruction> node, Stack<Expression<TInstruction>> stack)
    {
        var variables = FlushStackInternal(node, stack, static (n, value) =>
        {
            // Optimization: If we are simply reassigning variables, we can skip introducing a new variable (inlining).
            if (value is VariableExpression<TInstruction> variableExpression)
                return variableExpression.Variable;
            
            var intermediate = n.DeclareStackIntermediate();
            n.Transformed.Contents.Instructions.Add(Statement.Assignment(intermediate, value));
            return intermediate;
        });

        for (int i = 0; i < variables.Count; i++)
            stack.Push(variables[i].ToExpression<TInstruction>());
    }

    private void FlushStackIfImpure(
        LiftedNode<TInstruction> node, 
        Stack<Expression<TInstruction>> stack, 
        Expression<TInstruction> expression)
    {
        // Is this expression pure with 100% certainty?
        if (expression.IsPure(_purityClassifier).ToBooleanOrFalse())
            return;

        // Does the stack contain potentially impure expressions?
        // We then still need to flush to preserve order of operations.
        bool fullyPureStack = true;
        foreach (var value in stack)
        {
            if (!value.IsPure(_purityClassifier).ToBooleanOrFalse())
            {
                fullyPureStack = false;
                break;
            }
        }

        if (!fullyPureStack)
            FlushStackAndPush(node, stack);
    }

    private static IList<IVariable> FlushStackInternal(
        LiftedNode<TInstruction> node, 
        Stack<Expression<TInstruction>> stack,
        Func<LiftedNode<TInstruction>, Expression<TInstruction>, IVariable> flush)
    {
        // Collect all values from the stack.
        var values = new Expression<TInstruction>[stack.Count];
        for (int i = values.Length - 1; i >= 0; i--)
            values[i] = stack.Pop();
        
        // Flush them to variables.
        var variables = new IVariable[values.Length];
        for (int i = 0; i < values.Length; i++)
            variables[i] = flush(node, values[i]);

        return variables;
    }

    private void PopulatePhiStatements()
    {
        var recordedStates = new Dictionary<LiftedNode<TInstruction>, StackState<TInstruction>>();

        var agenda = new Queue<StackState<TInstruction>>();
        agenda.Enqueue(new StackState<TInstruction>(_original.EntryPoint!));

        while (agenda.Count > 0)
        {
            var currentState = agenda.Dequeue();
            var liftedNode = _liftedNodes[currentState.Node];

            if (!recordedStates.TryGetValue(liftedNode, out var previousState))
            {
                // We have never visited this block before. Register the new state.
                recordedStates[liftedNode] = currentState;
            }
            else if (previousState.MergeWith(currentState, out var mergedState))
            {
                // Merging the states resulted in a change. We have to revisit this path.
                currentState = mergedState;
                recordedStates[liftedNode] = mergedState;
            }
            else
            {
                // We did not change anything to the recorded input states, so there is no need to recompute the PHI
                // nodes nor any of its successors.
                continue;
            }

            // Consume stack values, and add them to the phi statements.
            for (int i = liftedNode.StackInputs.Count - 1; i >= 0; i--)
            {
                var input = liftedNode.StackInputs[i];
                
                // Protection against malformed code streams with stack underflow.
                if (currentState.Stack.IsEmpty)
                    break;

                currentState = currentState.Pop(out var value);
                foreach (var source in value.Sources)
                {
                    if (!input.HasSource(source))
                        input.Sources.Add(source.ToExpression<TInstruction>());
                }
            }

            // Push new values on stack.
            foreach (var output in liftedNode.StackOutputs)
                currentState = currentState.Push(new StackSlot(output));

            // Schedule successors.
            foreach (var successor in currentState.Node.GetSuccessors())
                agenda.Enqueue(currentState.MoveTo(successor));
        }
    }

    private void InsertPhiStatements()
    {
        foreach (var block in _liftedNodes.Values)
        {
            for (int i = block.StackInputs.Count - 1; i >= 0; i--)
            {
                var input = block.StackInputs[i];
                if (input.Sources.Count == 1)
                {
                    // Optimization: if there is one source only for the phi node, we can inline the input stack
                    // variable. Since an input stack slot is only consumed once, it thus only has one variable
                    // expression. Therefore, inlining is exactly one update of a variable expression.
                    
                    var singleSource = input.Sources[0];
                    input.Sources.RemoveAt(0);
                    
                    if (block.StackInputReferences.TryGetValue(input.Representative, out var expression))
                        expression.Variable = singleSource.Variable;
                }
                else
                {
                    // Insert the phi node in front of the block.
                    block.Transformed.Contents.Instructions.Insert(0, input);
                }
            }
        }
    }

    private void AddEdges()
    {
        foreach (var lifted in _liftedNodes.Values)
        {
            if (lifted.Original.UnconditionalEdge is { } x)
                lifted.Transformed.ConnectWith(_liftedNodes[x.Target].Transformed, x.Type);

            foreach (var edge in lifted.Original.ConditionalEdges)
                lifted.Transformed.ConnectWith(_liftedNodes[edge.Target].Transformed, ControlFlowEdgeType.Conditional);
            
            foreach (var edge in lifted.Original.AbnormalEdges)
                lifted.Transformed.ConnectWith(_liftedNodes[edge.Target].Transformed, ControlFlowEdgeType.Abnormal);
        }
    }

    private void TransformRegions()
    {
        _lifted.EntryPoint = _liftedNodes[_original.EntryPoint!].Transformed;
        foreach (var region in _original.Regions)
            TransformRegion(x => _lifted.Regions.Add(x), region);
    }

    private void TransformRegion(
        Action<ControlFlowRegion<Statement<TInstruction>>> addSection,
        ControlFlowRegion<TInstruction> region
    )
    {
        switch (region)
        {
            case ScopeRegion<TInstruction> scopeRegion:
                var newBasicRegion = new ScopeRegion<Statement<TInstruction>>();
                addSection(newBasicRegion);
                
                TransformScope(scopeRegion, newBasicRegion);
                break;

            case ExceptionHandlerRegion<TInstruction> ehRegion:
                var newEhRegion = new ExceptionHandlerRegion<Statement<TInstruction>>();
                addSection(newEhRegion);

                TransformScope(ehRegion.ProtectedRegion, newEhRegion.ProtectedRegion);
                foreach (var subRegion in ehRegion.Handlers)
                    TransformHandlerRegion(newEhRegion, subRegion);

                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(region));
        }
    }

    private void TransformScope(ScopeRegion<TInstruction> scopeRegion, ScopeRegion<Statement<TInstruction>> newScopeRegion)
    {
        // Add the lifted nodes within the current scope.
        foreach (var node in scopeRegion.Nodes)
            newScopeRegion.Nodes.Add(_liftedNodes[node].Transformed);

        // Set up entry point.
        newScopeRegion.EntryPoint = _liftedNodes[scopeRegion.EntryPoint!].Transformed;
        
        // Recursively traverse the region tree.   
        TransformSubRegions(scopeRegion, newScopeRegion);
    }

    private void TransformHandlerRegion(
        ExceptionHandlerRegion<Statement<TInstruction>> parentRegion,
        HandlerRegion<TInstruction> handlerRegion
    )
    {
        var result = new HandlerRegion<Statement<TInstruction>>();
        parentRegion.Handlers.Add(result);

        if (handlerRegion.Prologue is not null)
            TransformRegion(x => result.Prologue = (ScopeRegion<Statement<TInstruction>>) x, handlerRegion.Prologue);

        if (handlerRegion.Epilogue is not null)
            TransformRegion(x => result.Epilogue = (ScopeRegion<Statement<TInstruction>>) x, handlerRegion.Epilogue);

        TransformScope(handlerRegion.Contents, result.Contents);

        result.Tag = handlerRegion.Tag;
    }

    private void TransformSubRegions(
        ScopeRegion<TInstruction> originalRegion,
        ScopeRegion<Statement<TInstruction>> newRegion
    )
    {
        foreach (var subRegion in originalRegion.Regions)
            TransformRegion(x => newRegion.Regions.Add(x), subRegion);
    }

}