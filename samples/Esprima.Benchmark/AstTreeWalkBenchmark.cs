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
            var parser = new ScriptWalker();
            parser.Visit(_script);
        }

        private sealed class ScriptWalker
        {
            private int _lexicalNameCount;
            private int _varNameCount;
            private int _functionCount;

            public void Visit(Node node)
            {
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

                    Visit(childNode);
                }
            }
        }
    }
}