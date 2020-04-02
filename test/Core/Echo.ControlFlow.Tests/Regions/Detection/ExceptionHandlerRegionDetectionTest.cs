using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Echo.ControlFlow.Construction.Static;
using Echo.ControlFlow.Regions;
using Echo.ControlFlow.Regions.Detection;
using Echo.Core.Code;
using Echo.Platforms.DummyPlatform.Code;
using Xunit;

namespace Echo.ControlFlow.Tests.Regions.Detection
{
    public class ExceptionHandlerRegionDetectionTest
    {
        private static ControlFlowGraph<DummyInstruction> ConstructGraphWithEHRegions(IEnumerable<DummyInstruction> instructions, IEnumerable<ExceptionHandlerRange> ranges)
        {
            var architecture = DummyArchitecture.Instance;
            var builder = new StaticFlowGraphBuilder<DummyInstruction>(
                architecture,
                architecture.SuccessorResolver);

            var knownBlockHeaders = ranges.SelectMany(r => new[]
            {
                r.ProtectedRange.Start, r.HandlerRange.Start
            });
            
            var cfg = builder.ConstructFlowGraph(instructions, 0, knownBlockHeaders);
            cfg.DetectExceptionHandlerRegions(ranges);
            
            return cfg;
        }

        [Fact]
        public void DetectSingleEHByRange()
        {
            var ranges = new[]
            {
                new ExceptionHandlerRange(new AddressRange(1, 3), new AddressRange(3, 5)),
            };

            var instructions = new[]
            {
                DummyInstruction.Op(0, 0, 0),
                
                // trystart
                DummyInstruction.Op(1, 0, 0),
                DummyInstruction.Jmp(2, 5),
                
                // handlerstart
                DummyInstruction.Op(3, 0, 0),
                DummyInstruction.Jmp(4, 5),
                
                DummyInstruction.Ret(5),
            };

            var cfg = ConstructGraphWithEHRegions(instructions, ranges);
            
            Assert.Same(cfg, cfg.Nodes[0].ParentRegion);
            Assert.Same(cfg, cfg.Nodes[5].ParentRegion);

            var ehRegion = cfg.Nodes[1].GetParentExceptionHandler();
            Assert.NotNull(ehRegion);
            
            Assert.Same(ehRegion.ProtectedRegion, cfg.Nodes[1].ParentRegion); 
            Assert.Same(ehRegion.HandlerRegion, cfg.Nodes[3].ParentRegion); 
        }

        [Fact]
        public void DetectSequentialEHsByRanges()
        {
            var ranges = new[]
            {
                new ExceptionHandlerRange(new AddressRange(1, 3), new AddressRange(3, 5)),
                new ExceptionHandlerRange(new AddressRange(6, 8), new AddressRange(8, 10)),
            };

            var instructions = new[]
            {
                DummyInstruction.Op(0, 0, 0),
                
                // trystart
                DummyInstruction.Op(1, 0, 0),
                DummyInstruction.Jmp(2, 5),
                
                // handlerstart
                DummyInstruction.Op(3, 0, 0),
                DummyInstruction.Jmp(4, 5),
                
                DummyInstruction.Op(5, 0, 0),
                
                // try start
                DummyInstruction.Op(6, 0, 0),
                DummyInstruction.Jmp(7, 5),
                
                // handler start
                DummyInstruction.Op(8, 0, 0),
                DummyInstruction.Jmp(9, 5),
                
                DummyInstruction.Ret(10),
            };

            var cfg = ConstructGraphWithEHRegions(instructions, ranges);
            
            var ehRegion1 = cfg.Nodes[1].GetParentExceptionHandler();
            var ehRegion2 = cfg.Nodes[6].GetParentExceptionHandler();
            
            Assert.NotSame(ehRegion1, ehRegion2);
            Assert.Same(ehRegion1.ProtectedRegion, cfg.Nodes[1].ParentRegion); 
            Assert.Same(ehRegion1.HandlerRegion, cfg.Nodes[3].ParentRegion); 
            Assert.Same(ehRegion1.ProtectedRegion, cfg.Nodes[1].ParentRegion); 
            Assert.Same(ehRegion1.HandlerRegion, cfg.Nodes[3].ParentRegion); 
        }

        [Fact]
        public void DetectEHWithMultipleHandlers()
        {
            var ranges = new[]
            {
                new ExceptionHandlerRange(new AddressRange(1, 3), new AddressRange(3, 5)),
                new ExceptionHandlerRange(new AddressRange(1, 3), new AddressRange(5, 7)),
            };

            var instructions = new[]
            {
                DummyInstruction.Op(0, 0, 0),
                
                // trystart (x2)
                DummyInstruction.Op(1, 0, 0),
                DummyInstruction.Jmp(2, 7),
                
                // handlerstart
                DummyInstruction.Op(3, 0, 0),
                DummyInstruction.Jmp(4, 7),
                
                // handler start
                DummyInstruction.Op(5, 0, 0),
                DummyInstruction.Jmp(6, 7),
                
                DummyInstruction.Ret(7),
            };

            var cfg = ConstructGraphWithEHRegions(instructions, ranges);

            var ehRegion1 = cfg.Nodes[1].GetParentExceptionHandler();
            var ehRegion2 = cfg.Nodes[5].GetParentExceptionHandler();
            
            Assert.NotSame(ehRegion1, ehRegion2);
            Assert.Same(ehRegion2, ehRegion1.GetParentExceptionHandler());
            
            Assert.Same(ehRegion1.ProtectedRegion, cfg.Nodes[1].ParentRegion); 
            Assert.Same(ehRegion1.HandlerRegion, cfg.Nodes[3].ParentRegion);  
            Assert.Same(ehRegion2.HandlerRegion, cfg.Nodes[5].ParentRegion); 
        }

    }
}