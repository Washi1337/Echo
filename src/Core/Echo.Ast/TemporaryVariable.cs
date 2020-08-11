using Echo.Core.Code;

namespace Echo.Ast
{
    internal sealed class TemporaryVariable : IVariable
    {
        internal TemporaryVariable(IVariable original)
        {
            Original = original;
        }

        public string Name => $"{Original.Name}_tmp";

        public IVariable Original
        {
            get;
        }
    }
}