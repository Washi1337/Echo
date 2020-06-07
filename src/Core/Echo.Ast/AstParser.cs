using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow;
using Echo.Core.Code;

namespace Echo.Ast
{
    /// <summary>
    /// Provides functionality to parse a given <see cref="ControlFlowGraph{TInstruction}"/> into expression form (AST)
    /// </summary>
    /// <typeparam name="TInstruction">The type of the instruction model</typeparam>
    public static class AstParser<TInstruction>
    {
        public static AbstractSyntaxTree ParseAst(ControlFlowGraph<TInstruction> controlFlowGraph)
        {
            var root = new AbstractSyntaxTree();
            var architecture = controlFlowGraph.Architecture;
            var entry = controlFlowGraph.Entrypoint;
            var placeholderVariables = IntroduceVariables(entry, architecture, new Stack<StackVariable>());

            var stack = new Stack<StackVariable>();
            foreach (var instruction in entry.Contents.Instructions)
            {
                var offset = architecture.GetOffset(instruction);
                var pops = architecture.GetStackPopCount(instruction);
                var pushes = architecture.GetStackPushCount(instruction);
                var readVariables = architecture.GetReadVariables(instruction);
                var writtenVariables = architecture.GetWrittenVariables(instruction);
                
                var arguments = new StackVariable[pops];
                for (var i = 0; i < pops; i++)
                    arguments[i] = stack.Pop();

                for (var i = 0; i < pushes; i++)
                {
                    stack.Push(placeholderVariables[offset + i]);
                }
            }
            
            return root;
        }

        private static IReadOnlyDictionary<long, StackVariable> IntroduceVariables(
            ControlFlowNode<TInstruction> controlFlowNode,
            IInstructionSetArchitecture<TInstruction> architecture,
            Stack<StackVariable> stack)
        {
            var mapping = new Dictionary<long, StackVariable>();
            var instructions = controlFlowNode.Contents.Instructions;

            foreach (var instruction in instructions)
            {
                var offset = architecture.GetOffset(instruction);
                var pops = architecture.GetStackPopCount(instruction);
                var pushes = architecture.GetStackPushCount(instruction);
                
                for (var i = 0; i < pops; i++)
                    stack.Pop();

                for (var i = 0; i < pushes; i++)
                {
                    var placeholder = new StackVariable($"stack_slot_{offset:X4}_{i}");
                    stack.Push(placeholder);
                    mapping[offset + i] = placeholder;
                }
            }
            
            return mapping;
        }
    }
}