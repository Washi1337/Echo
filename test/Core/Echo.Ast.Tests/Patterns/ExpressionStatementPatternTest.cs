using System;
using System.Collections.Generic;
using Echo.Ast.Patterns;
using Echo.Platforms.DummyPlatform.Code;
using Xunit;

namespace Echo.Ast.Tests.Patterns
{
    public class ExpressionStatementPatternTest
    {
        [Fact]
        public void TestComplexCapture()
        {
            var valueExpression = new InstructionExpression<DummyInstruction>(2, 
                DummyInstruction.Push(0, 1),
                ArraySegment<Expression<DummyInstruction>>.Empty);

            var statement = new ExpressionStatement<DummyInstruction>(0,
                new InstructionExpression<DummyInstruction>(1,
                    DummyInstruction.Ret(1), new List<Expression<DummyInstruction>>
                    {
                        valueExpression
                    }));
            
            // Define capture group.
            var returnValueGroup = new CaptureGroup("returnValue");
            
            // Create ret(?) pattern. 
            var pattern = StatementPattern.Expression(
                ExpressionPattern
                    .Instruction(new DummyInstructionPattern(DummyOpCode.Ret))
                    .WithArguments(
                        ExpressionPattern.Any<DummyInstruction>().CaptureAs(returnValueGroup)
                    )
            );

            // Match.
            var result = pattern.Match(statement);
            
            // Extract return expression node.
            var capturedObject = result.Captures[returnValueGroup][0];
            
            Assert.Same(valueExpression, capturedObject);
        }
    }
}