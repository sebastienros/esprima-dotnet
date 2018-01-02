using System.Collections.Generic;
using Esprima.Ast;

namespace Esprima
{
    /// <summary>
    /// Used to save references to all function and variable declarations in a specific scope.
    /// </summary>
    public class HoistingScope
    {
        public List<FunctionDeclaration> FunctionDeclarations { get; } = new List<FunctionDeclaration>();
        public List<VariableDeclaration> VariableDeclarations { get; } = new List<VariableDeclaration>();
    }
}