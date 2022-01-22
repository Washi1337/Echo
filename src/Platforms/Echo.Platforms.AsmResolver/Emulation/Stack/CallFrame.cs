using System;
using System.Collections.Generic;
using AsmResolver;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using Echo.Concrete;
using Echo.Concrete.Memory;
using Echo.Core.Code;

namespace Echo.Platforms.AsmResolver.Emulation.Stack
{
    /// <summary>
    /// Represents a single frame in a virtual stack.
    /// </summary>
    public class CallFrame : IMemorySpace
    {
        // Stack layout sketch:
        // 
        //  offset | field
        //  -------+-----------------
        //  0      | local 0
        //  ...    | ...
        //  n      | local n
        //  n+1    | return address
        //  n+2    | arg 0
        //  ...    | ...
        //  n+m+2  | arg m

        private readonly List<uint> _offsets = new();
        private long _baseAddress;

        /// <summary>
        /// Constructs a new call stack frame.
        /// </summary>
        /// <param name="method">The method that this frame is associated with.</param>
        /// <param name="factory">A factory used for measuring the size of the frame.</param>
        /// <exception cref="ArgumentException">
        /// Occurs when the provided method is invalid or contains invalid metadata that could not be dealt with.
        /// </exception>
        public CallFrame(IMethodDescriptor method, ValueFactory factory)
        {
            if (method.Signature is null)
                throw new ArgumentException("Method does not have a valid signature.");
            
            uint pointerSize = factory.Is32Bit 
                ? (uint) sizeof(uint) 
                : sizeof(ulong);
            
            var context = GenericContext.FromMethod(method);
            Method = method;

            uint currentOffset = 0;
            bool initializeLocals = false;
            
            // Allocate local variables. 
            if (method.Resolve()?.CilMethodBody is { } body)
            {
                initializeLocals = body.InitializeLocals;
                LocalsCount = body.LocalVariables.Count;

                foreach (var local in body.LocalVariables)
                    AllocateFrameField(local.VariableType);

                Body = body;
            }
            else
            {
                Body = null;
            }

            // Allocate return address.
            _offsets.Add(currentOffset);
            currentOffset += pointerSize;

            // Allocate this parameter if required.
            if (method.Signature.HasThis)
                AllocateFrameField(method.DeclaringType?.ToTypeSignature() ?? method.Module!.CorLibTypeFactory.Object);
            
            // Allocate rest of the parameters.
            foreach (var parameterType in method.Signature.ParameterTypes)
                AllocateFrameField(parameterType);

            // Actually reserve space for the entire frame.
            LocalStorage = new BitVector((int) (currentOffset * pointerSize), false);
            
            // Initialize locals to zero when method body says we should.
            if (initializeLocals)
            {
                uint localsSize = _offsets[LocalsCount];
                LocalStorage
                    .AsSpan(0, (int) localsSize * 8)
                    .WriteBytes(0, new byte[localsSize]);
            }

            void AllocateFrameField(TypeSignature type)
            {
                _offsets.Add(currentOffset);
                var actualType = type.InstantiateGenericTypes(context);
                currentOffset += factory.GetTypeValueMemoryLayout(actualType).Size;
                currentOffset = currentOffset.Align(pointerSize);
            }
        }
        
        /// <summary>
        /// Gets the method which this frame was associated with.
        /// </summary>
        public IMethodDescriptor Method
        {
            get;
        }

        public CilMethodBody? Body
        {
            get;
        }

        /// <summary>
        /// Obtains the raw storage for frame fields such as local variables, return address and arguments. 
        /// </summary>
        public BitVector LocalStorage
        {
            get;
        }

        /// <summary>
        /// Gets the number of locals stored in the frame.
        /// </summary>
        public int LocalsCount
        {
            get;
        } = 0;

        public int ProgramCounter
        {
            get;
            set;
        }
        
        /// <summary>
        /// Gets a virtual evaluation stack associated stored the frame.
        /// </summary>
        public Stack<BitVector> EvaluationStack
        {
            get;
        } = new();

        /// <summary>
        /// Gets the static size (number of bytes excluding the evaluation stack) of the stack frame.
        /// </summary>
        public int Size => LocalStorage.Count / 8;

        /// <inheritdoc />
        public AddressRange AddressRange => new(_baseAddress, _baseAddress + LocalStorage.Count / 8);

        /// <inheritdoc />
        public bool IsValidAddress(long address) => AddressRange.Contains(address);

        /// <inheritdoc />
        public void Rebase(long baseAddress) => _baseAddress = baseAddress;

        /// <summary>
        /// Gets the address (relative to the start of the frame) to a local variable in the frame. 
        /// </summary>
        /// <param name="index">The index of the local variable to get the address for.</param>
        /// <returns>The address</returns>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when the local index is invalid.</exception>
        public long GetLocalAddress(int index) => index < LocalsCount
            ? _baseAddress + _offsets[index]
            : throw new ArgumentOutOfRangeException(nameof(index));
        
        /// <summary>
        /// Reads the value of a local variable into a buffer. 
        /// </summary>
        /// <param name="index">The index of the variable.</param>
        /// <param name="buffer">The buffer to write the data into.</param>
        public void ReadLocal(int index, BitVectorSpan buffer) => Read(GetLocalAddress(index), buffer);

        /// <summary>
        /// Assigns a new value to a local variable. 
        /// </summary>
        /// <param name="index">The index of the variable.</param>
        /// <param name="buffer">The buffer containing the new data.</param>
        public void WriteLocal(int index, BitVectorSpan buffer) => Write(GetLocalAddress(index), buffer);

        /// <summary>
        /// Gets the address (relative to the start of the frame) to an argument in the frame. 
        /// </summary>
        /// <param name="index">The index of the argument to get the address for.</param>
        /// <returns>The address</returns>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when the argument index is invalid.</exception>
        public long GetArgumentAddress(int index) => index < Method.Signature!.GetTotalParameterCount()
            ? _baseAddress + _offsets[LocalsCount + 1 + index]
            : throw new ArgumentOutOfRangeException(nameof(index));

        /// <summary>
        /// Reads the value of an argument into a buffer. 
        /// </summary>
        /// <param name="index">The index of the argument to read.</param>
        /// <param name="buffer">The buffer to write the data into.</param>
        public void ReadArgument(int index, BitVectorSpan buffer) => Read(GetArgumentAddress(index), buffer);

        /// <summary>
        /// Assigns a new value to an argument. 
        /// </summary>
        /// <param name="index">The index of the argument.</param>
        /// <param name="buffer">The buffer containing the new data.</param>
        public void WriteArgument(int index, BitVectorSpan buffer) => Write(GetArgumentAddress(index), buffer);

        /// <inheritdoc />
        public void Read(long address, BitVectorSpan buffer)
        {
            LocalStorage.AsSpan((int) (address - _baseAddress) * 8, buffer.Count).CopyTo(buffer);
        }

        /// <inheritdoc />
        public void Write(long address, BitVectorSpan buffer)
        {
            buffer.CopyTo(LocalStorage.AsSpan((int) (address - _baseAddress) * 8, buffer.Count));
        }

        /// <inheritdoc />
        public void Write(long address, ReadOnlySpan<byte> buffer)
        {
            LocalStorage.AsSpan().WriteBytes((int) (address - _baseAddress) * 8, buffer);
        }

        public override string ToString() => $"{Method.FullName}+IL_{ProgramCounter:X4}";
    }
}