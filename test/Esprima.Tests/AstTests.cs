using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Esprima.Ast;
using Esprima.Test;
using Esprima.Tests.Helpers;
using Esprima.Utils;

namespace Esprima.Tests
{
    public class AstTests
    {
        public static IEnumerable<object[]> OperatorTokenConversionsData => new[]
        {
            new object[]
            {
                typeof(AssignmentOperator),
                (Enum op) => AssignmentExpression.GetAssignmentOperatorToken((AssignmentOperator)op),
                (string op) => (Enum)AssignmentExpression.ParseAssignmentOperator(op),
            },
            new object[]
            {
                typeof(BinaryOperator),
                (Enum op) => BinaryExpression.GetBinaryOperatorToken((BinaryOperator)op),
                (string op) => (Enum)BinaryExpression.ParseBinaryOperator(op),
            },
            new object[]
            {
                typeof(UnaryOperator),
                (Enum op) => UnaryExpression.GetUnaryOperatorToken((UnaryOperator)op),
                (string op) => (Enum)UnaryExpression.ParseUnaryOperator(op),
            },
        };

        [Theory]
        [MemberData(nameof(OperatorTokenConversionsData))]
        public void OperatorTokenConversions(Type operatorEnumType, Func<Enum, string> getToken, Func<string, Enum> parseToken)
        {
            foreach (Enum enumValue in Enum.GetValues(operatorEnumType))
            {
                Assert.Equal(enumValue, parseToken(getToken(enumValue)));
            }
        }

        public sealed class ChildNodesVerifier : AstVisitor
        {
            private Node? _parentNode;

            public override object? Visit(Node node)
            {
                // Save visited child nodes into parent's Data.
                if (_parentNode is not null)
                {
                    var children = (List<Node>) (_parentNode.Data ??= new List<Node>());
                    children.Add(node);
                }

                var originalParentNode = _parentNode;
                _parentNode = node;

                var result = base.Visit(node);

                _parentNode = originalParentNode;

                // Verify that the list of visited children matches ChildNodes.
                Assert.True(node.ChildNodes.SequenceEqualUnordered((IEnumerable<Node>?) node.Data ?? Enumerable.Empty<Node>()));

                return result;
            }
        }

        [Fact]
        public void ChildNodesAndVisitorMustBeInSync()
        {
            var source = File.ReadAllText(Path.Combine(Fixtures.GetFixturesPath(), "Fixtures", "3rdparty", "bundle.js"));

            var parser = new JavaScriptParser(source);
            var script = parser.ParseScript();

            new ChildNodesVerifier().Visit(script);
        }

        private sealed class CustomNode : Node
        {
            public CustomNode(Node node1, Node node2) : base(Nodes.Extension)
            {
                Node1 = node1;
                Node2 = node2;
            }

            public Node Node1 { get; }
            public Node Node2 { get; }

            protected internal override object? Accept(AstVisitor visitor) => throw new NotSupportedException();

            protected internal override IEnumerator<Node>? GetChildNodes()
            {
                yield return Node1;
                yield return Node2;
            }
        }

        [Fact]
        public void ChildNodesCanBeImplementedByInheritors()
        {
            var id1 = new Identifier("a");
            var id2 = new Identifier("b");

            var customNode = new CustomNode(id1, id2);

            Assert.Equal(new[] { id1, id2 }, customNode.ChildNodes);
        }

        public static IEnumerable<object[]> ReusedNodeInstancesData => new[]
        {
            new object[]
            {
                "export { a }",
                (IEnumerable<Node> nodes) => nodes.OfType<Identifier>().Where(id => id.Name == "a")
            },
            new object[]
            {
                "import { b } from 'x'",
                (IEnumerable<Node> nodes) => nodes.OfType<Identifier>().Where(id => id.Name == "b")
            },
            new object[]
            {
                "({ c })",
                (IEnumerable<Node> nodes) => nodes.OfType<Identifier>().Where(id => id.Name == "c")
            },
            new object[]
            {
                "var { v } = { }",
                (IEnumerable<Node> nodes) => nodes.OfType<Identifier>().Where(id => id.Name == "v")
            },
            new object[]
            {
                "var { v = 0 } = { }",
                (IEnumerable<Node> nodes) => nodes.OfType<Identifier>().Where(id => id.Name == "v")
            },
        };

        [Theory]
        [MemberData(nameof(ReusedNodeInstancesData))]
        public void ReusedNodeInstancesEnumeratedOnlyOnce(string source, Func<IEnumerable<Node>, IEnumerable<Node>> reusedNodeSelector)
        {
            var parser = new JavaScriptParser(source);
            var module = parser.ParseModule();

            var nodes = module.DescendantNodes();

            Assert.Single(reusedNodeSelector(nodes));
        }

        public sealed class VisitedNodesCollector : AstVisitor
        {
            private readonly List<Node> _nodes = new List<Node>();

            public IReadOnlyList<Node> Collect(Node node)
            {
                _nodes.Clear();
                base.Visit(node);
                return _nodes;
            }

            public override object? Visit(Node node)
            {
                _nodes.Add(node);
                return base.Visit(node);
            }
        }

        [Theory]
        [MemberData(nameof(ReusedNodeInstancesData))]
        public void ReusedNodeInstancesVisitedOnlyOnce(string source, Func<IEnumerable<Node>, IEnumerable<Node>> reusedNodeSelector)
        {
            var parser = new JavaScriptParser(source);
            var module = parser.ParseModule();

            var nodes = new VisitedNodesCollector().Collect(module);

            Assert.Single(reusedNodeSelector(nodes));
        }

        public sealed class RewrittenNodesCollector : AstRewriter
        {
            private readonly List<Node> _nodes = new List<Node>();

            public IReadOnlyList<Node> Collect(Node node)
            {
                _nodes.Clear();
                base.Visit(node);
                return _nodes;
            }

            public override object? Visit(Node node)
            {
                _nodes.Add(node);
                return base.Visit(node);
            }
        }

        [Theory]
        [MemberData(nameof(ReusedNodeInstancesData))]
        public void ReusedNodeInstancesRewrittenOnlyOnce(string source, Func<IEnumerable<Node>, IEnumerable<Node>> reusedNodeSelector)
        {
            var parser = new JavaScriptParser(source);
            var module = parser.ParseModule();

            var nodes = new RewrittenNodesCollector().Collect(module);

            Assert.Single(reusedNodeSelector(nodes));
        }
    }
}
