using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Esprima.Utils;

namespace Esprima.Sample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            const string code = @"
""use strict"";
try { } catch (evil) { }

";
            var scanner = new Scanner(code);
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

        private static void Parse(string source, TextWriter output)
        {
            var parser = new JavaScriptParser();
            var program = parser.ParseScript(source);

            program.WriteJson(output);
            Console.WriteLine();
        }
    }
}
