using System.Collections.Generic;
using Echo.ControlFlow;
using Echo.Code;
using Echo.DataFlow;

namespace Echo.Ast.Construction
{
    internal sealed class AstParserContext<TInstruction>
    {
        private readonly ControlFlowGraph<TInstruction> _controlFlowGraph;

        internal AstParserContext(
            ControlFlowGraph<TInstruction> controlFlowGraph,
            DataFlowGraph<TInstruction> dataFlowGraph)
        {
            _controlFlowGraph = controlFlowGraph;
            DataFlowGraph = dataFlowGraph;
        }

        internal IArchitecture<TInstruction> Architecture => _controlFlowGraph.Architecture;

        internal Dictionary<TInstruction, AstVariable[]> StackSlots
        {
            get;
        } = new Dictionary<TInstruction, AstVariable[]>();

        internal Dictionary<ExternalDataSourceNode<TInstruction>, AstVariable> ExternalSources
        {
            get;
        } = new Dictionary<ExternalDataSourceNode<TInstruction>, AstVariable>();

        internal Dictionary<IVariable, int> VariableVersions
        {
            get;
        } = new Dictionary<IVariable, int>();

        internal Dictionary<VariableSnapshot, AstVariable> VersionedVariables
        {
            get;
        } = new Dictionary<VariableSnapshot, AstVariable>();

        internal Dictionary<List<AstVariable>, AstVariable> VariableSourcesToPhiVariable
        {
            get;
        } = new Dictionary<List<AstVariable>, AstVariable>(new AstVariableListComparer());

        internal Dictionary<TInstruction, Dictionary<IVariable, int>> VariableStates
        {
            get;
        } = new Dictionary<TInstruction, Dictionary<IVariable, int>>();

        internal DataFlowGraph<TInstruction> DataFlowGraph
        {
            get;
        }

        internal int PhiCount
        {
            get;
            set;
        }

        private int StackSlotCount
        {
            get;
            set;
        }

        internal AstVariable CreateStackSlot()
        {
            return new AstVariable($"stack_slot_{StackSlotCount++}");
        }

        internal int GetVariableVersion(IVariable variable)
        {
            if (!VariableVersions.ContainsKey(variable))
                VariableVersions.Add(variable, 0);

            return VariableVersions[variable];
        }

        internal int IncrementVariableVersion(IVariable variable)
        {
            // Ensure variable is created first.
            GetVariableVersion(variable);

            // Increment the version and return the incremented result
            return ++VariableVersions[variable];
        }

        internal AstVariable GetVersionedVariable(VariableSnapshot snapshot)
        {
            if (VersionedVariables.TryGetValue(snapshot, out var variable))
                return variable;
            
            variable = new AstVariable(snapshot.ToString());
            VersionedVariables.Add(snapshot, variable);

            return variable;
        }

        internal AstVariable GetExternalVariable(ExternalDataSourceNode<TInstruction> external)
        {
            if (ExternalSources.TryGetValue(external, out var variable))
                return variable;
            
            variable = new AstVariable(external.Name);
            ExternalSources.Add(external, variable);
            
            return variable;
        }
    }
}