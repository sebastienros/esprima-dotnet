using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using Esprima.Ast;

namespace Esprima.Benchmark
{
    [MemoryDiagnoser]
    public class AstTreeWalkBenchmark
    {
        private Script _script;

        [GlobalSetup]
        public void Setup()
        {
            var code = File.ReadAllText("3rdparty/angular-1.7.9.js");
            var parser = new JavaScriptParser(code);
            _script = parser.ParseScript();
        }

        [Benchmark]
        public void VisitChildren()
        {
            var walker = new ScriptWalker();
            walker.Visit(_script);
            if (walker._lexicalNameCount != 0)
            {
                throw new InvalidOperationException("wrong _lexicalNameCount" + walker._lexicalNameCount);
            }

            if (walker._varNameCount != 1856)
            {
                throw new InvalidOperationException("wrong _varNameCount " + walker._varNameCount);
            }

            if (walker._functionCount != 1610)
            {
                throw new InvalidOperationException("wrong _functionCount " + walker._functionCount);
            }
        }

        private sealed class ScriptWalker
        {
            internal int _lexicalNameCount;
            internal int _varNameCount;
            internal int _functionCount;
            internal int _visitCount;

            public void Visit(Node node)
            {
                _visitCount++;
                foreach (var childNode in node.ChildNodes)
                {
                    if (childNode is null)
                    {
                        // array expression can push null nodes in Esprima
                        continue;
                    }

                    if (childNode is VariableDeclaration variableDeclaration)
                    {
                        if (variableDeclaration.Kind == VariableDeclarationKind.Var)
                        {
                            ref readonly var nodeList = ref variableDeclaration.Declarations;
                            foreach (var declaration in nodeList)
                            {
                                _varNameCount++;
                            }
                        }

                        if (variableDeclaration.Kind != VariableDeclarationKind.Var)
                        {
                            ref readonly var nodeList = ref variableDeclaration.Declarations;
                            foreach (var declaration in nodeList)
                            {
                                _lexicalNameCount++;
                            }
                        }
                    }
                    else
                    {
                        var type = childNode.Type;
                        if (type == Nodes.FunctionDeclaration
                            || type == Nodes.FunctionExpression
                            || type == Nodes.ArrowFunctionExpression
                            || type == Nodes.ArrowParameterPlaceHolder)
                        {
                            _functionCount++;
                        }
                    }

                    if (childNode.ChildNodes.Count > 0)
                    {
                        Visit(childNode);
                    }
                }
            }
        }
    }
}
