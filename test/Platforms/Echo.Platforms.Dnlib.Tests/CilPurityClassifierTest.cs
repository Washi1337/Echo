using System;
using System.IO;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Echo.Core;
using Xunit;

namespace Echo.Platforms.Dnlib.Tests 
{
    public class CilPurityClassifierTest 
    {
        private readonly CilPurityClassifier _classifier = new CilPurityClassifier();

        [Theory]
        [InlineData(Code.Ldc_I4_0, null)]
        [InlineData(Code.Ldc_R4, 1.0f)]
        [InlineData(Code.Ldc_I4, 123)]
        [InlineData(Code.Ldstr, "Hello, world!")]
        public void PushingConstantsShouldBePure(Code code, object operand) 
        {
            var instruction = new Instruction(code.ToOpCode(), operand);
            Assert.Equal(Trilean.True, _classifier.IsPure(instruction));
        }

        [Theory]
        [InlineData(Code.Add)]
        [InlineData(Code.Sub)]
        [InlineData(Code.Mul)]
        [InlineData(Code.Div)]
        [InlineData(Code.Shr)]
        [InlineData(Code.Shl)]
        public void ArithmeticShouldBePure(Code code) 
        {
            var instruction = new Instruction(code.ToOpCode());
            Assert.Equal(Trilean.True, _classifier.IsPure(instruction));
        }

        [Theory]
        [InlineData(TrileanValue.False)]
        [InlineData(TrileanValue.True)]
        [InlineData(TrileanValue.Unknown)]
        public void NormalMethodCallShouldBeUserDefined(TrileanValue purity) 
        {
            _classifier.DefaultMethodCallPurity = purity;

            var module = ModuleDefMD.Load(typeof(Math).Assembly.Location);
            var mathType = (TypeDef)module.ResolveToken(typeof(Math).MetadataToken);
            var sinMethod = mathType.Methods.First(m => m.Name == nameof(Math.Sin));

            var instruction = new Instruction(OpCodes.Call, sinMethod);
            Assert.Equal(purity, _classifier.IsPure(instruction));
        }

        [Fact]
        public void WhitelistedMethodCallShouldBeConsideredPure() 
        {
            var module = ModuleDefMD.Load(typeof(Math).Assembly.Location);
            var mathType = (TypeDef)module.ResolveToken(typeof(Math).MetadataToken);
            var sinMethod = mathType.Methods.First(m => m.Name == nameof(Math.Sin));
            _classifier.KnownPureMethods.Add(sinMethod);

            var instruction = new Instruction(OpCodes.Call, sinMethod);
            Assert.Equal(Trilean.True, _classifier.IsPure(instruction));
        }

        [Fact]
        public void BlacklistedMethodCallShouldBeConsideredImpure() 
        {
            var module = ModuleDefMD.Load(typeof(Math).Assembly.Location);
            var mathType = (TypeDef)module.ResolveToken(typeof(Stream).MetadataToken);
            var sinMethod = mathType.Methods.First(m => m.Name == nameof(Stream.Write));
            _classifier.KnownImpureMethods.Add(sinMethod);

            var instruction = new Instruction(OpCodes.Call, sinMethod);
            Assert.Equal(Trilean.False, _classifier.IsPure(instruction));
        }
    }
}
