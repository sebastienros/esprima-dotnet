using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Esprima;

public enum JsxTokenType
{
    JsxIdentifier,
    JsxText
};

public sealed class JsxToken : Token
{
    public JsxToken()
    {
        base.Type = TokenType.Extension;
    }

    public new JsxTokenType Type;
}
