using System;
using System.Linq;
using dnlib.DotNet;

namespace Echo.Platforms.Dnlib.Tests
{
    internal static class Helpers
    {
        public static MethodDef GetTestMethod(Type containingType, string name)
        {
            var assembly = AssemblyDef.Load(typeof(Helpers).Assembly.Location);
            var module = assembly.ManifestModule;
            var type = module.GetTypes().Single(t =>
                t.Name == containingType.Name && t.DeclaringType?.Name == containingType.DeclaringType?.Name);

            return type.FindMethod(name);
        }
    }
}