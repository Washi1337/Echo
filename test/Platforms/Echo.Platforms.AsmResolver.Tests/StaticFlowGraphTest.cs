using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Echo.ControlFlow.Regions;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests
{
    public class StaticFlowGraphTest : IClassFixture<MockModuleFixture>
    {
        private MockModuleFixture _fixture;

        public StaticFlowGraphTest(MockModuleFixture fixture)
        {
            _fixture = fixture;
        }
        
        [Fact]
        public void ExceptionHandlerWithHandlerEndOutsideOfMethod()
        {
            // https://github.com/Washi1337/Echo/issues/101

            // Set up test case.
            var module = new ModuleDefinition("Module");
            var method = new MethodDefinition("MyMethod", MethodAttributes.Static,
                MethodSignature.CreateStatic(module.CorLibTypeFactory.Void));
            var body = method.CilMethodBody = new CilMethodBody();
            var instructions = body.Instructions;

            var start = new CilInstructionLabel();
            var tryStart = new CilInstructionLabel();
            var handlerStart = new CilInstructionLabel();

            start.Instruction = instructions.Add(CilOpCodes.Nop);
            tryStart.Instruction = instructions.Add(CilOpCodes.Nop);
            instructions.Add(CilOpCodes.Leave_S, start);
            handlerStart.Instruction = instructions.Add(CilOpCodes.Pop);
            instructions.Add(CilOpCodes.Leave_S, start);

            body.ExceptionHandlers.Add(new CilExceptionHandler
            {
                ExceptionType = module.CorLibTypeFactory.Object.ToTypeDefOrRef(),
                TryStart = tryStart,
                TryEnd = handlerStart,
                HandlerStart = handlerStart,
                HandlerEnd = instructions.EndLabel // Use end label.
            });

            instructions.CalculateOffsets();

            // Construct CFG.
            var cfg = body.ConstructStaticFlowGraph();
            
            // Verify handler exists.
            Assert.NotNull(cfg);
            Assert.Equal(3, cfg.Nodes.Count);
            var region = Assert.Single(cfg.Regions);
            var eh = Assert.IsAssignableFrom<ExceptionHandlerRegion<CilInstruction>>(region);
            Assert.Single(eh.Handlers);
        }
    }
}