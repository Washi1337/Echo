using System;
using System.Collections.Generic;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Echo.Core.Code;
using IVariable = Echo.Core.Code.IVariable;

namespace Echo.Platforms.Dnlib
{
    /// <summary>
    /// Provides a description of the CIL instruction set architecture (ISA) that is modelled by dnlib.   
    /// </summary>
    public class CilArchitecture : IInstructionSetArchitecture<Instruction>
    {
        private readonly IList<CilVariable> _variables;
        private readonly IList<CilParameter> _parameters;

        /// <summary>
        /// Creates a new CIL architecture description based on a CIL method body.
        /// </summary>
        /// <param name="method">The method definition.</param>
        public CilArchitecture(MethodDef method)
        {
            Method = method ?? throw new ArgumentNullException(nameof(method));

            if (!method.HasBody || method.Body is null)
                throw new ArgumentException("Method does not have a CIL method body.", nameof(method));

            _variables = MethodBody.Variables
                .Select(v => new CilVariable(v))
                .ToArray();

            _parameters = Method.Parameters
                .Select(p => new CilParameter(p))
                .ToArray();

            Method = method;
        }

        /// <summary>
        /// The CIL method definition.
        /// </summary>
        public MethodDef Method { get; }

        internal CilBody MethodBody => Method.Body;

        /// <summary>
        /// Gets the default static successor resolution engine for this architecture.
        /// </summary>
        public CilStaticSuccessorResolver SuccessorResolver
        {
            get;
        } = new CilStaticSuccessorResolver();

        /// <inheritdoc />
        public long GetOffset(in Instruction instruction) => instruction.Offset;

        /// <inheritdoc />
        public int GetSize(in Instruction instruction) => instruction.GetSize();

        /// <inheritdoc />
        public InstructionFlowControl GetFlowControl(in Instruction instruction)
        {
            // see https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.flowcontrol?view=netcore-3.1
            switch (instruction.OpCode.FlowControl)
            {
                case FlowControl.Break:
                case FlowControl.Call:
                case FlowControl.Meta: // used for prefixes and invalid instructions
                case FlowControl.Next:
                    return InstructionFlowControl.Fallthrough;
                case FlowControl.Branch:
                case FlowControl.Cond_Branch:
                case FlowControl.Throw:
                    return InstructionFlowControl.CanBranch;
                case FlowControl.Return:
                    return InstructionFlowControl.IsTerminator;
                case FlowControl.Phi:
                    throw new NotSupportedException("There are no known instructions with Phi control flow");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <inheritdoc />
        public int GetStackPushCount(in Instruction instruction)
        {
            instruction.CalculateStackUsage(out int pushes, out _);
            return pushes;
        }

        /// <inheritdoc />
        public int GetStackPopCount(in Instruction instruction)
        {
            bool isVoid = Method.ReturnType.ElementType == ElementType.Void;
            instruction.CalculateStackUsage(!isVoid, out _, out int pops);
            return pops;
        }

        /// <inheritdoc />
        public int GetReadVariablesCount(in Instruction instruction) => 
            instruction.IsLdloc() || instruction.IsLdarg() 
                ? 1
                : 0;
        
        /// <inheritdoc />
        public int GetReadVariables(in Instruction instruction, Span<IVariable> variablesBuffer)
        {
            if (instruction.IsLdloc())
            {
                variablesBuffer[0] = _variables[instruction.GetLocal(MethodBody.Variables).Index];
                return 1;
            }

            if (instruction.IsLdarg())
            {
                variablesBuffer[0] = _parameters[instruction.GetParameter(Method.Parameters).Index];
                return 1;
            }

            return 0;
        }
        
        /// <inheritdoc />
        public int GetWrittenVariablesCount(in Instruction instruction) => 
            instruction.IsStloc() || instruction.IsStarg() 
                ? 1
                : 0;

        /// <inheritdoc />
        public int GetWrittenVariables(in Instruction instruction, Span<IVariable> variablesBuffer)
        {
            if (instruction.IsStloc())
            {
                variablesBuffer[0] = _variables[instruction.GetLocal(MethodBody.Variables).Index];
                return 1;
            }

            if (instruction.IsStarg())
            {
                variablesBuffer[0] = _parameters[instruction.GetParameter(Method.Parameters).Index];
                return 1;
            }

            return 0;
        }
    }
}