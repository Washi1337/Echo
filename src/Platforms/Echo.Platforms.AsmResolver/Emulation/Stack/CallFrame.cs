using System;
using System.Collections.Generic;
using AsmResolver;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using Echo.Concrete;
using Echo.Concrete.Memory;
using Echo.Core;
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

        private readonly bool _initializeLocals;
        private readonly List<uint> _offsets = new();
        private BitVector _localStorage;
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
            : this(method, factory, false)
        {
        }
        
        internal CallFrame(IMethodDescriptor method, ValueFactory factory, bool isRoot)
        {
            IsRoot = isRoot;
            EvaluationStack = new EvaluationStack(factory);
            
            if (method.Signature is null)
                throw new ArgumentException("Method does not have a valid signature.");
            
            uint pointerSize = factory.PointerSize;
            
            var context = GenericContext.FromMethod(method);
            Method = method;

            uint currentOffset = 0;
            _initializeLocals = false;
            
            // Allocate local variables. 
            if (method.Resolve()?.CilMethodBody is not { } body)
            {
                Body = null;
            }
            else
            {
                _initializeLocals = body.InitializeLocals;
                LocalsCount = body.LocalVariables.Count;

                foreach (var local in body.LocalVariables)
                    AllocateFrameField(local.VariableType);

                Body = body;
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
            _localStorage = new BitVector((int) (currentOffset * pointerSize), false);
            
            // Initialize locals to zero when method body says we should.
            if (_initializeLocals)
            {
                uint localsSize = _offsets[LocalsCount];
                _localStorage
                    .AsSpan(0, (int) localsSize * 8)
                    .Write(new byte[localsSize]);
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
        /// Gets a value indicating the frame is the root frame of the call stack.
        /// </summary>
        public bool IsRoot
        {
            get;
        }
        
        /// <summary>
        /// Gets the method which this frame was associated with.
        /// </summary>
        public IMethodDescriptor Method
        {
            get;
        }

        /// <summary>
        /// Gets the managed body of the method that this frame is associated with (if available).
        /// </summary>
        public CilMethodBody? Body
        {
            get;
        }

        /// <summary>
        /// Gets the number of locals stored in the frame.
        /// </summary>
        public int LocalsCount
        {
            get;
        }

        /// <summary>
        /// Gets the offset within the method body of the next instruction to evaluate.
        /// </summary>
        public int ProgramCounter
        {
            get;
            set;
        }
        
        /// <summary>
        /// Gets a virtual evaluation stack associated stored the frame.
        /// </summary>
        public EvaluationStack EvaluationStack
        {
            get;
        }

        /// <summary>
        /// Gets the number of bytes (excluding the evaluation stack) the stack frame spans.
        /// </summary>
        public int Size => _localStorage.Count / 8;

        /// <summary>
        /// Gets a value indicating whether the frame can be extended with extra stack memory.
        /// </summary>
        public bool CanAllocateMemory
        {
            get;
            internal set;
        }

        /// <inheritdoc />
        public AddressRange AddressRange => new(_baseAddress, _baseAddress + _localStorage.ByteCount);

        /// <inheritdoc />
        public bool IsValidAddress(long address) => AddressRange.Contains(address);

        /// <summary>
        /// Allocates local stack memory in the stack frame.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public long Allocate(int size)
        {
            if (!CanAllocateMemory)
                throw new InvalidOperationException("Stack frame cannot be resized in the current state.");
            if (size < 0)
                throw new ArgumentOutOfRangeException(nameof(size));

            long address = AddressRange.End;
            _localStorage = _localStorage.Resize(_localStorage.Count + size * 8, false);
            if (!_initializeLocals)
                _localStorage.AsSpan((int) (address - _baseAddress)).MarkFullyUnknown();
            
            return address;
        }

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
            _localStorage.AsSpan((int) (address - _baseAddress) * 8, buffer.Count).CopyTo(buffer);
        }

        /// <inheritdoc />
        public void Write(long address, BitVectorSpan buffer)
        {
            buffer.CopyTo(_localStorage.AsSpan((int) (address - _baseAddress) * 8, buffer.Count));
        }

        /// <inheritdoc />
        public void Write(long address, ReadOnlySpan<byte> buffer)
        {
            _localStorage.AsSpan((int) (address - _baseAddress) * 8, buffer.Length).Write(buffer);
        }

        /// <inheritdoc />
        public override string ToString() => $"{Method.FullName}+IL_{ProgramCounter:X4}";
    }
}