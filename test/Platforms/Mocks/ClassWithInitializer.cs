namespace Mocks;

public class ClassWithInitializer
{
    public static string Field = "Test";
    
    public static string MethodNoFieldAccess() => "MethodNoFieldAccess";

    public static string MethodFieldAccess() => "MethodFieldAccess: " + Field;
}