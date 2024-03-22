using System;

namespace Mocks;

public class ClassWithThrowingInitializer
{
    public static string Field = "Test";
    
    static ClassWithThrowingInitializer()
    {
        throw new Exception();
    }
    
    public static string MethodNoFieldAccess() => "MethodNoFieldAccess";

    public static string MethodFieldAccess() => "MethodFieldAccess: " + Field;
}