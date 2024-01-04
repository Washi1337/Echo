using System;
using Echo.Ast.Analysis;
using Echo.Code;

namespace Echo.Ast
{
    /// <summary>
    /// Describes an architecture that is lifted from a stack-based platform to an expression-based platform.
    /// </summary>
    /// <typeparam name="TInstruction">The instructions defined by the satck-based platform.</typeparam>
    public class AstArchitecture<TInstruction>
        : IArchitecture<Statement<TInstruction>>
    {
        private readonly FlowControlDeterminer<TInstruction> _flowControlDeterminer;

        /// <summary>
        /// Wraps the provided stack-based architecture to the lifted expression-based architecture.
        /// </summary>
        /// <param name="baseArchitecture">The <see cref="IArchitecture{TInstruction}"/> to decorate</param>
        public AstArchitecture(IArchitecture<TInstruction> baseArchitecture)
        {
            _flowControlDeterminer = new FlowControlDeterminer<TInstruction>(baseArchitecture);
        }

        /// <inheritdoc />
        public long GetOffset(in Statement<TInstruction> instruction) => throw new NotSupportedException();

        /// <inheritdoc />
        public int GetSize(in Statement<TInstruction> instruction) => 1;

        /// <inheritdoc />
        public InstructionFlowControl GetFlowControl(in Statement<TInstruction> instruction)
        {
            return instruction.Accept(_flowControlDeterminer, null);
        }

        /// <inheritdoc />
        public int GetStackPushCount(in Statement<TInstruction> instruction) => 0;

        /// <inheritdoc />
        public int GetStackPopCount(in Statement<TInstruction> instruction) => 0;

        /// <inheritdoc />
        public int GetReadVariablesCount(in Statement<TInstruction> instruction)
        {
            var finder = new ReadVariableFinder<TInstruction>();
            AstNodeWalker<TInstruction>.Walk(finder, instruction);
            return finder.Variables.Count;
        }

        /// <inheritdoc />
        public int GetReadVariables(in Statement<TInstruction> instruction, Span<IVariable> variablesBuffer)
        {
            var finder = new ReadVariableFinder<TInstruction>();
            AstNodeWalker<TInstruction>.Walk(finder, instruction);
            
            int i = 0;
            foreach (var variable in finder.Variables)
                variablesBuffer[i++] = variable;

            return finder.Variables.Count;
        }

        /// <inheritdoc />
        public int GetWrittenVariablesCount(in Statement<TInstruction> instruction)
        {
            var finder = new WrittenVariableFinder<TInstruction>();
            AstNodeWalker<TInstruction>.Walk(finder, instruction);
            return finder.Variables.Count;
        }

        /// <inheritdoc />
        public int GetWrittenVariables(in Statement<TInstruction> instruction, Span<IVariable> variablesBuffer)
        {
            var finder = new WrittenVariableFinder<TInstruction>();
            AstNodeWalker<TInstruction>.Walk(finder, instruction);
            
            int i = 0;
            foreach (var variable in finder.Variables)
                variablesBuffer[i++] = variable;

            return finder.Variables.Count;
        }
    }
}