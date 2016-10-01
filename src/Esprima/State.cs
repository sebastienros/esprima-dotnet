using System.Collections.Generic;

namespace Esprima
{
    public class Context
    {
        public int LastCommentStart;
        public bool AllowIn;
        public bool AllowYield;
        public Token FirstCoverInitializedNameError;
        public bool IsAssignmentTarget;
        public bool IsBindingElement;
        public bool InFunctionBody;
        public bool InIteration;
        public bool InSwitch;
        public bool Strict;
        public HashSet<string> LabelSet;
        public Stack<int> MarkerStack;
    }
}
