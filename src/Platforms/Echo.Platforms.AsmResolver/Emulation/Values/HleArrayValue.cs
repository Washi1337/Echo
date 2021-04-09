using AsmResolver.DotNet;
using AsmResolver.DotNet.Memory;
using AsmResolver.DotNet.Signatures.Types;
using Echo.Concrete.Values;
using Echo.Core;
using Echo.Core.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Values
{
    /// <summary>
    /// Provides a high level representation of a sequence of values. 
    /// </summary>
    public class HleArrayValue : IDotNetArrayValue
    {
        private readonly IValueFactory _valueFactory;
        private readonly IConcreteValue[] _values;

        /// <summary>
        /// Creates a new array value.
        /// </summary>
        /// <param name="valueFactory">The object used to create default elements with.</param>
        /// <param name="elementType">The type of the elements stored in the array.</param>
        /// <param name="length">The number of elements stored in the array.</param>
        public HleArrayValue(IValueFactory valueFactory, TypeSignature elementType, int length)
        {
            ElementType = elementType;
            Type = elementType.MakeSzArrayType();
            _valueFactory = valueFactory;
            
            _values = new IConcreteValue[length];
            for (int i = 0; i < length; i++)
                _values[i] = _valueFactory.CreateValue(ElementType, true);
        }

        /// <inheritdoc />
        public bool IsKnown
        {
            get
            {
                foreach (var value in _values)
                {
                    if (!value.IsKnown)
                        return false;
                }

                return true;
            }
        }

        /// <inheritdoc />
        public int Size => (int) (_values.Length * _valueFactory.GetTypeMemoryLayout(ElementType).Size);

        /// <inheritdoc />
        public IValue Copy()
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public bool IsValueType => false;

        /// <inheritdoc />
        public Trilean IsZero => false;

        /// <inheritdoc />
        public Trilean IsNonZero => true;

        /// <inheritdoc />
        public Trilean IsPositive => true;

        /// <inheritdoc />
        public Trilean IsNegative => false;

        /// <inheritdoc />
        public TypeSignature Type
        {
            get;
        }

        /// <summary>
        /// Gets the type of the elements stored int the array.
        /// </summary>
        public TypeSignature ElementType
        {
            get;
        }

        private CorLibTypeFactory CorLibTypeFactory => ElementType.Module.CorLibTypeFactory;

        /// <inheritdoc />
        public int Length => _values.Length;

        /// <inheritdoc />
        public ICliValue LoadElement(int index, TypeMemoryLayout typeLayout, ICliMarshaller marshaller) =>
            marshaller.ToCliValue(_values[index], typeLayout.Type.ToTypeSignature());

        /// <inheritdoc />
        public NativeIntegerValue LoadElementI(int index, ICliMarshaller marshaller) =>
            marshaller.ToCliValue(_values[index], CorLibTypeFactory.IntPtr).InterpretAsI(_valueFactory.Is32Bit);

        /// <inheritdoc />
        public I4Value LoadElementI1(int index, ICliMarshaller marshaller) =>
            marshaller.ToCliValue(_values[index], CorLibTypeFactory.SByte).InterpretAsI1();

        /// <inheritdoc />
        public I4Value LoadElementI2(int index, ICliMarshaller marshaller) =>
            marshaller.ToCliValue(_values[index], CorLibTypeFactory.Int16).InterpretAsI2();

        /// <inheritdoc />
        public I4Value LoadElementI4(int index, ICliMarshaller marshaller) =>
            marshaller.ToCliValue(_values[index], CorLibTypeFactory.Int32).InterpretAsI4();

        /// <inheritdoc />
        public I8Value LoadElementI8(int index, ICliMarshaller marshaller) =>
            marshaller.ToCliValue(_values[index], CorLibTypeFactory.Int64).InterpretAsI8();

        /// <inheritdoc />
        public I4Value LoadElementU1(int index, ICliMarshaller marshaller) =>
            marshaller.ToCliValue(_values[index], CorLibTypeFactory.Byte).InterpretAsU1();

        /// <inheritdoc />
        public I4Value LoadElementU2(int index, ICliMarshaller marshaller) =>
            marshaller.ToCliValue(_values[index], CorLibTypeFactory.UInt16).InterpretAsU2();

        /// <inheritdoc />
        public I4Value LoadElementU4(int index, ICliMarshaller marshaller) =>
            marshaller.ToCliValue(_values[index], CorLibTypeFactory.UInt32).InterpretAsU4();

        /// <inheritdoc />
        public FValue LoadElementR4(int index, ICliMarshaller marshaller) =>
            marshaller.ToCliValue(_values[index], CorLibTypeFactory.Single).InterpretAsR4();

        /// <inheritdoc />
        public FValue LoadElementR8(int index, ICliMarshaller marshaller) =>
            marshaller.ToCliValue(_values[index], CorLibTypeFactory.Double).InterpretAsR8();

        /// <inheritdoc />
        public OValue LoadElementRef(int index, ICliMarshaller marshaller) =>
            marshaller.ToCliValue(_values[index], CorLibTypeFactory.Object).InterpretAsRef(_valueFactory.Is32Bit);

        /// <inheritdoc />
        public void StoreElement(int index, TypeMemoryLayout typeLayout, ICliValue value, ICliMarshaller marshaller) =>
            _values[index] = marshaller.ToCtsValue(value, typeLayout.Type.ToTypeSignature());

        /// <inheritdoc />
        public void StoreElementI(int index, NativeIntegerValue value, ICliMarshaller marshaller) =>
            _values[index] = marshaller.ToCtsValue(value, CorLibTypeFactory.IntPtr);

        /// <inheritdoc />
        public void StoreElementI1(int index, I4Value value, ICliMarshaller marshaller) =>
            _values[index] = marshaller.ToCtsValue(value, CorLibTypeFactory.SByte);

        /// <inheritdoc />
        public void StoreElementI2(int index, I4Value value, ICliMarshaller marshaller) =>
            _values[index] = marshaller.ToCtsValue(value, CorLibTypeFactory.Int16);

        /// <inheritdoc />
        public void StoreElementI4(int index, I4Value value, ICliMarshaller marshaller) =>
            _values[index] = marshaller.ToCtsValue(value, CorLibTypeFactory.Int32);

        /// <inheritdoc />
        public void StoreElementI8(int index, I8Value value, ICliMarshaller marshaller) =>
            _values[index] = marshaller.ToCtsValue(value, CorLibTypeFactory.Int64);

        /// <inheritdoc />
        public void StoreElementU1(int index, I4Value value, ICliMarshaller marshaller) =>
            _values[index] = marshaller.ToCtsValue(value, CorLibTypeFactory.Byte);

        /// <inheritdoc />
        public void StoreElementU2(int index, I4Value value, ICliMarshaller marshaller) =>
            _values[index] = marshaller.ToCtsValue(value, CorLibTypeFactory.UInt16);

        /// <inheritdoc />
        public void StoreElementU4(int index, I4Value value, ICliMarshaller marshaller) =>
            _values[index] = marshaller.ToCtsValue(value, CorLibTypeFactory.UInt32);

        /// <inheritdoc />
        public void StoreElementR4(int index, FValue value, ICliMarshaller marshaller) =>
            _values[index] = marshaller.ToCtsValue(value, CorLibTypeFactory.Single);

        /// <inheritdoc />
        public void StoreElementR8(int index, FValue value, ICliMarshaller marshaller) =>
            _values[index] = marshaller.ToCtsValue(value, CorLibTypeFactory.Double);

        /// <inheritdoc />
        public void StoreElementRef(int index, OValue value, ICliMarshaller marshaller) =>
            _values[index] = marshaller.ToCtsValue(value, CorLibTypeFactory.Object);
    }
}