using System;
using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Echo.Code;

namespace Echo.Platforms.AsmResolver
{
    /// <summary>
    /// Provides an implementation for the <see cref="IPurityClassifier{TInstruction}"/> interface that determines
    /// whether CIL instructions are considered pure or have side effects.
    /// </summary>
    public class CilPurityClassifier : IPurityClassifier<CilInstruction>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="CilPurityClassifier"/> class.
        /// </summary>
        public CilPurityClassifier()
        {
            KnownPureMethods = new HashSet<IMethodDescriptor>(SignatureComparer.Default);
            KnownImpureMethods = new HashSet<IMethodDescriptor>(SignatureComparer.Default);
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
        public ICollection<IMethodDescriptor> KnownPureMethods
        {
            get;
        }

        /// <summary>
        /// Gets a mutable collection of known methods that should be considered impure and guaranteed have side-effects.
        /// </summary>
        public ICollection<IMethodDescriptor> KnownImpureMethods
        {
            get;
        }
            
        /// <inheritdoc />
        public Trilean IsPure(in CilInstruction instruction)
        {
            switch (instruction.OpCode.OperandType)
            {
                case CilOperandType.InlineNone:
                    return ClassifyInlineNone(instruction);
                
                case CilOperandType.InlineI:
                case CilOperandType.ShortInlineI:
                case CilOperandType.InlineR:
                case CilOperandType.ShortInlineR:
                case CilOperandType.InlineString:
                case CilOperandType.InlineI8:
                case CilOperandType.InlineBrTarget:
                case CilOperandType.ShortInlineBrTarget:
                case CilOperandType.InlineSwitch:
                    return true;

                case CilOperandType.InlineVar:
                case CilOperandType.ShortInlineVar:
                case CilOperandType.InlineArgument:
                case CilOperandType.ShortInlineArgument:
                    return ClassifyInlineVariableOrArgument(instruction);
                
                case CilOperandType.InlineField:
                    return ClassifyInlineField(instruction);

                case CilOperandType.InlineMethod:
                    return ClassifyInlineMethod(instruction);

                case CilOperandType.InlineType:
                    return ClassifyInlineType(instruction);

                case CilOperandType.InlineSig:
                    return DefaultIndirectCallPurity;

                case CilOperandType.InlineTok:
                    return ClassifyInlineToken(instruction);

                case CilOperandType.InlinePhi:
                    throw new NotSupportedException();

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private Trilean ClassifyInlineNone(in CilInstruction instruction)
        {
            switch (instruction.OpCode.Code)
            {
                case CilCode.Stloc_0:
                case CilCode.Stloc_1:
                case CilCode.Stloc_2:
                case CilCode.Stloc_3:
                    return LocalWritePurity;
                
                case CilCode.Stelem_I:
                case CilCode.Stelem_I1:
                case CilCode.Stelem_I2:
                case CilCode.Stelem_I4:
                case CilCode.Stelem_I8:
                case CilCode.Stelem_R4:
                case CilCode.Stelem_R8:
                case CilCode.Stelem_Ref:
                    return ArrayWritePurity;
                
                case CilCode.Stind_I:
                case CilCode.Stind_I1:
                case CilCode.Stind_I2:
                case CilCode.Stind_I4:
                case CilCode.Stind_I8:
                case CilCode.Stind_R8:
                    return PointerWritePurity;
                
                default:
                    return true;
            }
        }

        private Trilean ClassifyInlineVariableOrArgument(in CilInstruction instruction)
        {
            switch (instruction.OpCode.Code)
            {
                case CilCode.Stloc:
                case CilCode.Stloc_S:
                case CilCode.Starg:
                case CilCode.Starg_S:
                    return LocalWritePurity;
                
                default:
                    return true;
            }
        }

        private Trilean ClassifyInlineField(in CilInstruction instruction)
        {
            switch (instruction.OpCode.Code)
            {
                case CilCode.Ldfld:
                case CilCode.Ldsfld:
                case CilCode.Ldflda:
                case CilCode.Ldsflda:
                    return DefaultFieldAccessPurity;

                case CilCode.Stfld:
                case CilCode.Stsfld:
                    return DefaultFieldWritePurity;

                default:
                    return Trilean.Unknown;
            }
        }

        private Trilean ClassifyInlineMethod(in CilInstruction instruction)
        {
            switch (instruction.OpCode.Code)
            {
                case CilCode.Call:
                case CilCode.Callvirt:
                case CilCode.Newobj:
                case CilCode.Jmp:
                    if (instruction.Operand is IMethodDescriptor method)
                    {
                        if (KnownPureMethods.Contains(method))
                            return true;
                        if (KnownImpureMethods.Contains(method))
                            return false;
                    }

                    return DefaultMethodCallPurity;

                case CilCode.Ldftn:
                case CilCode.Ldvirtftn:
                    return DefaultMethodAccessPurity;

                default:
                    return Trilean.Unknown;
            }
        }

        private Trilean ClassifyInlineType(in CilInstruction instruction)
        {
            switch (instruction.OpCode.Code)
            {
                case CilCode.Stelem:
                    return ArrayWritePurity | DefaultTypeAccessPurity;
                
                 default:
                     return DefaultTypeAccessPurity;
            }
        }

        private Trilean ClassifyInlineToken(in CilInstruction instruction)
        {
            var token = ((IMetadataMember) instruction.Operand!).MetadataToken; 
            switch (token.Table)
            {
                case TableIndex.TypeDef:
                case TableIndex.TypeRef:
                case TableIndex.TypeSpec:
                    return DefaultTypeAccessPurity;

                case TableIndex.Method:
                case TableIndex.MethodSpec:
                    return DefaultMethodAccessPurity;

                case TableIndex.Field:
                    return DefaultFieldAccessPurity;

                case TableIndex.MemberRef:
                    var reference = (MemberReference) instruction.Operand;
                    if (reference.IsField)
                        return DefaultFieldAccessPurity;
                    else if (reference.IsMethod)
                        return DefaultMethodAccessPurity;
                    else 
                        return Trilean.Unknown;

                default:
                    return Trilean.Unknown;
            }
        }
    }
}