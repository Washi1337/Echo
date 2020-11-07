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
    /// Provides a default implementation of the <see cref="IValueFactory"/> interface.  
    /// </summary>
    public class ValueFactory : IValueFactory
    {
        private readonly ICilRuntimeEnvironment _environment;

        /// <summary>
        /// Creates a new instance of the <see cref="ValueFactory"/> class.
        /// </summary>
        public ValueFactory(ICilRuntimeEnvironment environment)
        {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        /// <inheritdoc />
        public IConcreteValue CreateUnknown(TypeSignature type)
        {
            while (true)
            {
                switch (type.ElementType)
                {
                    case ElementType.Boolean:
                        // For booleans, only the least significant bit is unknown (it's either zero or one).
                        return new Integer8Value(0, 0xFE);

                    case ElementType.I1:
                    case ElementType.U1:
                        return new Integer8Value(0, 0);

                    case ElementType.I2:
                    case ElementType.U2:
                    case ElementType.Char:
                        return new Integer16Value(0, 0);

                    case ElementType.I4:
                    case ElementType.U4:
                        return new Integer32Value(0, 0);

                    case ElementType.I8:
                    case ElementType.U8:
                        return new Integer64Value(0, 0);

                    case ElementType.R4:
                        return new Float32Value(0); // TODO: use unknown floats.

                    case ElementType.R8:
                        return new Float64Value(0); // TODO: use unknown floats.

                    case ElementType.FnPtr:
                    case ElementType.Ptr:
                        // TODO: this could be improved when CLI pointers are supported.
                    
                    case ElementType.I:
                    case ElementType.U:
                        return new NativeIntegerValue(0, 0, _environment.Is32Bit);

                    case ElementType.Object:
                    case ElementType.String:
                    case ElementType.Array:
                    case ElementType.SzArray:
                    case ElementType.GenericInst:
                        return new ObjectReference(null, false, _environment.Is32Bit);

                    case ElementType.Class:
                        // NOTE: This has an issue where fields defined in super classes of type will not be included.
                        return new ObjectReference(new HleObjectValue(type, _environment.Is32Bit), true, _environment.Is32Bit);

                    case ElementType.MVar:
                    case ElementType.Var:
                        // TODO: resolve type argument (maybe add a generic context parameter to this factory method?)
                        return new ObjectReference(null, false, _environment.Is32Bit);

                    case ElementType.ByRef:
                    case ElementType.ValueType:
                    case ElementType.TypedByRef:
                        return _environment.CliMarshaller.ToCliValue(
                            _environment.MemoryAllocator.AllocateObject(type),
                            type);

                    case ElementType.CModReqD:
                    case ElementType.CModOpt:
                    case ElementType.Pinned:
                        // Annotated types don't affect the actual type of value. Move to the inner type signature.

                        type = ((CustomModifierTypeSignature) type).BaseType;
                        continue;

                    default:
                        // At this point we know it is at least an object reference, as value types have been captured by
                        // other cases. Return an unknown object reference.

                        return new ObjectReference(null, false, _environment.Is32Bit);
                }
            }
        }
        
    }
}