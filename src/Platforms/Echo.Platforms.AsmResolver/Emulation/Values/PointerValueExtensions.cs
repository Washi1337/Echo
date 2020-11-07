using System;
using AsmResolver.DotNet.Memory;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Echo.Concrete.Values;
using Echo.Concrete.Values.ReferenceType;
using Echo.Concrete.Values.ValueType;

namespace Echo.Platforms.AsmResolver.Emulation.Values
{
    /// <summary>
    /// Provides extension methods that allow for reading and writing .NET structures from a pointer value.
    /// </summary>
    public static class PointerValueExtensions
    {
        /// <summary>
        /// Reads a single .NET structure at the provided offset.
        /// </summary>
        /// <param name="self">The base pointer value to read from.</param>
        /// <param name="offset">The offset to start reading.</param>
        /// <param name="memoryAllocator">The memory allocator responsible for managing type layouts.</param>
        /// <param name="typeLayout">The type layout to read.</param>
        /// <returns>The read structure.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Occurs when the offset does not fall within the memory range.
        /// </exception>
        public static IConcreteValue ReadStruct(this IPointerValue self, int offset, IMemoryAllocator memoryAllocator, TypeMemoryLayout typeLayout)
        {
            return typeLayout.Type.ToTypeSignature().ElementType switch
            {
                ElementType.Boolean => self.ReadInteger8(offset),
                ElementType.Char => self.ReadInteger16(offset),
                ElementType.I1 => self.ReadInteger8(offset),
                ElementType.U1 => self.ReadInteger8(offset),
                ElementType.I2 => self.ReadInteger16(offset),
                ElementType.U2 => self.ReadInteger16(offset),
                ElementType.I4 => self.ReadInteger32(offset),
                ElementType.U4 => self.ReadInteger32(offset),
                ElementType.I8 => self.ReadInteger64(offset),
                ElementType.U8 => self.ReadInteger64(offset),
                ElementType.R4 => self.ReadFloat32(offset),
                ElementType.R8 => self.ReadFloat64(offset),
                ElementType.ValueType => self.ReadStructSlow(offset, memoryAllocator, typeLayout),
                ElementType.I => self.Is32Bit ? (IntegerValue) self.ReadInteger32(offset) : self.ReadInteger64(offset),
                ElementType.U => self.Is32Bit ? (IntegerValue) self.ReadInteger32(offset) : self.ReadInteger64(offset),
                ElementType.Enum => ReadEnumValue(self, offset, memoryAllocator, typeLayout),
                _ => new UnknownValue()
            };
        }

        private static IConcreteValue ReadEnumValue(IPointerValue self, int offset, IMemoryAllocator memoryAllocator, TypeMemoryLayout typeLayout)
        {
            var underlyingTypeLayout = memoryAllocator.GetTypeMemoryLayout(typeLayout.Type.Resolve().GetEnumUnderlyingType());
            return self.ReadStruct(offset, memoryAllocator, underlyingTypeLayout);
        }

        private static IConcreteValue ReadStructSlow(this IPointerValue self, int offset, IMemoryAllocator memoryAllocator, TypeMemoryLayout typeLayout)
        {
            Span<byte> contents = stackalloc byte[(int) typeLayout.Size];
            Span<byte> bitmask = stackalloc byte[(int) typeLayout.Size];
            self.ReadBytes(offset, contents, bitmask);
            
            var structValue = (IPointerValue) memoryAllocator.AllocateObject(typeLayout.Type.ToTypeSignature(), false);
            structValue.WriteBytes(0, contents, bitmask);
            return structValue;
        }

        /// <summary>
        /// Writes a single .NET structure at the provided offset.
        /// </summary>
        /// <param name="self">The base pointer to write to.</param>
        /// <param name="offset">The offset to start writing at.</param>
        /// <param name="memoryAllocator">The memory allocator responsible for managing type layouts.</param>
        /// <param name="typeLayout">The structure type to write.</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Occurs when the offset does not fall within the memory range.
        /// </exception>
        public static void WriteStruct(this IPointerValue self, 
            int offset,
            IMemoryAllocator memoryAllocator,
            TypeMemoryLayout typeLayout,
            IConcreteValue value)
        {
            switch (typeLayout.Type.ToTypeSignature().ElementType)
            {
                case ElementType.Boolean:
                case ElementType.I1:
                case ElementType.U1:
                    self.WriteInteger8(offset, (Integer8Value) value);
                    break;
                
                case ElementType.Char:
                case ElementType.I2:
                case ElementType.U2:
                    self.WriteInteger16(offset, (Integer16Value) value);
                    break;
                
                case ElementType.I4:
                case ElementType.U4:
                case ElementType.I when self.Is32Bit:
                case ElementType.U when self.Is32Bit:
                    self.WriteInteger32(offset, (Integer32Value) value);
                    break;
                
                case ElementType.I8:
                case ElementType.U8:
                case ElementType.I when !self.Is32Bit:
                case ElementType.U when !self.Is32Bit:
                    self.WriteInteger64(offset, (Integer64Value) value);
                    break;
                
                case ElementType.R4:
                    self.WriteFloat32(offset, (Float32Value) value);
                    break;
                
                case ElementType.R8:
                    self.WriteFloat64(offset, (Float64Value) value);
                    break;
                
                case ElementType.ValueType:
                    self.WriteStructSlow(offset, typeLayout, value);
                    break;
                
                case ElementType.Enum:
                    WriteEnumValue(self, offset, memoryAllocator, typeLayout, value);
                    break;
                
                case ElementType.String:
                case ElementType.Ptr:
                case ElementType.ByRef:
                case ElementType.Class:
                case ElementType.Array:
                case ElementType.GenericInst:
                case ElementType.FnPtr:
                case ElementType.Object:
                case ElementType.SzArray:
                    // We cannot really know the raw value of object pointers, write an unknown value.
                    if (self.Is32Bit)
                        self.WriteInteger32(offset, new Integer32Value(0, 0));
                    else 
                        self.WriteInteger64(offset, new Integer64Value(0, 0));
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void WriteEnumValue(
            IPointerValue self, 
            int offset, 
            IMemoryAllocator memoryAllocator,
            TypeMemoryLayout typeLayout,
            IConcreteValue value)
        {
            var enumLayout = memoryAllocator.GetTypeMemoryLayout(typeLayout.Type.Resolve().GetEnumUnderlyingType());
            self.WriteStruct(offset, memoryAllocator, enumLayout, value);
        }

        private static void WriteStructSlow(this IPointerValue self, int offset, TypeMemoryLayout typeLayout, IConcreteValue value)
        {
            Span<byte> contents = stackalloc byte[(int) typeLayout.Size];
            Span<byte> bitmask = stackalloc byte[(int) typeLayout.Size];
            
            if (value is IValueTypeValue valueTypeValue)
            {
                // Value is a structure, we can get the raw contents of the struct.
                valueTypeValue.GetBits(contents);
                valueTypeValue.GetMask(bitmask);
            }
            else
            {
                // Value is not a struct value, mark bits as unknown.
                contents.Fill(0);
                bitmask.Fill(0);
            }
            
            self.WriteBytes(offset, contents, bitmask);
        }
        
    }
}