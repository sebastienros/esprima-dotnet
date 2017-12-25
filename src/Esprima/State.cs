using System.Collections.Generic;

namespace Esprima
{
    public class Context
    {
        public int LastCommentStart { get; internal set; }
        public bool AllowIn { get; internal set; }
        public bool AllowYield { get; internal set; }
        public Token FirstCoverInitializedNameError { get; internal set; }
        public bool IsAssignmentTarget { get; internal set; }
        public bool IsBindingElement { get; internal set; }
        public bool InFunctionBody { get; internal set; }
        public bool InIteration { get; internal set; }
        public bool InSwitch { get; internal set; }
        public bool Strict { get; internal set; }
        public HashSet<string> LabelSet { get; internal set; }
        public Stack<int> MarkerStack { get; internal set; }
    }
}