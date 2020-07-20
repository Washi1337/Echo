using System.Collections.Generic;
using System.Linq;
using Echo.Core.Code;
using JavaResolver.Class.Code;
using JavaResolver.Class.TypeSystem;

namespace Echo.Platforms.JavaResolver
{
    /// <summary>
    ///     Provides a description of the ByteCode instruction set architecture (ISA) that is modelled by JavaResolver.   
    /// </summary>
    public class ByteCodeArchitecture : IInstructionSetArchitecture<ByteCodeInstruction>
    {
        private readonly IList<ByteCodeVariable> _variables;
        
        /// <summary>
        ///     Creates a new bytecode architecture.
        /// </summary>
        /// <param name="methodDefinition">The method.</param>
        public ByteCodeArchitecture(MethodDefinition methodDefinition)
        {
            _variables = methodDefinition.Body.Variables
                .Select(v => new ByteCodeVariable(v))
                .ToArray();
        }
        
        /// <inheritdoc />
        public long GetOffset(ByteCodeInstruction instruction) => instruction.Offset;

        /// <inheritdoc />
        public int GetSize(ByteCodeInstruction instruction) => instruction.Size;

        /// <inheritdoc />
        public InstructionFlowControl GetFlowControl(ByteCodeInstruction instruction) =>
            instruction.OpCode.FlowControl switch
            {
                ByteCodeFlowControl.Branch => InstructionFlowControl.Fallthrough | InstructionFlowControl.CanBranch,
                ByteCodeFlowControl.ConditionalBranch => InstructionFlowControl.Fallthrough | InstructionFlowControl.CanBranch,
                ByteCodeFlowControl.Return => InstructionFlowControl.Fallthrough | InstructionFlowControl.IsTerminator,
                ByteCodeFlowControl.Throw => InstructionFlowControl.Fallthrough | InstructionFlowControl.IsTerminator,
                _ => InstructionFlowControl.Fallthrough
            };

        /// <inheritdoc />
        public int GetStackPushCount(ByteCodeInstruction instruction) => instruction.GetStackPushCount();

        /// <inheritdoc />
        public int GetStackPopCount(ByteCodeInstruction instruction) => instruction.GetStackPopCount();

        /// <inheritdoc />
        public IEnumerable<IVariable> GetReadVariables(ByteCodeInstruction instruction)
        {
            if (instruction.OpCode.IsALoad())
                return new[] { _variables[((LocalVariable)instruction.Operand).Index] };
            
            return Enumerable.Empty<IVariable>();
        }

        /// <inheritdoc />
        public IEnumerable<IVariable> GetWrittenVariables(ByteCodeInstruction instruction)
        {
            if (instruction.OpCode.IsIStore())
                return new[] { _variables[((LocalVariable)instruction.Operand).Index] };
            
            return Enumerable.Empty<IVariable>();
        }
    }
}