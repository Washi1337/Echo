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
        private readonly IArchitecture<TInstruction> _baseArchitecture;
        private readonly FlowControlDeterminer<TInstruction> _flowControlDeterminer;

        /// <summary>
        /// Wraps the provided stack-based architecture to the lifted expression-based architecture.
        /// </summary>
        /// <param name="baseArchitecture">The <see cref="IArchitecture{TInstruction}"/> to decorate</param>
        public AstArchitecture(IArchitecture<TInstruction> baseArchitecture)
        {
            _baseArchitecture = baseArchitecture;
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
            var finder = new ReadVariableFinder<TInstruction>(_baseArchitecture);
            AstNodeWalker<TInstruction>.Walk(finder, instruction);
            return finder.Variables.Count;
        }

        /// <inheritdoc />
        public int GetReadVariables(in Statement<TInstruction> instruction, Span<IVariable> variablesBuffer)
        {
            var finder = new ReadVariableFinder<TInstruction>(_baseArchitecture);
            AstNodeWalker<TInstruction>.Walk(finder, instruction);
            
            int i = 0;
            foreach (var variable in finder.Variables)
                variablesBuffer[i++] = variable;

            return finder.Variables.Count;
        }

        /// <inheritdoc />
        public int GetWrittenVariablesCount(in Statement<TInstruction> instruction)
        {
            var finder = new WrittenVariableFinder<TInstruction>(_baseArchitecture);
            AstNodeWalker<TInstruction>.Walk(finder, instruction);
            return finder.Variables.Count;
        }

        /// <inheritdoc />
        public int GetWrittenVariables(in Statement<TInstruction> instruction, Span<IVariable> variablesBuffer)
        {
            var finder = new WrittenVariableFinder<TInstruction>(_baseArchitecture);
            AstNodeWalker<TInstruction>.Walk(finder, instruction);
            
            int i = 0;
            foreach (var variable in finder.Variables)
                variablesBuffer[i++] = variable;

            return finder.Variables.Count;
        }
    }

    /// <summary>
    /// Provides extension methods for AST architectures.
    /// </summary>
    public static class AstArchitectureExtensions
    {
        /// <summary>
        /// Wraps the provided architecture to an AST architecture.
        /// </summary>
        /// <param name="self">The architecture.</param>
        /// <typeparam name="TInstruction">The type of instructions defined by the architecture.</typeparam>
        /// <returns>The lifted architecture.</returns>
        public static AstArchitecture<TInstruction> ToAst<TInstruction>(this IArchitecture<TInstruction> self) 
            => new(self);
    }
}