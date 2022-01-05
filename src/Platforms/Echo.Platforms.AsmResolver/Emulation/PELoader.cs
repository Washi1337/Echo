using System;
using AsmResolver;
using AsmResolver.IO;
using AsmResolver.PE.File;
using Echo.Concrete;
using Echo.Concrete.Memory;

namespace Echo.Platforms.AsmResolver.Emulation
{
    /// <summary>
    /// Provides a mechanism for mapping portable executable files into virtual memory.
    /// </summary>
    public class PELoader
    {
        private readonly VirtualMemory _memory;
        private long _currentAddress = 0x0040_0000;
        private const ulong _moduleAlignment = 0x0001_0000;

        /// <summary>
        /// Creates a new instance of a PE loader.
        /// </summary>
        /// <param name="memory">The virtual memory to map the executables into.</param>
        public PELoader(VirtualMemory memory)
        {
            _memory = memory;
        }

        /// <summary>
        /// Maps all sections of the provided PE file into memory.
        /// </summary>
        /// <param name="file">The file to map.</param>
        /// <returns>The new base address of the PE file.</returns>
        public long MapPE(IPEFile file)
        {
            long baseAddress = _currentAddress;
            
            // HACK: Access the original data source of the underlying PE file, so that we can read and copy the 
            // original PE header.
            uint headerLength = (uint) file.Sections[0].Offset;
            var source = file.CreateReaderAtFileOffset((uint) file.Sections[0].Offset).DataSource;
            var reader = new BinaryStreamReader(source, 0, 0, headerLength);
            
            // Map PE header at base address.
            byte[] rawHeader = new byte[headerLength];
            reader.ReadBytes(rawHeader, 0, rawHeader.Length);
            var headerSpace = new BasicMemorySpace(rawHeader);
            _memory.Map(baseAddress, headerSpace);

            // Map all sections.
            foreach (var section in file.Sections)
            {
                // Compute final virtual address.
                long va = baseAddress + section.Rva;

                // Allocate new space for the section.
                var virtualContents = new BasicMemorySpace(
                    (int) section.GetVirtualSize(),
                    section.IsContentInitializedData);

                // If this section has readable contents, write it into the memory space.
                if (section.Contents is IReadableSegment readableSegment)
                {
                    byte[] physicalContents = readableSegment.ToArray();
                    int length = Math.Min(virtualContents.BackBuffer.Count, physicalContents.Length * 8);
                    virtualContents.Write(0, new BitVector(physicalContents).AsSpan(0, length));
                }
                
                // Map section.
                _memory.Map(va, virtualContents);
            }

            // Move to next base address for second PE.
            _currentAddress += file.OptionalHeader.SizeOfImage;
            _currentAddress = (long) ((ulong) _currentAddress).Align(_moduleAlignment);

            // TODO: base relocations.
            
            return baseAddress;
        }
    }
}