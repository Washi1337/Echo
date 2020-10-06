using System.Collections.Generic;
using System.Linq;
using Echo.Ast.Helpers;
using Echo.Core.Code;
using Echo.DataFlow;

namespace Echo.Ast.Factories
{
    internal static class AstVariableCollectionFactory<TInstruction>
    {
        internal static AstVariableCollection CollectDependencies(
            AstParserContext<TInstruction> context,
            IVariable variable,
            DataDependency<TInstruction> dependency)
        {
            var result = new AstVariableCollection();

            foreach (long offset in dependency.Select(d => context.Architecture.GetOffset(d.Node.Contents)))
            {
                if (context.InstructionVersionStates.TryGetValue(offset, out var versions))
                {
                    result.Add(context.VersionedAstVariables[(variable, versions[variable])]);
                }
                else
                {
                    int version = context.IncrementVersion(variable);
                    var slot = VariableFactory.CreateVariable(variable.Name, version);
                    context.InstructionVersionStates.Add(offset, new Dictionary<IVariable, int>
                    {
                        [variable] = version
                    });

                    context.VersionedAstVariables[(variable, version)] = slot;
                    result.Add(slot);
                }
            }

            return result;
        }
    }
}