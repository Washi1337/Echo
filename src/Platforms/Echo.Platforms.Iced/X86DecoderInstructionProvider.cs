using System;
using System.IO;
using Echo.Code;
using Iced.Intel;

namespace Echo.Platforms.Iced
{
    /// <summary>
    /// Provides an adapter for the <see cref="Decoder"/> class to work with any object that requires an instance of the
    /// <see cref="IStaticInstructionProvider{TInstruction}"/> interface, and decodes x86 instructions on every
    /// call to the <see cref="GetInstructionAtOffset"/> method. 
    /// </summary>
    public class X86DecoderInstructionProvider : IStaticInstructionProvider<Instruction>
    {
        private readonly Stream _inputStream;
        private readonly Decoder _decoder;

        /// <summary>
        /// Creates a new wrapper around a raw stream containing x86 code.
        /// </summary>
        /// <param name="architecture">The x86 architecture.</param>
        /// <param name="inputCode">The raw code stream.</param>
        /// <param name="bitness">The bitness of the x86 code. This value must be either 16, 32 or 64.</param>
        public X86DecoderInstructionProvider(
            IArchitecture<Instruction> architecture,
            byte[] inputCode,
            int bitness)
            : this(architecture, new MemoryStream(inputCode), bitness, 0, DecoderOptions.None)
        {
        }
        
        /// <summary>
        /// Creates a new wrapper around a raw stream containing x86 code.
        /// </summary>
        /// <param name="architecture">The x86 architecture.</param>
        /// <param name="inputStream">The raw code stream.</param>
        /// <param name="bitness">The bitness of the x86 code. This value must be either 16, 32 or 64.</param>
        public X86DecoderInstructionProvider(
            IArchitecture<Instruction> architecture,
            Stream inputStream,
            int bitness)
            : this(architecture, inputStream, bitness, 0, DecoderOptions.None)
        {
        }
        
        /// <summary>
        /// Creates a new wrapper around a raw stream containing x86 code.
        /// </summary>
        /// <param name="architecture">The x86 architecture.</param>
        /// <param name="inputStream">The raw code stream.</param>
        /// <param name="bitness">The bitness of the x86 code. This value must be either 16, 32 or 64.</param>
        /// <param name="baseAddress">The base address of the code stream.</param>
        /// <param name="decoderOptions">Additional decoder options that need to be passed onto the Iced decoder.</param>
        public X86DecoderInstructionProvider(
            IArchitecture<Instruction> architecture,
            Stream inputStream, 
            int bitness,
            ulong baseAddress,
            DecoderOptions decoderOptions)
        {
            _inputStream = inputStream ?? throw new ArgumentNullException(nameof(inputStream));
            if (!inputStream.CanRead)
                throw new ArgumentException("Input stream must be readable.");
            if (!inputStream.CanSeek)
                throw new ArgumentException("Input stream must be seekable.");
            
            _decoder = Decoder.Create(bitness, new StreamCodeReader(inputStream), decoderOptions);
            Architecture = architecture;
            BaseAddress = baseAddress;
        }

        /// <inheritdoc />
        public IArchitecture<Instruction> Architecture
        {
            get;
        }

        /// <summary>
        /// Gets the base address of the code stream.
        /// </summary>
        public ulong BaseAddress
        {
            get;
        }

        /// <inheritdoc />
        public Instruction GetInstructionAtOffset(long offset)
        {
            if (offset - (long) BaseAddress < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            
            _inputStream.Position = offset - (long) BaseAddress;
            _decoder.IP = (ulong) offset;
            return _decoder.Decode();
        }
    }
}