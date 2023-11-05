using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Echo.ControlFlow;

namespace Echo.Ast.Construction;

[DebuggerDisplay("{Node} (Stack: {Stack})")]
internal readonly struct StackState<TInstruction>
{
    public StackState(ControlFlowNode<TInstruction> node)
    {
        Node = node;
        Stack = ImmutableStack<StackSlot>.Empty;
    }

    public StackState(ControlFlowNode<TInstruction> node, ImmutableStack<StackSlot> stack)
    {
        Node = node;
        Stack = stack;
    }

    public ControlFlowNode<TInstruction> Node { get; }

    public ImmutableStack<StackSlot> Stack { get; }

    public StackState<TInstruction> Pop(out StackSlot variable) => new(Node, Stack.Pop(out variable));

    public StackState<TInstruction> Push(StackSlot variable) => new(Node, Stack.Push(variable));

    public StackState<TInstruction> MoveTo(ControlFlowNode<TInstruction> node) => new(node, Stack);

    public bool MergeWith(StackState<TInstruction> other, out StackState<TInstruction> newState)
    {
        newState = this;

        bool changed = false;

        // Verify stack depths are the same, otherwise we cannot merge.
        int count = Stack.Count();
        if (other.Stack.Count() != count)
            throw new DataFlow.Emulation.StackImbalanceException(Node.Offset);

        var mergedSlots = new StackSlot[count];

        var current = this;
        for (int i = 0; i < count; i++)
        {
            // Get values of both stacks.
            current = current.Pop(out var value1);
            other = other.Pop(out var value2);

            // Unify them.
            var union = value1.Union(value2);
            if (union.Sources.Count != value1.Sources.Count)
                changed = true;

            // Store in new state.
            mergedSlots[count - 1 - i] = union;
        }

        if (changed)
            newState = new StackState<TInstruction>(Node, ImmutableStack.Create(mergedSlots));

        return changed;
    }
}