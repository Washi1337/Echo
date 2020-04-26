using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Echo.ControlFlow.Construction;
using Echo.ControlFlow.Construction.Static;
using Echo.ControlFlow.Regions.Detection;
using Echo.ControlFlow.Serialization.Dot;
using Echo.Core.Code;
using Echo.Core.Graphing.Serialization.Dot;
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
                instructions,
                architecture.SuccessorResolver);

            var rangesArray = ranges as ExceptionHandlerRange[] ?? ranges.ToArray();
            var cfg = builder.ConstructFlowGraph(0, rangesArray);
            cfg.DetectExceptionHandlerRegions(rangesArray);
            
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
                
                // try start
                DummyInstruction.Op(1, 0, 0),
                DummyInstruction.Jmp(2, 5),
                
                // handler start
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
            Assert.Contains(cfg.Nodes[3].ParentRegion, ehRegion.HandlerRegions); 
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void DetectSequentialEHsByRanges(bool reverse)
        {
            var ranges = new[]
            {
                new ExceptionHandlerRange(new AddressRange(1, 3), new AddressRange(3, 5)),
                new ExceptionHandlerRange(new AddressRange(6, 8), new AddressRange(8, 10)),
            };
            
            if (reverse)
                Array.Reverse(ranges);

            var instructions = new[]
            {
                DummyInstruction.Op(0, 0, 0),
                
                // try start 1
                DummyInstruction.Op(1, 0, 0),
                DummyInstruction.Jmp(2, 5),
                
                // handler start 1
                DummyInstruction.Op(3, 0, 0),
                DummyInstruction.Jmp(4, 5),
                
                DummyInstruction.Op(5, 0, 0),
                
                // try start 2
                DummyInstruction.Op(6, 0, 0),
                DummyInstruction.Jmp(7, 10),
                
                // handler start 2
                DummyInstruction.Op(8, 0, 0),
                DummyInstruction.Jmp(9, 10),
                
                DummyInstruction.Ret(10),
            };

            var cfg = ConstructGraphWithEHRegions(instructions, ranges);
            
            var ehRegion1 = cfg.Nodes[1].GetParentExceptionHandler();
            var ehRegion2 = cfg.Nodes[6].GetParentExceptionHandler();
            
            Assert.NotSame(ehRegion1, ehRegion2);
            Assert.Same(ehRegion1.ProtectedRegion, cfg.Nodes[1].ParentRegion); 
            Assert.Contains(cfg.Nodes[3].ParentRegion, ehRegion1.HandlerRegions); 
            Assert.Same(ehRegion1.ProtectedRegion, cfg.Nodes[1].ParentRegion); 
            Assert.Contains(cfg.Nodes[3].ParentRegion, ehRegion1.HandlerRegions); 
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void EHWithMultipleHandlersByRangesShouldGroupTogether(bool reverse)
        {
            var ranges = new[]
            {
                new ExceptionHandlerRange(new AddressRange(1, 3), new AddressRange(3, 5)),
                new ExceptionHandlerRange(new AddressRange(1, 3), new AddressRange(5, 7)),
            };
            
            if (reverse)
                Array.Reverse(ranges);

            var instructions = new[]
            {
                DummyInstruction.Op(0, 0, 0),
                
                // try start 1 & 2
                DummyInstruction.Op(1, 0, 0),
                DummyInstruction.Jmp(2, 7),
                
                // handler start 2
                DummyInstruction.Op(3, 0, 0),
                DummyInstruction.Jmp(4, 7),
                
                // handler start 1
                DummyInstruction.Op(5, 0, 0),
                DummyInstruction.Jmp(6, 7),
                
                DummyInstruction.Ret(7),
            };

            var cfg = ConstructGraphWithEHRegions(instructions, ranges);

            var ehRegion = cfg.Nodes[1].GetParentExceptionHandler();
            
            Assert.Same(ehRegion, cfg.Nodes[3].GetParentExceptionHandler());
            Assert.Same(ehRegion, cfg.Nodes[5].GetParentExceptionHandler());
            Assert.NotSame(cfg.Nodes[3].ParentRegion, cfg.Nodes[5].ParentRegion);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void DetectNestedEHInProtectedRegionByRanges(bool reverse)
        {
            var ranges = new[]
            {
                new ExceptionHandlerRange(new AddressRange(2, 4), new AddressRange(4, 6)),
                new ExceptionHandlerRange(new AddressRange(1, 7), new AddressRange(7, 9)),
            };

            if (reverse)
                Array.Reverse(ranges);
            
            var instructions = new[]
            {
                DummyInstruction.Op(0, 0, 0),
                
                // try start 1
                DummyInstruction.Op(1, 0, 0),
                
                // try start 2
                DummyInstruction.Op(2, 0, 0),
                DummyInstruction.Jmp(3, 6),
                
                // handler start 2
                DummyInstruction.Op(4, 0, 0),
                DummyInstruction.Jmp(5, 6),
                
                DummyInstruction.Jmp(6, 9),
                
                // handler start 1
                DummyInstruction.Op(7, 0, 0),
                DummyInstruction.Jmp(8, 9),
                
                DummyInstruction.Ret(9),
            };

            var cfg = ConstructGraphWithEHRegions(instructions, ranges);

            var ehRegion1 = cfg.Nodes[1].GetParentExceptionHandler();
            var ehRegion2 = cfg.Nodes[2].GetParentExceptionHandler();
            
            Assert.NotSame(ehRegion1, ehRegion2);
            Assert.Null(cfg.Nodes[0].GetParentExceptionHandler());
            Assert.Same(ehRegion1, cfg.Nodes[1].GetParentExceptionHandler());
            Assert.Same(ehRegion2, cfg.Nodes[2].GetParentExceptionHandler());
            Assert.Same(ehRegion2, cfg.Nodes[4].GetParentExceptionHandler());
            Assert.Same(ehRegion1, cfg.Nodes[6].GetParentExceptionHandler());
            Assert.Same(ehRegion1, cfg.Nodes[7].GetParentExceptionHandler());
            Assert.Null(cfg.Nodes[9].GetParentExceptionHandler());
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void DetectNestedEHInHandlerRegionByRanges(bool reverse)
        {
            var ranges = new[]
            {
                new ExceptionHandlerRange(new AddressRange(1, 3), new AddressRange(3, 9)),
                new ExceptionHandlerRange(new AddressRange(4, 6), new AddressRange(6, 8)),
            };
            
            if (reverse)
                Array.Reverse(ranges);

            var instructions = new[]
            {
                DummyInstruction.Op(0, 0, 0),
                
                // try start 1
                DummyInstruction.Op(1, 0, 0),
                DummyInstruction.Jmp(2, 9),
                
                // handler start 1
                DummyInstruction.Op(3, 0, 0),
                
                // try start 2
                DummyInstruction.Op(4, 0, 0),
                DummyInstruction.Jmp(5, 8),
                
                // handler start 2
                DummyInstruction.Op(6, 0, 0),
                DummyInstruction.Jmp(7, 8),
                
                DummyInstruction.Jmp(8, 9),
                
                DummyInstruction.Ret(9),
            };

            var cfg = ConstructGraphWithEHRegions(instructions, ranges);

            var ehRegion1 = cfg.Nodes[1].GetParentExceptionHandler();
            var ehRegion2 = cfg.Nodes[4].GetParentExceptionHandler();
            
            Assert.NotSame(ehRegion1, ehRegion2);
            Assert.Null(cfg.Nodes[0].GetParentExceptionHandler());
            Assert.Same(ehRegion1, cfg.Nodes[1].GetParentExceptionHandler());
            Assert.Same(ehRegion1, cfg.Nodes[3].GetParentExceptionHandler());
            Assert.Same(ehRegion2, cfg.Nodes[4].GetParentExceptionHandler());
            Assert.Same(ehRegion2, cfg.Nodes[6].GetParentExceptionHandler());
            Assert.Same(ehRegion1, cfg.Nodes[8].GetParentExceptionHandler());
            Assert.Null(cfg.Nodes[9].GetParentExceptionHandler());
        }

    }
}