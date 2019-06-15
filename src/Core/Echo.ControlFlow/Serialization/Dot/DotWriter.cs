using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Echo.ControlFlow.Serialization.Dot
{
    /// <summary>
    /// Provides a mechanism for writing graphs to a character stream using the dot file format.
    /// </summary>
    public class DotWriter
    {
        public static readonly IDictionary<char, string> EscapedCharacters = new Dictionary<char, string>
        {
            ['\r'] = "\\\r",
            ['\n'] = "\\\n",
            ['"'] = "\\\"",
            ['\t'] = "\\\t",
        };

        protected TextWriter Writer
        {
            get;
        }

        /// <summary>
        /// Creates a new dot writer. 
        /// </summary>
        /// <param name="writer">The writer responsible for writing the output.</param>
        public DotWriter(TextWriter writer)
        {
            Writer = writer ?? throw new ArgumentNullException(nameof(writer));
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
        /// Writes a graph to the character stream.
        /// </summary>
        /// <param name="graph">The graph to write.</param>
        public void Write(IGraph graph)
        {
            WriteHeader();
            
            // Nodes
            var nodeIdentifiers = new Dictionary<INode, string>();
            foreach (var node in graph.GetNodes())
            {
                string identifier = nodeIdentifiers.Count.ToString();
                nodeIdentifiers[node] = identifier;
                
                if (SeparateNodesAndEdges
                    || !node.GetIncomingEdges().Any() && !node.GetOutgoingEdges().Any())
                {
                    Write(node, identifier);
                }
            }

            // Edges
            foreach (var edge in graph.GetEdges())
                Write(nodeIdentifiers, edge);

            WriteFooter();
        }

        protected virtual void WriteHeader()
        {
            Writer.WriteLine("strict digraph {");
        }

        private void WriteFooter()
        {
            Writer.WriteLine("}");
        }

        protected virtual void Write(INode node, string identifier)
        {
            WriteIdentifier(identifier);
            WriteSemicolon();
            Writer.WriteLine();
        }

        protected virtual void Write(IDictionary<INode, string> nodeIdentifiers, IEdge edge)
        {
            WriteIdentifier(nodeIdentifiers[edge.Source]);
            Writer.Write(" -> ");
            WriteIdentifier(nodeIdentifiers[edge.Target]);
            
            WriteSemicolon();
            
            Writer.WriteLine();
        }

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
        
        protected void WriteSemicolon()
        {
            if (IncludeSemicolons)
                Writer.Write(';');
        }

        protected static bool NeedsEscaping(string text)
        {
            return text.ToCharArray().Any(c => EscapedCharacters.ContainsKey(c) || !char.IsLetterOrDigit(c));
        }

        protected void WriteEscapedCharacter(char c)
        {
            if (EscapedCharacters.TryGetValue(c, out string escaped))
                Writer.Write(escaped);
            else
                Writer.Write(c);
        }
        
    }
}