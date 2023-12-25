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
            // Define test "ret(push(0, 1))" expression/
            var expression = new InstructionExpression<DummyInstruction>(DummyInstruction.Push(0, 1));
            var statement = new ExpressionStatement<DummyInstruction>(
                new InstructionExpression<DummyInstruction>(DummyInstruction.Ret(1), expression)
            );
            
            // Define capture group.
            var returnValueGroup = new CaptureGroup<Expression<DummyInstruction>>("returnValue");
            
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
            Assert.Same(expression, Assert.Single(result.GetCaptures(returnValueGroup)));
        }
    }
}