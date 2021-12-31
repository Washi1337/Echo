using System;
using System.Linq;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Serialized;
using AsmResolver.PE;
using AsmResolver.PE.File;
using Echo.Concrete;
using Echo.Concrete.Memory;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Mocks;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation
{
    public class PELoaderTest : IClassFixture<MockModuleFixture>
    {
        private readonly MockModuleFixture _fixture;

        public PELoaderTest(MockModuleFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void LoadMock()
        {
            var memory = new VirtualMemory();
            var loader = new PELoader(memory);

            // Grab the underlying PE file.
            var file = ((SerializedPEImage) ((SerializedModuleDefinition) _fixture.MockModule).ReaderContext.Image)
                .PEFile;
            
            // Map into virtual memory.
            long baseAddress = loader.MapPE(file);
            
            // Reload PE File from virtual memory as a mapped PE file.
            var dataSource = new VirtualDataSource(memory, baseAddress, file.OptionalHeader.SizeOfImage);
            var newFile = PEFile.FromDataSource(dataSource, PEMappingMode.Mapped);
            
            // Verify sections.
            Assert.Equal(newFile.Sections.Select(s => s.Name), file.Sections.Select(s => s.Name));

            // Try to load the .NET module and check name. 
            // Should be a pretty good indication that virtual memory works.
            var module = ModuleDefinition.FromFile(newFile);
            Assert.Equal(_fixture.MockModule.Name, module.Name);
        }

        [Fact]
        public void Test()
        {
            var memory = new VirtualMemory();
            var loader = new PELoader(memory);

            // Grab the underlying PE file.
            var file = ((SerializedPEImage) ((SerializedModuleDefinition) _fixture.MockModule).ReaderContext.Image)
                .PEFile;
            
            // Map into virtual memory.
            long baseAddress = loader.MapPE(file);

            // Allocate some stack memory.
            var stack = new VirtualStack(0x10000);
            memory.Map(0x7fff_0000, stack);
            
            // Push some random frame.
            stack.Push(new VirtualFrame(_fixture.GetTestMethod(nameof(TestClass.MultipleLocalsMultipleArguments)),
                new ValueFactory(_fixture.MockModule, false)));
            
            // Read from the base address.
            var buffer = new BitVector(16, false).AsSpan();
            memory.Read(baseAddress, buffer);
            
            Assert.Equal(0x5a4d, BitConverter.ToUInt16(buffer.Bits));
            
            memory.Read(0x7fff_0000, buffer);
            Assert.Equal(0, BitConverter.ToUInt16(buffer.Bits));
        }
    }
}