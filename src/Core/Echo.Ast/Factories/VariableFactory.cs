using System;
using Echo.Core.Code;

namespace Echo.Ast.Factories
{
    internal static class VariableFactory
    {
        internal static AstVariable CreateVariable(string name) => new AstVariable(name);
        
        internal static AstVariable CreateVariable(string name, int version) => new AstVariable($"{name}_v{version}");

        internal static IVariable[] CreateVariableBuffer(int count) =>
            count == 0 ? Array.Empty<IVariable>() : new IVariable[count];
    }
}