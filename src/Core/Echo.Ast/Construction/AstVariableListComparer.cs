using System;
using System.Collections.Generic;
using System.Linq;

namespace Echo.Ast.Construction
{
    internal sealed class AstVariableListComparer : EqualityComparer<List<AstVariable>>
    {
        public override bool Equals(List<AstVariable> x, List<AstVariable> y)
        {
            if (x == null)
                throw new ArgumentNullException(nameof(x));
            if (y == null)
                throw new ArgumentNullException(nameof(y));
            
            return x.SequenceEqual(y);
        }

        public override int GetHashCode(List<AstVariable> obj)
        {
            int hash = 1337;
            unchecked
            {
                foreach (var item in obj)
                    hash += item.GetHashCode() ^ 397;
            }

            return hash;
        }
    }
}