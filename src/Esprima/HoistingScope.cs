using Esprima.Ast;

namespace Esprima
{
    /// <summary>
    /// Used to save references to all function and variable declarations in a specific scope.
    /// </summary>
    public class HoistingScope
    {
        public NodeList<FunctionDeclaration> FunctionDeclarations { get; }
        public NodeList<VariableDeclaration> VariableDeclarations { get; }

        public HoistingScope() :
            this(default, default) {}

        public HoistingScope(NodeList<FunctionDeclaration> functionDeclarations,
                             NodeList<VariableDeclaration> variableDeclarations)
        {
            FunctionDeclarations = functionDeclarations;
            VariableDeclarations = variableDeclarations;
        }
    }
}