using System.Collections.Generic;
using Esprima.Ast;

namespace Esprima
{
    /// <summary>
    /// Used to save references to all function and variable declarations in a specific scope.
    /// </summary>
    public class HoistingScope
    {
        public IList<FunctionDeclaration> FunctionDeclarations { get; } = new List<FunctionDeclaration>();
        public IList<VariableDeclaration> VariableDeclarations { get; } = new List<VariableDeclaration>();
    }
}