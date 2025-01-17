using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Echo.Graphing.Serialization.Dot
{
    /// <summary>
    /// Provides a mechanism for writing graphs to a character stream using the dot file format.
    /// </summary>
    public class DotWriter
    {
        private static readonly IDictionary<char, string> EscapedCharacters = new Dictionary<char, string>
        {
            ['\r'] = "\\r",
            ['\n'] = "\\n",
            ['"'] = "\\\"",
            ['\t'] = "\\t",
        };

        /// <summary>
        /// Creates a new dot writer. 
        /// </summary>
        /// <param name="writer">The writer responsible for writing the output.</param>
        public DotWriter(TextWriter writer)
        {
            Writer = new System.CodeDom.Compiler.IndentedTextWriter(writer ?? throw new ArgumentNullException(nameof(writer)));
        }

        /// <summary>
        /// Gets the writer that is used to write textual data to the output stream.
        /// </summary>
        protected System.CodeDom.Compiler.IndentedTextWriter Writer
        {
            get;
        }

        /// <summary>
        /// Gets or sets a value indicating whether nodes in the output file should be explicitly defined before the
        /// edges are defined.
        /// </summary>
        public bool SeparateNodesAndEdges
        {
            get;
            set;
        } = true;

        /// <summary>
        /// Gets or sets a value indicating whether statements in the output file should be separated by semicolons.
        /// </summary>
        public bool IncludeSemicolons
        {
            get;
            set;
        } = true;

        /// <summary>
        /// Gets or sets the object responsible for assigning unique identifiers to nodes in a graph.
        /// </summary>
        public INodeIdentifier NodeIdentifier
        {
            get;
            set;
        } = new IncrementingNodeIdentifier();

        /// <summary>
        /// Gets or sets the adorner to use for adorning the nodes in the final output.
        /// </summary>
        /// <remarks>
        /// When this property is set to <c>null</c>, no adornments will be added.
        /// </remarks>
        public IDotNodeAdorner? NodeAdorner
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the adorner to use for adorning the edges in the final output.
        /// </summary>
        /// <remarks>
        /// When this property is set to <c>null</c>, no adornments will be added.
        /// </remarks>
        public IDotEdgeAdorner? EdgeAdorner
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the adorner to use for adorning the sub graphs in the final output.
        /// </summary>
        /// <remarks>
        /// When this property is set to <c>null</c>, no adornments will be added.
        /// </remarks>
        public IDotSubGraphAdorner? SubGraphAdorner
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating the resulting graph should be rendered as a directed graph or an
        /// undirected graph.
        /// </summary>
        public bool DirectedGraph
        {
            get;
            set;
        } = true;

        /// <summary>
        /// Writes a graph to the character stream.
        /// </summary>
        /// <param name="graph">The graph to write.</param>
        public void Write(IGraph graph)
        {
            WriteHeader(DirectedGraph ? "digraph" : "graph", null);
            Writer.Indent++;
            
            var freeNodes = new HashSet<INode>(graph.GetNodes());
            
            // Sub graphs.
            foreach (var subGraph in graph.GetSubGraphs())
                WriteSubGraph(subGraph, freeNodes);

            // Nodes
            foreach (var node in freeNodes)
            {
                if (SeparateNodesAndEdges
                    || !node.GetIncomingEdges().Any() && !node.GetOutgoingEdges().Any())
                {
                    WriteNode(node);
                }
            }

            // Edges
            foreach (var edge in graph.GetEdges())
                WriteEdge(edge);

            Writer.Indent--;
            WriteFooter();
        }

        private void WriteSubGraph(ISubGraph subGraph, HashSet<INode> scope)
        {
            if (SubGraphAdorner is null)
            {
                WriteHeader("subgraph", null);
                Writer.Indent++;
            }
            else
            {
                WriteHeader("subgraph", SubGraphAdorner.GetSubGraphName(subGraph));
                Writer.Indent++;
                
                var attributes = SubGraphAdorner.GetSubGraphAttributes(subGraph);
                if (attributes is {Count: > 0})
                {
                    string delimiter = IncludeSemicolons
                        ? ";"
                        : string.Empty;

                    WriteAttributes(attributes, delimiter, true);
                    Writer.WriteLine(delimiter);
                    Writer.WriteLine();
                }
            }

            foreach (var nested in subGraph.GetSubGraphs())
                WriteSubGraph(nested, scope);

            foreach (var node in subGraph.GetNodes())
            {
                if (scope.Remove(node))
                    WriteNode(node);
            }

            Writer.Indent--;
            WriteFooter();

            Writer.WriteLine();
        }

        /// <summary>
        /// Appends the header of a new graph to the output stream.
        /// </summary>
        protected virtual void WriteHeader(string graphType, string? graphName)
        {
            Writer.Write(graphType);
            if (!string.IsNullOrEmpty(graphName))
            {
                Writer.Write(' ');
                WriteIdentifier(graphName!);
            }
            
            Writer.WriteLine(" {");
        }

        /// <summary>
        /// Appends the footer of a graph to the output stream.
        /// </summary>
        private void WriteFooter()
        {
            Writer.WriteLine('}');
        }

        /// <summary>
        /// Appends a single node definition to the output stream.
        /// </summary>
        /// <param name="node">The node to append.</param>
        protected virtual void WriteNode(INode node)
        {
            long id = NodeIdentifier.GetIdentifier(node);
            WriteIdentifier(id.ToString());
            
            if (NodeAdorner is not null)
                WriteEntityAttributes(NodeAdorner.GetNodeAttributes(node, id));
            
            WriteSemicolon();
            Writer.WriteLine();
        }

        /// <summary>
        /// Appends an edge to the output stream.
        /// </summary>
        /// <param name="edge">The edge to append.</param>
        protected virtual void WriteEdge(IEdge edge)
        {
            long sourceId = NodeIdentifier.GetIdentifier(edge.Origin);
            long targetId = NodeIdentifier.GetIdentifier(edge.Target);
            
            WriteIdentifier(sourceId.ToString());
            Writer.Write(DirectedGraph ? " -> " : " -- ");
            WriteIdentifier(targetId.ToString());
            
            if (EdgeAdorner is not null)
                WriteEntityAttributes(EdgeAdorner.GetEdgeAttributes(edge, sourceId, targetId));
            
            WriteSemicolon();
            Writer.WriteLine();
        }

        private void WriteEntityAttributes(IDictionary<string, string>? attributes)
        {
            if (attributes is {Count: > 0})
            {
                Writer.Write(" [");
                WriteAttributes(attributes, ", ", false);
                Writer.Write(']');
            }
        }

        private void WriteAttributes(IDictionary<string, string>? attributes, string delimiter, bool newLines)
        {
            if (attributes is null)
                return;

            bool writtenOne = false;
            foreach (var item in attributes)
            {
                if (writtenOne)
                {
                    Writer.Write(delimiter);
                    if (newLines)
                        Writer.WriteLine();
                }

                WriteIdentifier(item.Key);
                Writer.Write('=');
                WriteIdentifier(item.Value);
                writtenOne = true;
            }
        }

        /// <summary>
        /// Appends a single identifier to the output stream.
        /// </summary>
        /// <param name="text">The identifier to write.</param>
        protected void WriteIdentifier(string text)
        {
            if (!NeedsEscaping(text))
            {
                Writer.Write(text);
            }
            else
            {
                Writer.Write('"');
                foreach (char c in text)
                    WriteEscapedCharacter(c);
                Writer.Write('"');
            }
        }
        
        /// <summary>
        /// Appends a semicolon to the output stream, depending on the value of <see cref="IncludeSemicolons"/>.
        /// </summary>
        protected void WriteSemicolon()
        {
            if (IncludeSemicolons)
                Writer.Write(';');
        }

        /// <summary>
        /// Determines whether an identifier requires escaping.
        /// </summary>
        /// <param name="text">The identifier to test.</param>
        /// <returns><c>True</c> if the identifier needs escaping, <c>false</c> otherwise.</returns>
        protected static bool NeedsEscaping(string text)
        {
            bool startsWithDigit = char.IsDigit(text[0]);
            foreach (char c in text)
            {
                if (EscapedCharacters.ContainsKey(c) || !char.IsLetterOrDigit(c) || startsWithDigit && !char.IsDigit(c))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Appends a single character to the output stream, and escapes it when necessary.
        /// </summary>
        /// <param name="c">The character to write.</param>
        protected void WriteEscapedCharacter(char c)
        {
            if (EscapedCharacters.TryGetValue(c, out string? escaped))
                Writer.Write(escaped);
            else
                Writer.Write(c);
        }
        
    }
}