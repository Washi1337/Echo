using System;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Echo.Concrete.Values;
using Echo.Concrete.Values.ReferenceType;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Values
{
    /// <summary>
    /// Provides a default implementation of the <see cref="ICliMarshaller"/> interface, which marshals concrete values
    /// put into variables and fields within a .NET program to values on the evaluation stack of the CLI and vice versa. 
    /// </summary>
    public class DefaultCliMarshaller : ICliMarshaller
    {
        /// <summary>
        /// Creates a new instance of the <see cref="DefaultCliMarshaller"/> class.
        /// </summary>
        /// <param name="environment">The environment this marshaller is for.</param>
        public DefaultCliMarshaller(ICilRuntimeEnvironment environment)
        {
            Environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }
        
        /// <summary>
        /// Gets the environment that the marshaller converts values for.
        /// </summary>
        public ICilRuntimeEnvironment Environment
        {
            get;
        }

        /// <inheritdoc />
        public bool Is32Bit => Environment.Is32Bit;

        /// <inheritdoc />
        public ICliValue ToCliValue(IConcreteValue value, TypeSignature originalType)
        {
            return originalType.ElementType switch
            {
                ElementType.I1 => Int8ToI4(value as IntegerValue, true),
                ElementType.U1 => Int8ToI4(value as IntegerValue, false),
                ElementType.I2 => Int16ToI4(value as IntegerValue, true),
                ElementType.Char => Int16ToI4(value as IntegerValue, false),
                ElementType.U2 => Int16ToI4(value as IntegerValue, false),
                ElementType.Boolean => BoolToI4(value as IntegerValue),
                ElementType.I4 => Int32ToI4(value as IntegerValue),
                ElementType.U4 => Int32ToI4(value as IntegerValue),
                ElementType.I8 => Int64ToI8(value as IntegerValue),
                ElementType.U8 => Int64ToI8(value as IntegerValue),
                ElementType.R4 => Float32ToR4((Float32Value) value),
                ElementType.R8 => Float64ToR8((Float64Value) value),
                ElementType.I => IntToI(value as IntegerValue),
                ElementType.U => IntToI(value as IntegerValue),
                ElementType.Ptr => PtrToPointerValue(value as IPointerValue),
                ElementType.ValueType => ObjectToStruct(value, originalType),
                _ => ObjectToO(value)
            };
        }

        /// <summary>
        /// Converts the provided (partially) known 8 bit integer value to an I4 value. 
        /// </summary>
        /// <param name="value">The value to marshal.</param>
        /// <param name="signed">Indicates whether the value is originally a signed or an unsigned integer.</param>
        /// <returns>The marshalled value.</returns>
        /// <remarks>
        /// This method implements the conversion rules for 8 bit integers to 32 bit integers as described in the
        /// ECMA-335 III.1.1.1, and therefore sign extends the value when the integer is signed. This also holds when
        /// the sign bit is marked unknown. In such a case, all the remaining 24 bits will be marked unknown. 
        /// </remarks>
        protected virtual I4Value Int8ToI4(IntegerValue value, bool signed)
        {
            if (value is Integer8Value int8)
            {
                uint signMask = !signed || int8.GetBit(7).IsKnown ? 0xFFFFFF00 : 0;
                return new I4Value(signed ? int8.I8 : (int) int8.U8, int8.Mask | signMask);
            }

            return new I4Value(0, 0);
        }
        
        /// <summary>
        /// Converts the provided (partially) known 16 bit integer value to an I4 value. 
        /// </summary>
        /// <param name="value">The value to marshal.</param>
        /// <param name="signed">Indicates whether the value is originally a signed or an unsigned integer.</param>
        /// <returns>The marshalled value.</returns>
        /// <remarks>
        /// This method implements the conversion rules for 16 bit integers to 32 bit integers as described in the
        /// ECMA-335 III.1.1.1, and therefore sign extends the value when the integer is signed. This also holds when
        /// the sign bit is marked unknown. In such a case, all the remaining 16 bits will be marked unknown. 
        /// </remarks>
        protected virtual  I4Value Int16ToI4(IntegerValue value, bool signed)
        {
            if (value is Integer16Value int16)
            {
                uint signMask = !signed || int16.GetBit(15).IsKnown ? 0xFFFF0000 : 0;
                return new I4Value(signed ? int16.I16 : (int) int16.U16, int16.Mask | signMask);
            }

            return new I4Value(0, 0);
        }

        /// <summary>
        /// Converts the provided (partially) known boolean value to an I4 value. 
        /// </summary>
        /// <param name="value">The value to marshal.</param>
        /// <returns>The marshalled value.</returns>
        protected virtual I4Value BoolToI4(IntegerValue value)
        {
            if (value is Integer32Value int32)
                return new I4Value(int32.I32, int32.Mask);
            return new I4Value(0, 0xFFFFFFFE);
        }

        /// <summary>
        /// Converts the provided (partially) known 32 bit integer value to an I4 value. 
        /// </summary>
        /// <param name="value">The value to marshal.</param>
        /// <returns>The marshalled value.</returns>
        protected virtual  I4Value Int32ToI4(IntegerValue value)
        {
            if (value is Integer32Value int32)
                return new I4Value(int32.I32, int32.Mask);
            return new I4Value(0, 0);
        }

        /// <summary>
        /// Converts the provided (partially) known 64 bit integer value to an I8 value. 
        /// </summary>
        /// <param name="value">The value to marshal.</param>
        /// <returns>The marshalled value.</returns>
        protected virtual  I8Value Int64ToI8(IntegerValue value)
        {
            if (value is Integer64Value int64)
                return new I8Value(int64.I64, int64.Mask);
            return new I8Value(0, 0);
        }

        /// <summary>
        /// Converts the provided (partially) known integer value to a native sized integer value. 
        /// </summary>
        /// <param name="value">The value to marshal.</param>
        /// <returns>The marshalled value.</returns>
        protected virtual  NativeIntegerValue IntToI(IntegerValue value)
        {
            return new NativeIntegerValue(value, Is32Bit);
        }

        /// <summary>
        /// Converts the provided (partially) known 32 bit floating point number to an F value. 
        /// </summary>
        /// <param name="value">The value to marshal.</param>
        /// <returns>The marshalled value.</returns>
        protected virtual FValue Float32ToR4(Float32Value value)
        {
            return new FValue(value.F32);
        }

        /// <summary>
        /// Converts the provided (partially) known 64 bit floating point number to an F value. 
        /// </summary>
        /// <param name="value">The value to marshal.</param>
        /// <returns>The marshalled value.</returns>
        protected virtual FValue Float64ToR8(Float64Value value)
        {
            return new FValue(value.F64);
        }

        /// <summary>
        /// Converts the provided value-typed object into a struct value. 
        /// </summary>
        /// <param name="value">The value to marshal.</param>
        /// <returns>The marshalled value.</returns>
        protected virtual ICliValue ObjectToStruct(IConcreteValue value, TypeSignature type)
        {
            var enumType = type.Resolve();
            if (enumType.IsEnum)
                return ToCliValue(value, enumType.GetEnumUnderlyingType());
            
            if (value is LleObjectValue lleObjectValue)
                return new StructValue(Environment.MemoryAllocator, lleObjectValue.Type, lleObjectValue.Contents);

            throw new NotSupportedException($"Invalid or unsupported value {value}.");
        }

        /// <summary>
        /// Converts the provided object value to a type O object reference.
        /// </summary>
        /// <param name="value">The value to marshal.</param>
        /// <returns>The marshalled value.</returns>
        protected virtual OValue ObjectToO(IConcreteValue value)
        {
            var referencedObject = value is ObjectReference objectReference
                ? objectReference.ReferencedObject 
                : value;

            return new OValue(referencedObject, value.IsKnown, Is32Bit);
        }

        /// <summary>
        /// Converts the provided object value to a type O object reference.
        /// </summary>
        /// <param name="value">The value to marshal.</param>
        /// <returns>The marshalled value.</returns>
        protected virtual PointerValue PtrToPointerValue(IConcreteValue value)
        {
            switch (value)
            {
                case RelativePointerValue relativePointerValue:
                    return new PointerValue(relativePointerValue.BasePointer, relativePointerValue.CurrentOffset);
                
                case IPointerValue pointerValue:
                    return new PointerValue(pointerValue);

                default:
                    return new PointerValue(false);
            }
        }
        
        /// <inheritdoc />
        public IConcreteValue ToCtsValue(ICliValue value, TypeSignature targetType)
        {
            switch (targetType.ElementType)
            {
                case ElementType.I1:
                case ElementType.U1:
                {
                    var i4Value = value.InterpretAsI4();
                    return new Integer8Value((byte) (i4Value.I32 & 0xFF), (byte) (i4Value.Mask & 0xFF));
                }
                
                case ElementType.I2:
                case ElementType.Char:
                case ElementType.U2:
                {
                    var i4Value = value.InterpretAsI4();
                    return new Integer16Value((ushort) (i4Value.I32 & 0xFFFF), (ushort) (i4Value.Mask & 0xFFFF));
                }
                
                case ElementType.Boolean:
                case ElementType.I4:
                case ElementType.U4:
                {
                    var i4Value = value.InterpretAsI4();
                    return new Integer32Value(i4Value.I32, i4Value.Mask);
                }
                
                case ElementType.I8:
                case ElementType.U8:
                {
                    var i8Value = value.InterpretAsI8();
                    return new Integer64Value(i8Value.I64, i8Value.Mask);
                }

                case ElementType.R4:
                {
                    return new Float32Value((float) value.InterpretAsR4().F64);
                }

                case ElementType.R8:
                {
                    return new Float64Value(value.InterpretAsR8().F64);
                }

                case ElementType.I:
                {
                    return value.InterpretAsI(Is32Bit);
                }

                case ElementType.U:
                {
                    return value.InterpretAsU(Is32Bit);
                }

                case ElementType.Object:
                case ElementType.Array:
                case ElementType.SzArray:
                case ElementType.String:
                case ElementType.Class:
                {
                    var oValue = value.InterpretAsRef(Is32Bit);
                    return new ObjectReference(oValue.ReferencedObject, oValue.IsKnown, Is32Bit);
                }

                case ElementType.Ptr:
                {
                    var ptrValue = (PointerValue) value;
                    return new RelativePointerValue(ptrValue.BasePointer, ptrValue.CurrentOffset);
                }

                case ElementType.ValueType:
                {
                    var enumType = targetType.Resolve();
                    if (enumType.IsEnum)
                        return ToCtsValue(value, enumType.GetEnumUnderlyingType());

                    var structValue = (StructValue) value;
                    return new LleObjectValue(Environment.MemoryAllocator, structValue.Type, structValue.Contents);
                }

                default:
                {
                    throw new NotSupportedException();
                }
            }
        }
        
    }
}