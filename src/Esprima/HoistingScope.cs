using Esprima.Ast;

namespace Esprima
{
    /// <summary>
    /// Used to save references to all function and variable declarations in a specific scope.
    /// </summary>
    public class HoistingScope
    {
        public List<FunctionDeclaration> FunctionDeclarations = new List<FunctionDeclaration>();
        public List<VariableDeclaration> VariableDeclarations = new List<VariableDeclaration>();
    }
}