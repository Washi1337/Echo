using System;
using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow.Construction;
using Echo.ControlFlow.Regions.Detection;
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
            var offsetMap = cfg.Nodes.CreateOffsetMap();
            
            Assert.Same(cfg, offsetMap[0]!.ParentRegion);
            Assert.Same(cfg, offsetMap[5]!.ParentRegion);

            var ehRegion = offsetMap[1]!.GetParentExceptionHandler();
            Assert.NotNull(ehRegion);
            
            Assert.Same(ehRegion.ProtectedRegion, offsetMap[1].ParentRegion); 
            Assert.Contains(offsetMap[3].GetParentHandler(), ehRegion.Handlers); 
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
            var offsetMap = cfg.Nodes.CreateOffsetMap();
            
            var ehRegion1 = offsetMap[1].GetParentExceptionHandler()!;
            var ehRegion2 = offsetMap[6].GetParentExceptionHandler()!;
            
            Assert.NotSame(ehRegion1, ehRegion2);
            Assert.Same(ehRegion1.ProtectedRegion, offsetMap[1].ParentRegion); 
            Assert.Contains(offsetMap[3].GetParentHandler(), ehRegion1.Handlers); 
            Assert.Same(ehRegion1.ProtectedRegion, offsetMap[1].ParentRegion); 
            Assert.Contains(offsetMap[3].GetParentHandler(), ehRegion1.Handlers); 
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
            var offsetMap = cfg.Nodes.CreateOffsetMap();

            var ehRegion = offsetMap[1].GetParentExceptionHandler();
            
            Assert.Same(ehRegion, offsetMap[3].GetParentExceptionHandler());
            Assert.Same(ehRegion, offsetMap[5].GetParentExceptionHandler());
            Assert.NotSame(offsetMap[3].ParentRegion, offsetMap[5].ParentRegion);
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
            var offsetMap = cfg.Nodes.CreateOffsetMap();

            var ehRegion1 = offsetMap[1].GetParentExceptionHandler();
            var ehRegion2 = offsetMap[2].GetParentExceptionHandler();
            
            Assert.NotSame(ehRegion1, ehRegion2);
            Assert.Null(offsetMap[0].GetParentExceptionHandler());
            Assert.Same(ehRegion1, offsetMap[1].GetParentExceptionHandler());
            Assert.Same(ehRegion2, offsetMap[2].GetParentExceptionHandler());
            Assert.Same(ehRegion2, offsetMap[4].GetParentExceptionHandler());
            Assert.Same(ehRegion1, offsetMap[6].GetParentExceptionHandler());
            Assert.Same(ehRegion1, offsetMap[7].GetParentExceptionHandler());
            Assert.Null(offsetMap[9].GetParentExceptionHandler());
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
            var offsetMap = cfg.Nodes.CreateOffsetMap();

            var ehRegion1 = offsetMap[1].GetParentExceptionHandler();
            var ehRegion2 = offsetMap[4].GetParentExceptionHandler();
            
            Assert.NotSame(ehRegion1, ehRegion2);
            Assert.Null(offsetMap[0].GetParentExceptionHandler());
            Assert.Same(ehRegion1, offsetMap[1].GetParentExceptionHandler());
            Assert.Same(ehRegion1, offsetMap[3].GetParentExceptionHandler());
            Assert.Same(ehRegion2, offsetMap[4].GetParentExceptionHandler());
            Assert.Same(ehRegion2, offsetMap[6].GetParentExceptionHandler());
            Assert.Same(ehRegion1, offsetMap[8].GetParentExceptionHandler());
            Assert.Null(offsetMap[9].GetParentExceptionHandler());
        }

        [Fact]
        public void ExceptionHandlerWithPrologueAndEpilogue()
        {
            var ranges = new[]
            {
                new ExceptionHandlerRange(
                    new AddressRange(1, 3),
                    new AddressRange(3, 5),
                    new AddressRange(5, 7),
                    new AddressRange(7, 9))
            };
            
            var instructions = new[]
            {
                DummyInstruction.Op(0, 0, 0),
                
                // try start
                DummyInstruction.Op(1, 0, 0),
                DummyInstruction.Jmp(2, 9),
                
                // handler prologue start
                DummyInstruction.Op(3, 0, 0),
                DummyInstruction.Ret(4),
                
                // handler start
                DummyInstruction.Op(5, 0, 0),
                DummyInstruction.Jmp(6, 9),
                
                // handler epilogue start
                DummyInstruction.Op(7, 0, 0),
                DummyInstruction.Ret(8),
                
                DummyInstruction.Ret(9),
            };

            var cfg = ConstructGraphWithEHRegions(instructions, ranges);
            var offsetMap = cfg.Nodes.CreateOffsetMap();

            var ehRegion = offsetMap[1].GetParentExceptionHandler()!;
            var handlerRegion = Assert.Single(ehRegion.Handlers);
            Assert.NotNull(handlerRegion);
            Assert.NotNull(handlerRegion.Prologue);
            Assert.NotNull(handlerRegion.Epilogue);

            Assert.Same(cfg, offsetMap[0].ParentRegion);
            Assert.Same(ehRegion.ProtectedRegion, offsetMap[1].ParentRegion);
            Assert.Same(handlerRegion.Prologue, offsetMap[3].GetParentHandler()!.Prologue);
            Assert.Same(handlerRegion.Contents, offsetMap[5].GetParentHandler()!.Contents);
            Assert.Same(handlerRegion.Epilogue, offsetMap[7].GetParentHandler()!.Epilogue);
            Assert.Same(cfg, offsetMap[9].ParentRegion);
        }

    }
}