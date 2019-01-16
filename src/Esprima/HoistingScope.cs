using System.Collections.Generic;
using Esprima.Ast;

namespace Esprima
{
    /// <summary>
    /// Used to save references to all function and variable declarations in a specific scope.
    /// </summary>
    public class HoistingScope
    {
        public Ast.List<FunctionDeclaration> FunctionDeclarations = new Ast.List<FunctionDeclaration>();
        public Ast.List<VariableDeclaration> VariableDeclarations = new Ast.List<VariableDeclaration>();
    }
}