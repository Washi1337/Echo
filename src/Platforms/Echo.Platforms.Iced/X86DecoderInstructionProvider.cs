using System;
using Echo.Core.Code;
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
        private readonly Decoder _decoder;

        /// <summary>
        /// Creates a new wrapper around the <see cref="Decoder"/> class.
        /// </summary>
        /// <param name="architecture">The x86 architecture.</param>
        /// <param name="decoder">The x86 decoder to use.</param>
        public X86DecoderInstructionProvider(IInstructionSetArchitecture<Instruction> architecture, Decoder decoder)
        {
            _decoder = decoder ?? throw new ArgumentNullException(nameof(decoder));
            Architecture = architecture;
        }

        /// <inheritdoc />
        public IInstructionSetArchitecture<Instruction> Architecture
        {
            get;
        }

        /// <inheritdoc />
        public Instruction GetInstructionAtOffset(long offset)
        {
            _decoder.IP = (ulong) offset;
            return _decoder.Decode();
        }
    }
}