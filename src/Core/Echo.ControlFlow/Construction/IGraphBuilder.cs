using Echo.Core.Code;

namespace Echo.ControlFlow.Construction
{
    public interface IGraphBuilder<TInstruction>
        where TInstruction : IInstruction
    {
        Graph<TInstruction> ConstructFlowGraph(long entrypoint);
    }
}