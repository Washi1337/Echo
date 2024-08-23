using System.Collections.ObjectModel;
using Echo.Code;

namespace Echo.Ast;

internal sealed class VariableCollection<TInstruction> : Collection<IVariable>
    where TInstruction : notnull
{
    private readonly Statement<TInstruction> _owner;

    public VariableCollection(Statement<TInstruction> owner)
    {
        _owner = owner;
    }

    protected override void InsertItem(int index, IVariable item)
    {
        base.InsertItem(index, item);
        _owner.GetParentCompilationUnit()?.RegisterVariableWrite(item, _owner);
    }

    protected override void SetItem(int index, IVariable item)
    {
        var root = _owner.GetParentCompilationUnit();
        root?.UnregisterVariableWrite(Items[index], _owner);
        base.SetItem(index, item);
        root?.RegisterVariableWrite(item, _owner);
    }

    protected override void RemoveItem(int index)
    {
        var variable = Items[index];
        base.RemoveItem(index);
        _owner.GetParentCompilationUnit()?.UnregisterVariableWrite(variable, _owner);
    }

    protected override void ClearItems()
    {
        if (_owner.GetParentCompilationUnit() is {} root)
        {
            foreach (var item in Items)
                root.UnregisterVariableWrite(item, _owner);
        }
        
        base.ClearItems();
    }
}