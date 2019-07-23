using Esprima.Ast;

namespace Esprima
{
    /// <summary>
    /// Used to save references to all function and variable declarations in a specific scope.
    /// </summary>
    public class HoistingScope
    {
        private NodeList<FunctionDeclaration> _functionDeclarations;
        private NodeList<VariableDeclaration> _variableDeclarations;

        public HoistingScope() :
            this(default, default) {}

        public HoistingScope(in NodeList<FunctionDeclaration> functionDeclarations,
                             in NodeList<VariableDeclaration> variableDeclarations)
        {
            _functionDeclarations = functionDeclarations;
            _variableDeclarations = variableDeclarations;
        }

        public ref readonly NodeList<FunctionDeclaration> FunctionDeclarations => ref _functionDeclarations;
        public ref readonly NodeList<VariableDeclaration> VariableDeclarations => ref _variableDeclarations;

    }
}