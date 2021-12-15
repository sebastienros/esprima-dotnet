using System;
using System.Collections.Generic;
using System.IO;
using Esprima.Utils;
using Newtonsoft.Json;

namespace Esprima.Sample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string source = @"
""use strict"";
let t = 5;

";
            var scanner = new Scanner(source);
            Tokenize(scanner);
            Parse(source, Console.Out);
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
            var parser = new JavaScriptParser(source);
            var program = parser.ParseScript();

            program.WriteJson(output);
            Console.WriteLine();
        }
    }
}
