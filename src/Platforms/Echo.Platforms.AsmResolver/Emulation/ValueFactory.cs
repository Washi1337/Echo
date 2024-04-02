using System;
using System.Collections.Generic;
using System.Reflection;
using AsmResolver;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Memory;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation.Runtime;

namespace Echo.Platforms.AsmResolver.Emulation
{
    /// <summary>
    /// Provides a service for querying information about- and constructing new values.
    /// </summary>
    public class ValueFactory
    {
        /// <summary>
        /// Creates a new value factory.
        /// </summary>
        /// <param name="typeManager"></param>
        public ValueFactory(RuntimeTypeManager typeManager)
        {
            TypeManager = typeManager;
            ClrMockMemory = new ClrMockMemory();
            BitVectorPool = new BitVectorPool();
            Marshaller = new CliMarshaller(this);

            var contextModule = typeManager.ContextModule;
            
            DecimalType = new TypeReference(
                contextModule, 
                contextModule.CorLibTypeFactory.CorLibScope, 
                nameof(System),
                nameof(Decimal)).Resolve()!;
            
            InvalidProgramExceptionType = new TypeReference(
                contextModule, 
                contextModule.CorLibTypeFactory.CorLibScope, 
                nameof(System),
                nameof(InvalidProgramException)).Resolve()!;
            
            TypeInitializationExceptionType = new TypeReference(
                contextModule, 
                contextModule.CorLibTypeFactory.CorLibScope, 
                nameof(System),
                nameof(TypeInitializationException)).Resolve()!;
            
            NullReferenceExceptionType = new TypeReference(
                contextModule, 
                contextModule.CorLibTypeFactory.CorLibScope, 
                nameof(System),
                nameof(NullReferenceException)).Resolve()!;
            
            InvalidProgramExceptionType = new TypeReference(
                contextModule, 
                contextModule.CorLibTypeFactory.CorLibScope, 
                nameof(System),
                nameof(InvalidProgramException)).Resolve()!;
            
            IndexOutOfRangeExceptionType = new TypeReference(
                contextModule, 
                contextModule.CorLibTypeFactory.CorLibScope, 
                nameof(System),
                nameof(IndexOutOfRangeException)).Resolve()!;
            
            StackOverflowExceptionType = new TypeReference(
                contextModule, 
                contextModule.CorLibTypeFactory.CorLibScope, 
                nameof(System),
                nameof(StackOverflowException)).Resolve()!;
            
            MissingMethodExceptionType = new TypeReference(
                contextModule, 
                contextModule.CorLibTypeFactory.CorLibScope, 
                nameof(System),
                nameof(MissingMethodException)).Resolve()!;
            
            InvalidCastExceptionType = new TypeReference(
                contextModule, 
                contextModule.CorLibTypeFactory.CorLibScope, 
                nameof(System),
                nameof(InvalidCastException)).Resolve()!;
            
            OverflowExceptionType = new TypeReference(
                contextModule, 
                contextModule.CorLibTypeFactory.CorLibScope, 
                nameof(System),
                nameof(OverflowException)).Resolve()!;
        }

        /// <summary>
        /// Gets the manifest module to use for context.
        /// </summary>
        public ModuleDefinition ContextModule => TypeManager.ContextModule;

        /// <summary>
        /// Gets a value indicating whether the environment is a 32-bit or 64-bit system.
        /// </summary>
        public bool Is32Bit => TypeManager.Is32Bit;

        /// <summary>
        /// Gets the size in bytes of a pointer in the current environment.
        /// </summary>
        public uint PointerSize => TypeManager.PointerSize;

        /// <summary>
        /// Gets the CLR mock memory used for managing method tables (types).
        /// </summary>
        public ClrMockMemory ClrMockMemory
        {
            get;
        }

        /// <summary>
        /// Gets the service responsible for managing types the value factory can create and initialize.
        /// </summary>
        public RuntimeTypeManager TypeManager
        {
            get;
        }

        /// <summary>
        /// Gets a reference to the <see cref="Decimal"/> type. 
        /// </summary>
        public ITypeDescriptor DecimalType
        {
            get;
        }

        /// <summary>
        /// Gets a reference to the <see cref="InvalidProgramException"/> type. 
        /// </summary>
        public ITypeDescriptor InvalidProgramExceptionType
        {
            get;
        }

        /// <summary>
        /// Gets a reference to the <see cref="TypeInitializationException"/> type. 
        /// </summary>
        public TypeDefinition TypeInitializationExceptionType
        {
            get;
        }

        /// <summary>
        /// Gets a reference to the <see cref="NullReferenceException"/> type. 
        /// </summary>
        public ITypeDescriptor NullReferenceExceptionType
        {
            get;
        }

        /// <summary>
        /// Gets a reference to the <see cref="IndexOutOfRangeException"/> type. 
        /// </summary>
        public ITypeDescriptor IndexOutOfRangeExceptionType
        {
            get;
        }

        /// <summary>
        /// Gets a reference to the <see cref="StackOverflowException"/> type. 
        /// </summary>
        public ITypeDescriptor StackOverflowExceptionType
        {
            get;
        }

        /// <summary>
        /// Gets a reference to the <see cref="MissingMethodException"/> type. 
        /// </summary>
        public ITypeDescriptor MissingMethodExceptionType
        {
            get;
        }

        /// <summary>
        /// Gets a reference to the <see cref="InvalidCastException"/> type. 
        /// </summary>
        public ITypeDescriptor InvalidCastExceptionType
        {
            get;
        }

        /// <summary>
        /// Gets a reference to the <see cref="OverflowException"/> type. 
        /// </summary>
        public ITypeDescriptor OverflowExceptionType
        {
            get;
        }

        /// <summary>
        /// Gets the bitvector pool that this factory uses for creating and reusing bit vectors. 
        /// </summary>
        public BitVectorPool BitVectorPool
        {
            get;
        }

        /// <summary>
        /// Gets the service responsible for marshalling values into stack slots and back. 
        /// </summary>
        public CliMarshaller Marshaller
        {
            get;
        }

       

        /// <summary>
        /// Creates a new native integer bit vector containing the null reference.
        /// </summary>
        /// <returns>The constructed bit vector.</returns>
        public BitVector CreateNull() => CreateNativeInteger(true);

        /// <summary>
        /// Creates a new native integer bit vector.
        /// </summary>
        /// <param name="initialize">
        /// <c>true</c> if the value should be set to 0, <c>false</c> if the integer should remain unknown.
        /// </param>
        /// <returns>The constructed bit vector.</returns>
        public BitVector CreateNativeInteger(bool initialize) => new((int) PointerSize * 8, initialize);
        
        /// <summary>
        /// Creates a new native integer bit vector.
        /// </summary>
        /// <param name="value">The value to initialize the integer with.</param>
        /// <returns>The constructed bit vector.</returns>
        public BitVector CreateNativeInteger(long value)
        {
            var vector = new BitVector((int) (PointerSize * 8), false);
            vector.AsSpan().WriteNativeInteger(value, Is32Bit);
            return vector;
        }

        /// <summary>
        /// Creates a new 32-bit vector from the bit vector pool containing the boolean value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The vector.</returns>
        public BitVector CreateBoolean(Trilean value)
        {
            var vector = new BitVector(32, true);
            var span = vector.AsSpan();
            span[0] = value;
            return vector;
        }

        /// <summary>
        /// Rents a new native integer bit vector containing the null reference.
        /// </summary>
        /// <returns>The constructed bit vector.</returns>
        public BitVector RentNull() => RentNativeInteger(true);

        /// <summary>
        /// Rents a native integer bit vector from the bit vector pool.
        /// </summary>
        /// <param name="initialize">
        /// <c>true</c> if the value should be set to 0, <c>false</c> if the integer should remain unknown.
        /// </param>
        /// <returns>The rented bit vector.</returns>
        public BitVector RentNativeInteger(bool initialize) => BitVectorPool.RentNativeInteger(Is32Bit, initialize);

        /// <summary>
        /// Rents a native integer bit vector from the bit vector pool.
        /// </summary>
        /// <param name="value">The value to initialize the integer with.</param>
        /// <returns>The rented bit vector.</returns>
        public BitVector RentNativeInteger(long value)
        {
            var vector = BitVectorPool.RentNativeInteger(Is32Bit, false);
            vector.AsSpan().WriteNativeInteger(value, Is32Bit);
            return vector;
        }

        /// <summary>
        /// Rents a 32-bit vector from the bit vector pool containing the boolean value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The vector</returns>
        public BitVector RentBoolean(Trilean value)
        {
            var vector = BitVectorPool.Rent(32, true);
            var span = vector.AsSpan();
            span[0] = value;
            return vector;
        }
        
        /// <summary>
        /// Creates a new bit vector that can be used to represent an instance of the provided type.
        /// </summary>
        /// <param name="type">The type to represent.</param>
        /// <param name="initialize">
        /// <c>true</c> if the value should be set to 0, <c>false</c> if the value should remain unknown.
        /// </param>
        /// <returns>The constructed bit vector.</returns>
        public BitVector CreateValue(TypeSignature type, bool initialize)
        {
            type = type.StripModifiers();
            uint size = TypeManager.GetMethodTable(type).ValueLayout.Size;
            if (type.ElementType != ElementType.Boolean)
                return new BitVector((int) size * 8, initialize);
            
            // For booleans, we only set the LSB to unknown if necessary.
            var result = new BitVector((int) size * 8, true);
            
            if (!initialize)
            {
                var span = result.AsSpan();
                span[0] = Trilean.Unknown;
            }

            return result;
        }
        
        /// <summary>
        /// Rents a bit vector from the pool that can be used to represent an instance of the provided type.
        /// </summary>
        /// <param name="type">The type to represent.</param>
        /// <param name="initialize">
        /// <c>true</c> if the value should be set to 0, <c>false</c> if the value should remain unknown.
        /// </param>
        /// <returns>The rented bit vector.</returns>
        public BitVector RentValue(TypeSignature type, bool initialize)
        {
            type = type.StripModifiers();
            uint size = TypeManager.GetMethodTable(type).ValueLayout.Size;
            if (type.ElementType != ElementType.Boolean)
                return BitVectorPool.Rent((int) size * 8, initialize);

            // For booleans, we only set the LSB to unknown if necessary.
            var result = BitVectorPool.Rent((int) size * 8, true);
            
            if (!initialize)
            {
                var span = result.AsSpan();
                span[0] = Trilean.Unknown;
            }

            return result;
        }
        
        
      
    }
}