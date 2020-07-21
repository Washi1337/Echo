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

            _variables = MethodBody.Variables.Select(v => new CilVariable(v)).ToArray();
            _parameters = Method.Parameters.Select(p => new CilParameter(p)).ToArray();

            Method = method;
        }

        /// <summary>
        /// The CIL method definition.
        /// </summary>
        public MethodDef Method { get; }

        private CilBody MethodBody => Method.Body;

        /// <summary>
        /// Gets the default static successor resolution engine for this architecture.
        /// </summary>
        public CilStaticSuccessorResolver SuccessorResolver
        {
            get;
        } = new CilStaticSuccessorResolver();

        /// <inheritdoc />
        public long GetOffset(Instruction instruction) => instruction.Offset;

        /// <inheritdoc />
        public int GetSize(Instruction instruction) => instruction.GetSize();

        /// <inheritdoc />
        public InstructionFlowControl GetFlowControl(Instruction instruction)
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
        public int GetStackPushCount(Instruction instruction)
        {
            instruction.CalculateStackUsage(out int pushes, out _);
            return pushes;
        }

        /// <inheritdoc />
        public int GetStackPopCount(Instruction instruction)
        {
            instruction.CalculateStackUsage(out _, out int pops);
            return pops;
        }

        /// <inheritdoc />
        public IEnumerable<IVariable> GetReadVariables(Instruction instruction)
        {
            if (instruction.IsLdloc())
            {
                return new[]
                {
                    _variables[instruction.GetLocal(MethodBody.Variables).Index]
                };
            }

            if (instruction.IsLdarg())
            {
                return new[]
                {
                    _parameters[instruction.GetParameter(Method.Parameters).Index]
                };
            }

            return Enumerable.Empty<IVariable>();
        }

        /// <inheritdoc />
        public IEnumerable<IVariable> GetWrittenVariables(Instruction instruction)
        {
            if (instruction.IsStloc())
            {
                return new[]
                {
                    _variables[instruction.GetLocal(MethodBody.Variables).Index]
                };
            }

            if (instruction.IsStarg())
            {
                return new[]
                {
                    _parameters[instruction.GetParameter(Method.Parameters).Index]
                };
            }

            return Enumerable.Empty<IVariable>();
        }
    }
}