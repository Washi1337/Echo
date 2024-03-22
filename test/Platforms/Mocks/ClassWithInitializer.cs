namespace Mocks;

public class ClassWithInitializer
{
    public static string Field = "Test";
    public static int Counter = 1337;
    
    public static string MethodNoFieldAccess() => "MethodNoFieldAccess";

    public static string MethodFieldAccess() => "MethodFieldAccess: " + Field;

    public static void IncrementCounter() => Counter++;
}