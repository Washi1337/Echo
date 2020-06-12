using System.Collections.Generic;
using Echo.ControlFlow;
using Echo.Core.Code;
using Echo.DataFlow;

namespace Echo.Ast
{
    /// <summary>
    /// Provides functionality to parse a given <see cref="ControlFlowGraph{TInstruction}"/>
    /// and <see cref="DataFlowGraph{TInstruction}"/> into expression form (AST)
    /// </summary>
    /// <typeparam name="TInstruction">The type of the instruction model</typeparam>
    public static class AstParser<TInstruction>
    {
        /// <summary>
        /// Parses a given <see cref="ControlFlowGraph{TInstruction}"/> into an <see cref="AbstractSyntaxTree"/>
        /// </summary>
        /// <param name="controlFlowGraph">The <see cref="ControlFlowGraph{TInstruction}"/> to parse</param>
        public static AbstractSyntaxTree ParseAst(ControlFlowGraph<TInstruction> controlFlowGraph)
        {
            return new AbstractSyntaxTree(0);
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
                long offset = architecture.GetOffset(instruction);
                int pops = architecture.GetStackPopCount(instruction);
                int pushes = architecture.GetStackPushCount(instruction);
                
                for (int i = 0; i < pops; i++)
                    stack.Pop();

                for (int i = 0; i < pushes; i++)
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
