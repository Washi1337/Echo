using System;
using System.IO;
using System.Linq;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests
{
    public class CilPurityClassifierTest
    {
        private readonly CilPurityClassifier _classifier = new CilPurityClassifier();
        
        [Theory]
        [InlineData(CilCode.Ldc_I4_0, null)]
        [InlineData(CilCode.Ldc_R4, 1.0f)]
        [InlineData(CilCode.Ldc_I4, 123)]
        [InlineData(CilCode.Ldstr, "Hello, world!")]
        public void PushingConstantsShouldBePure(CilCode code, object? operand)
        {
            var instruction = new CilInstruction(code.ToOpCode(), operand);
            Assert.Equal(Trilean.True, _classifier.IsPure(instruction));
        }
        
        [Theory]
        [InlineData(CilCode.Add)]
        [InlineData(CilCode.Sub)]
        [InlineData(CilCode.Mul)]
        [InlineData(CilCode.Div)]
        [InlineData(CilCode.Shr)]
        [InlineData(CilCode.Shl)]
        public void ArithmeticShouldBePure(CilCode code)
        {
            var instruction = new CilInstruction(code.ToOpCode());
            Assert.Equal(Trilean.True, _classifier.IsPure(instruction));
        }

        [Theory]
        [InlineData(TrileanValue.False)]
        [InlineData(TrileanValue.True)]
        [InlineData(TrileanValue.Unknown)]
        public void NormalMethodCallShouldBeUserDefined(TrileanValue purity)
        {
            _classifier.DefaultMethodCallPurity = purity;
            
            var module = ModuleDefinition.FromFile(typeof(Math).Assembly.Location);
            var mathType = (TypeDefinition) module.LookupMember(typeof(Math).MetadataToken);
            var sinMethod = mathType.Methods.First(m => m.Name == nameof(Math.Sin));
            
            var instruction = new CilInstruction(CilOpCodes.Call, sinMethod);
            Assert.Equal(purity, _classifier.IsPure(instruction));
        }
        
        [Fact]
        public void WhitelistedMethodCallShouldBeConsideredPure()
        {
            var module = ModuleDefinition.FromFile(typeof(Math).Assembly.Location);
            var mathType = (TypeDefinition) module.LookupMember(typeof(Math).MetadataToken);
            var sinMethod = mathType.Methods.First(m => m.Name == nameof(Math.Sin));
            _classifier.KnownPureMethods.Add(sinMethod);
            
            var instruction = new CilInstruction(CilOpCodes.Call, sinMethod);
            Assert.Equal(Trilean.True, _classifier.IsPure(instruction));
        }
        
        [Fact]
        public void BlacklistedMethodCallShouldBeConsideredImpure()
        {
            var module = ModuleDefinition.FromFile(typeof(Math).Assembly.Location);
            var mathType = (TypeDefinition) module.LookupMember(typeof(Stream).MetadataToken);
            var sinMethod = mathType.Methods.First(m => m.Name == nameof(Stream.Write));
            _classifier.KnownImpureMethods.Add(sinMethod);
            
            var instruction = new CilInstruction(CilOpCodes.Call, sinMethod);
            Assert.Equal(Trilean.False, _classifier.IsPure(instruction));
        }
    }
}