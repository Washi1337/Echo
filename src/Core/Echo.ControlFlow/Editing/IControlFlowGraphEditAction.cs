namespace Echo.ControlFlow.Editing
{
    public interface IControlFlowGraphEditAction<TInstruction>
    {
        void Apply(ControlFlowGraphEditContext<TInstruction> context);

        void Revert(ControlFlowGraphEditContext<TInstruction> context);
    }
}