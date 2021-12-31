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
    public class VirtualFrame : IMemorySpace
    {
        /*
         * local0
         * local1
         * local2
         * <return>
         * arg0
         * arg1
         * arg2
         */

        private readonly List<uint> _offsets = new();

        public VirtualFrame(IMethodDescriptor method, ValueFactory factory)
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
                currentOffset += factory.GetTypeMemoryLayout(actualType).Size;
                currentOffset = currentOffset.Align(pointerSize);
            }
        }
        
        public IMethodDescriptor Method
        {
            get;
        }

        public BitVector LocalStorage
        {
            get;
        }

        public int LocalsCount
        {
            get;
        } = 0;

        public Stack<BitVector> EvaluationStack
        {
            get;
        }

        public AddressRange AddressRange => new(0, LocalStorage.Count / 8);

        public bool IsValidAddress(long address) => AddressRange.Contains(address);

        public long GetLocalAddress(int index) => index < LocalsCount
            ? _offsets[index]
            : throw new ArgumentOutOfRangeException(nameof(index));
        
        public void ReadLocal(int index, BitVectorSpan buffer) => Read(GetLocalAddress(index), buffer);

        public void WriteLocal(int index, BitVectorSpan buffer) => Write(GetLocalAddress(index), buffer);

        public long GetArgumentAddress(int index) => index < Method.Signature!.GetTotalParameterCount()
            ? _offsets[LocalsCount + 1 + index]
            : throw new ArgumentOutOfRangeException(nameof(index));

        public void ReadArgument(int index, BitVectorSpan buffer) => Read(GetArgumentAddress(index), buffer);

        public void WriteArgument(int index, BitVectorSpan buffer) => Write(GetArgumentAddress(index), buffer);

        public void Read(long address, BitVectorSpan buffer)
        {
            LocalStorage.AsSpan((int) (address * 8), buffer.Count).CopyTo(buffer);
        }

        public void Write(long address, BitVectorSpan buffer)
        {
            buffer.CopyTo(LocalStorage.AsSpan((int) (address * 8), buffer.Count));
        }
    }
}