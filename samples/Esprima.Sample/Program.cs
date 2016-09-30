using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Esprima.Sample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var scanner = new Scanner(@"
""use strict"";
try { } catch (evil) { }

");
            Tokenize(scanner);
            //Parse(scanner);
        }

        private static void Tokenize(Scanner scanner)
        {
            var tokens = new List<Token>();
            Token token;

            do
            {
                scanner.ScanComments();
                token = scanner.Lex();
                tokens.Add(token);
            } while (token.Type != TokenType.EOF);

            Console.WriteLine(JsonConvert.SerializeObject(tokens, Formatting.Indented));
        }

        private static void Parse(string source)
        {
            var parser = new JavaScriptParser(source);
            var program = parser.ParseProgram();

            Console.WriteLine(JsonConvert.SerializeObject(program, Formatting.Indented));
        }
    }
}
