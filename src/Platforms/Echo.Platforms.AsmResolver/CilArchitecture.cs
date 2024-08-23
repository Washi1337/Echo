﻿using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Cil;
using Echo.Code;

namespace Echo.Platforms.AsmResolver
{
    /// <summary>
    /// Provides a description of the CIL instruction set architecture (ISA) that is modelled by AsmResolver.   
    /// </summary>
    public class CilArchitecture : IArchitecture<CilInstruction>
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

            if (parentBody.Owner.Signature!.HasThis)
                _parameters.Insert(0, new CilParameter(parentBody.Owner.Parameters.ThisParameter!));
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
        } = new();

        /// <summary>
        /// Gets the Echo symbol for the provided <see cref="CilLocalVariable"/> instance.
        /// </summary>
        /// <param name="variable">The local variable.</param>
        /// <returns>The Echo symbol representing the local variable.</returns>
        public CilVariable GetLocal(CilLocalVariable variable) => _variables[variable.Index];

        /// <summary>
        /// Gets the Echo symbol for the provided <see cref="Parameter"/> instance.
        /// </summary>
        /// <param name="variable">The parameter.</param>
        /// <returns>The Echo symbol representing the parameter.</returns>
        public CilParameter GetParameter(Parameter variable) => _parameters[variable.MethodSignatureIndex];
        
        /// <inheritdoc />
        public long GetOffset(in CilInstruction instruction) => instruction.Offset;

        /// <inheritdoc />
        public int GetSize(in CilInstruction instruction) => instruction.Size;

        /// <inheritdoc />
        public InstructionFlowControl GetFlowControl(in CilInstruction instruction)
        {
            switch (instruction.OpCode.FlowControl)
            {
                case CilFlowControl.Branch:
                case CilFlowControl.ConditionalBranch:
                    return InstructionFlowControl.CanBranch | InstructionFlowControl.Fallthrough;

                case CilFlowControl.Return:
                case CilFlowControl.Throw:
                    return InstructionFlowControl.IsTerminator;

                case CilFlowControl.Break:
                case CilFlowControl.Call:
                case CilFlowControl.Meta:
                case CilFlowControl.Next:
                case CilFlowControl.Phi:
                    return InstructionFlowControl.Fallthrough;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <inheritdoc />
        public int GetStackPushCount(in CilInstruction instruction) => instruction.GetStackPushCount();

        /// <inheritdoc />
        public int GetStackPopCount(in CilInstruction instruction) => instruction.GetStackPopCount(MethodBody);

        /// <inheritdoc />
        public void GetReadVariables(in CilInstruction instruction, ICollection<IVariable> variablesBuffer)
        {
            if (instruction.IsLdloc())
                variablesBuffer.Add(GetLocal(instruction.GetLocalVariable(MethodBody.LocalVariables)));
            else if (instruction.IsLdarg())
                variablesBuffer.Add(GetParameter(instruction.GetParameter(MethodBody.Owner.Parameters)));
        }

        /// <inheritdoc />
        public void GetWrittenVariables(in CilInstruction instruction, ICollection<IVariable> variablesBuffer)
        {
            if (instruction.IsStloc())
                variablesBuffer.Add(GetLocal(instruction.GetLocalVariable(MethodBody.LocalVariables)));
            else if (instruction.IsStarg())
                variablesBuffer.Add(GetParameter(instruction.GetParameter(MethodBody.Owner.Parameters)));
        }
    }
}