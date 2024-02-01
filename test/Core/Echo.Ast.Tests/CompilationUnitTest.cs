using Echo.Platforms.DummyPlatform.Code;
using Xunit;

namespace Echo.Ast.Tests;

public class CompilationUnitTest
{
    [Fact]
    public void AddVariableExpressionShouldRegisterUse()
    {
        var variable = new DummyVariable("x");
        
        var unit = new CompilationUnit<DummyInstruction>();
        var expression = variable.ToExpression<DummyInstruction>();

        // Detached expressions should not register variable use.
        Assert.Empty(variable.GetIsUsedBy(unit));
        
        // Add to the tree.
        unit.Root.Statements.Add(expression.ToStatement());
        
        // Attached expressions should be registered.
        var use = Assert.Single(variable.GetIsUsedBy(unit));
        Assert.Same(expression, use);
    }
    
    [Fact]
    public void AddAssignmentShouldRegisterWrite()
    {
        var variable = new DummyVariable("x");
        
        var unit = new CompilationUnit<DummyInstruction>();
        var statement = Statement.Assignment(variable, Expression.Instruction(DummyInstruction.Op(0, 0, 1)));

        // Detached statements should not register variable use.
        Assert.Empty(variable.GetIsWrittenBy(unit));
        
        // Add to the tree.
        unit.Root.Statements.Add(statement);
        
        // Attached statements should be registered.
        var use = Assert.Single(variable.GetIsWrittenBy(unit));
        Assert.Same(statement, use);
    }
    
    [Fact]
    public void AddPhiShouldRegisterWrite()
    {
        var variable1 = new DummyVariable("x");
        var variable2 = new DummyVariable("y");
        
        var unit = new CompilationUnit<DummyInstruction>();
        var statement = Statement.Phi<DummyInstruction>(variable1, variable2);

        // Detached statements should not register variable use.
        Assert.Empty(variable1.GetIsWrittenBy(unit));
        
        // Add to the tree.
        unit.Root.Statements.Add(statement);
        
        // Attached statements should be registered.
        var use = Assert.Single(variable1.GetIsWrittenBy(unit));
        Assert.Same(statement, use);
    }
    
    [Fact]
    public void RemoveVariableExpressionShouldUnregisterUse()
    {
        var variable = new DummyVariable("x");
        
        var unit = new CompilationUnit<DummyInstruction>();
        var expression = variable.ToExpression<DummyInstruction>();
        unit.Root.Statements.Add(expression.ToStatement());

        // Remove the statement.
        unit.Root.Statements.Clear();
        
        // Detached expressions should deregister variable use.
        Assert.Empty(variable.GetIsUsedBy(unit));
    }
    
    [Fact]
    public void RemoveAssignmentShouldUnregisterWrite()
    {
        var variable = new DummyVariable("x");
        
        var unit = new CompilationUnit<DummyInstruction>();
        unit.Root.Statements.Add(Statement.Assignment(variable, Expression.Instruction(DummyInstruction.Op(0, 0, 1))));

        // Remove the statement.
        unit.Root.Statements.Clear();
        
        // Detached expressions should deregister variable use.
        Assert.Empty(variable.GetIsWrittenBy(unit));
    }
    
    [Fact]
    public void RemovePhiShouldUnregisterWrite()
    {
        var variable1 = new DummyVariable("x");
        var variable2 = new DummyVariable("y");
        
        var unit = new CompilationUnit<DummyInstruction>();
        unit.Root.Statements.Add(Statement.Phi<DummyInstruction>(variable1, variable2));

        // Remove the statement.
        unit.Root.Statements.Clear();
        
        // Detached expressions should deregister variable use.
        Assert.Empty(variable1.GetIsWrittenBy(unit));
    }
}