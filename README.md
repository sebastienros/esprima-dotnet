**Esprima .NET** (BSD license) is a .NET port of the [esprima.org](http://esprima.org) project.
It is a standard-compliant [ECMAScript](http://www.ecma-international.org/publications/standards/Ecma-262.htm)
ECMAScript parser (also popularly known as
[JavaScript](https://en.wikipedia.org/wiki/JavaScript)).

### Features

- Full support for ECMAScript 2016 ([ECMA-262 7th Edition](http://www.ecma-international.org/publications/standards/Ecma-262.htm))
- Sensible [syntax tree format](https://github.com/estree/estree/blob/master/es5.md) as standardized by [ESTree project](https://github.com/estree/estree)
- Optional tracking of syntax node location (index-based and line-column)
- Heavily tested

### API

Esprima can be used to perform [lexical analysis](https://en.wikipedia.org/wiki/Lexical_analysis) (tokenization) or [syntactic analysis](https://en.wikipedia.org/wiki/Parsing) (parsing) of a JavaScript program.

A simple C# example:

```csharp
var parser = new JavaScriptParser("const answer = 42");
var program = parser.ParseProgram();
```

Will return this when serialized in json:

```json
{
    "type": "Program",
    "body": [
        {
            "type": "VariableDeclaration",
            "declarations": [
                {
                    "type": "VariableDeclarator",
                    "id": {
                        "type": "Identifier",
                        "name": "answer"
                    },
                    "init": {
                        "type": "Literal",
                        "value": 42,
                        "raw": "42"
                    }
                }
            ],
            "kind": "const"
        }
    ],
    "sourceType": "script"
}
```
