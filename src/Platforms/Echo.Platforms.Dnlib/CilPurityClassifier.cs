using System;
using System.Collections.Generic;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.MD;
using Echo.Core;
using Echo.Core.Code;

namespace Echo.Platforms.Dnlib 
{
    /// <summary>
    /// Provides an implementation for the <see cref="IPurityClassifier{TInstruction}"/> interface that determines
    /// whether CIL instructions are considered pure or have side effects.
    /// </summary>
    public class CilPurityClassifier : IPurityClassifier<Instruction> 
    {
        /// <summary>
        /// Creates a new instance of the <see cref="CilPurityClassifier"/> class.
        /// </summary>
        public CilPurityClassifier() 
        {
            KnownPureMethods = new HashSet<IMethod>(MethodEqualityComparer.CompareDeclaringTypes);
            KnownImpureMethods = new HashSet<IMethod>(MethodEqualityComparer.CompareDeclaringTypes);
        }

        /// <summary>
        /// Gets or sets a value indicating whether writes to local variables should be considered pure or not.
        /// </summary>
        public Trilean LocalWritePurity 
        {
            get;
            set;
        } = false;

        /// <summary>
        /// Gets or sets a value indicating whether writes to arrays should be considered pure or not.
        /// </summary>
        public Trilean ArrayWritePurity 
        {
            get;
            set;
        } = false;

        /// <summary>
        /// Gets or sets a value indicating whether writes to pointers should be considered pure or not.
        /// </summary>
        public Trilean PointerWritePurity 
        {
            get;
            set;
        } = false;

        /// <summary>
        /// Gets or sets a value indicating whether field read accesses should be considered pure or not by default.  
        /// </summary>
        public Trilean DefaultFieldAccessPurity 
        {
            get;
            set;
        } = true;

        /// <summary>
        /// Gets or sets a value indicating whether writes to field should be considered pure or not by default.  
        /// </summary>
        public Trilean DefaultFieldWritePurity 
        {
            get;
            set;
        } = false;

        /// <summary>
        /// Gets or sets a value indicating whether method accesses (e.g. reading method pointers) should be
        /// considered pure or not by default.  
        /// </summary>
        public Trilean DefaultMethodAccessPurity 
        {
            get;
            set;
        } = true;

        /// <summary>
        /// Gets or sets a value indicating whether method calls should be considered pure or not by default.  
        /// </summary>
        public Trilean DefaultMethodCallPurity 
        {
            get;
            set;
        } = Trilean.Unknown;

        /// <summary>
        /// Gets or sets a value indicating whether indirect method calls should be considered pure or not by default.  
        /// </summary>
        public Trilean DefaultIndirectCallPurity 
        {
            get;
            set;
        } = Trilean.Unknown;

        /// <summary>
        /// Gets or sets a value indicating whether type access (e.g. pushing type tokens) should be considered pure
        /// or not by default.  
        /// </summary>
        public Trilean DefaultTypeAccessPurity 
        {
            get;
            set;
        } = true;

        /// <summary>
        /// Gets a mutable collection of known methods that should be considered pure.
        /// </summary>
        public ICollection<IMethod> KnownPureMethods 
        {
            get;
        }

        /// <summary>
        /// Gets a mutable collection of known methods that should be considered impure and guaranteed have side-effects.
        /// </summary>
        public ICollection<IMethod> KnownImpureMethods 
        {
            get;
        }

        /// <inheritdoc />
        public Trilean IsPure(in Instruction instruction) 
        {
            switch (instruction.OpCode.OperandType) 
            {
                case OperandType.InlineNone:
                    return ClassifyInlineNone(instruction);

                case OperandType.InlineI:
                case OperandType.ShortInlineI:
                case OperandType.InlineR:
                case OperandType.ShortInlineR:
                case OperandType.InlineString:
                case OperandType.InlineI8:
                case OperandType.InlineBrTarget:
                case OperandType.ShortInlineBrTarget:
                case OperandType.InlineSwitch:
                    return true;

                case OperandType.InlineVar:
                case OperandType.ShortInlineVar:
                    return ClassifyInlineVariableOrArgument(instruction);

                case OperandType.InlineField:
                    return ClassifyInlineField(instruction);

                case OperandType.InlineMethod:
                    return ClassifyInlineMethod(instruction);

                case OperandType.InlineType:
                    return ClassifyInlineType(instruction);

                case OperandType.InlineSig:
                    return DefaultIndirectCallPurity;

                case OperandType.InlineTok:
                    return ClassifyInlineToken(instruction);

                case OperandType.InlinePhi:
                    throw new NotSupportedException();

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private Trilean ClassifyInlineNone(in Instruction instruction) 
        {
            switch (instruction.OpCode.Code) 
            {
                case Code.Stloc_0:
                case Code.Stloc_1:
                case Code.Stloc_2:
                case Code.Stloc_3:
                    return LocalWritePurity;

                case Code.Stelem_I:
                case Code.Stelem_I1:
                case Code.Stelem_I2:
                case Code.Stelem_I4:
                case Code.Stelem_I8:
                case Code.Stelem_R4:
                case Code.Stelem_R8:
                case Code.Stelem_Ref:
                    return ArrayWritePurity;

                case Code.Stind_I:
                case Code.Stind_I1:
                case Code.Stind_I2:
                case Code.Stind_I4:
                case Code.Stind_I8:
                case Code.Stind_R8:
                    return PointerWritePurity;

                default:
                    return true;
            }
        }

        private Trilean ClassifyInlineVariableOrArgument(in Instruction instruction) 
        {
            switch (instruction.OpCode.Code)
            {
                case Code.Stloc:
                case Code.Stloc_S:
                case Code.Starg:
                case Code.Starg_S:
                    return LocalWritePurity;

                default:
                    return true;
            }
        }

        private Trilean ClassifyInlineField(in Instruction instruction) 
        {
            switch (instruction.OpCode.Code) 
            {
                case Code.Ldfld:
                case Code.Ldsfld:
                case Code.Ldflda:
                case Code.Ldsflda:
                    return DefaultFieldAccessPurity;

                case Code.Stfld:
                case Code.Stsfld:
                    return DefaultFieldWritePurity;

                default:
                    return Trilean.Unknown;
            }
        }

        private Trilean ClassifyInlineMethod(in Instruction instruction) 
        {
            switch (instruction.OpCode.Code) 
            {
                case Code.Call:
                case Code.Callvirt:
                case Code.Newobj:
                case Code.Jmp:
                    if (instruction.Operand is IMethod method) {
                        if (KnownPureMethods.Contains(method))
                            return true;
                        if (KnownImpureMethods.Contains(method))
                            return false;
                    }

                    return DefaultMethodCallPurity;

                case Code.Ldftn:
                case Code.Ldvirtftn:
                    return DefaultMethodAccessPurity;

                default:
                    return Trilean.Unknown;
            }
        }

        private Trilean ClassifyInlineType(in Instruction instruction) 
        {
            switch (instruction.OpCode.Code) 
            {
                case Code.Stelem:
                    return ArrayWritePurity | DefaultTypeAccessPurity;

                default:
                    return DefaultTypeAccessPurity;
            }
        }

        private Trilean ClassifyInlineToken(in Instruction instruction) 
        {
            var token = ((IMDTokenProvider)instruction.Operand).MDToken;
            switch (token.Table) 
            {
                case Table.TypeDef:
                case Table.TypeRef:
                case Table.TypeSpec:
                    return DefaultTypeAccessPurity;

                case Table.Method:
                case Table.MethodSpec:
                    return DefaultMethodAccessPurity;

                case Table.Field:
                    return DefaultFieldAccessPurity;

                case Table.MemberRef:
                    var reference = (MemberRef)instruction.Operand;
                    if (reference.IsFieldRef)
                        return DefaultFieldAccessPurity;
                    else if (reference.IsMethodRef)
                        return DefaultMethodAccessPurity;
                    else
                        return Trilean.Unknown;

                default:
                    return Trilean.Unknown;
            }
        }
    }
}