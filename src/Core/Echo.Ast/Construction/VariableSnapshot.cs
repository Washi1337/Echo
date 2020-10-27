using System;
using Echo.Core.Code;

namespace Echo.Ast.Construction
{
    internal readonly struct VariableSnapshot : IEquatable<VariableSnapshot>
    {
        internal VariableSnapshot(IVariable variable, int version)
        {
            Variable = variable ?? throw new ArgumentNullException(nameof(variable));
            Version = version;
        }

        internal IVariable Variable
        {
            get;
        }

        internal int Version
        {
            get;
        }

        public bool Equals(VariableSnapshot other) => Variable.Equals(other.Variable) && Version == other.Version;

        public override bool Equals(object obj) => obj is VariableSnapshot other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (Variable.GetHashCode() * 397) ^ Version;
            }
        }

        public override string ToString() => $"{Variable}_v{Version}";
    }
}