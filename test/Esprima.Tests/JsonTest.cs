#if NET5_0_OR_GREATER
using System.Text.Json;
using Esprima.Ast;
using Esprima.Utils;
using Xunit;

namespace Esprima.Tests
{
    public class JsonTest
    {
        [Fact]
        public void JsonToAstParserTest()
        {
            var parser = new JavaScriptParser(@"if (true) { p(); }
                                                switch(foo) {
                                                    case 'A':
                                                        p();
                                                        break;
                                                }
                                                switch(foo) {
                                                    default:
                                                        p();
                                                        break;
                                                }
                                                for (var a = []; ; ) { }
                                                for (var elem of list) { }
                                                ");
            var program = parser.ParseScript();
            var json = AstJson.ToJsonString(program);

            var op = new JsonSerializerOptions();
            op.Converters.Add(new AstJsonConverter());
            var ast = JsonSerializer.Deserialize<Node>(json, op);
        }
    }
}
#endif
