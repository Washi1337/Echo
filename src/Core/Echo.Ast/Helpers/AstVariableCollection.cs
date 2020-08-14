using System.Collections.Generic;

namespace Echo.Ast.Helpers
{
    internal sealed class AstVariableCollection : List<AstVariable>
    {
        public override bool Equals(object obj)
        {
            if (!(obj is AstVariableCollection))
                return false;

            return GetHashCode() == obj.GetHashCode();
        }

        public override int GetHashCode()
        {
            int num = 0;

            foreach (var item in this)
            {
                unchecked
                {
                    int itemHash = item.Name.GetHashCode();
                    num += (itemHash / 6) >> 2;
                    num *= 5;
                }
            }

            return num;
        }
    }
}