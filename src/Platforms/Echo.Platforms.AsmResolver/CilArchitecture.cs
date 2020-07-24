using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using Echo.Core.Code;

namespace Echo.Platforms.AsmResolver
{
    /// <summary>
    /// Provides a description of the CIL instruction set architecture (ISA) that is modelled by AsmResolver.   
    /// </summary>
    public class CilArchitecture : IInstructionSetArchitecture<CilInstruction>
    {
        private readonly IList<CilVariable> _variables;
        private readonly IList<CilParameter> _parameters;

        /// <summary>
        /// Creates a new CIL architecture description based on a CIL method body.
        /// </summary>
        /// <param name="parentBody">The method body.</param>
        public CilArchitecture(CilMethodBody parentBody)
        {
            MethodBody = parentBody ?? throw new ArgumentNullException(nameof(parentBody));
            
            _variables = parentBody.LocalVariables
                .Select(v => new CilVariable(v))
                .ToArray();
            
            _parameters = parentBody.Owner.Parameters
                .Select(p => new CilParameter(p))
                .ToList();

            if (parentBody.Owner.Signature.HasThis)
                _parameters.Insert(0, new CilParameter(parentBody.Owner.Parameters.ThisParameter));
        }

        /// <summary>
        /// Gets the method body that was encapsulated.
        /// </summary>
        public CilMethodBody MethodBody
        {
            get;
        }

        /// <summary>
        /// Gets the default static successor resolution engine for this architecture.
        /// </summary>
        public CilStaticSuccessorResolver SuccessorResolver
        {
            get;
        } = new CilStaticSuccessorResolver();

        /// <inheritdoc />
        public long GetOffset(in CilInstruction instruction) => instruction.Offset;

        /// <inheritdoc />
        public int GetSize(in CilInstruction instruction) => instruction.Size;

        /// <inheritdoc />
        public InstructionFlowControl GetFlowControl(in CilInstruction instruction)
        {
            var result = InstructionFlowControl.Fallthrough;
            
            result |= instruction.OpCode.FlowControl switch
            {
                CilFlowControl.Branch => InstructionFlowControl.CanBranch,
                CilFlowControl.ConditionalBranch => InstructionFlowControl.CanBranch,
                CilFlowControl.Return => InstructionFlowControl.IsTerminator,
                CilFlowControl.Throw => InstructionFlowControl.IsTerminator,
                _ => InstructionFlowControl.Fallthrough
            };

            return result;
        }

        /// <inheritdoc />
        public int GetStackPushCount(in CilInstruction instruction)
        {
            return instruction.GetStackPushCount();
        }

        /// <inheritdoc />
        public int GetStackPopCount(in CilInstruction instruction)
        {
            return instruction.GetStackPopCount(MethodBody);
        }

        /// <inheritdoc />
        public IEnumerable<IVariable> GetReadVariables(in CilInstruction instruction)
        {
            if (instruction.IsLdloc())
            {
                return new[]
                {
                    _variables[instruction.GetLocalVariable(MethodBody.LocalVariables).Index]
                };
            }

            if (instruction.IsLdarg())
            {
                return new[]
                {
                    _parameters[instruction.GetParameter(MethodBody.Owner.Parameters).MethodSignatureIndex]
                };
            }

            return Enumerable.Empty<IVariable>();
        }

        /// <inheritdoc />
        public IEnumerable<IVariable> GetWrittenVariables(in CilInstruction instruction)
        {   
            if (instruction.IsStloc())
            {
                return new[]
                {
                    _variables[instruction.GetLocalVariable(MethodBody.LocalVariables).Index]
                };
            }

            if (instruction.IsStarg())
            {
                return new[]
                {
                    _parameters[instruction.GetParameter(MethodBody.Owner.Parameters).MethodSignatureIndex]
                };
            }

            return Enumerable.Empty<IVariable>();
        }
    }
}