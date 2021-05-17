[![Build](https://github.com/sebastienros/esprima-dotnet/actions/workflows/build.yml/badge.svg)](https://github.com/sebastienros/esprima-dotnet/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/esprima.svg)](https://www.nuget.org/packages/esprima)

**Esprima .NET** (BSD license) is a .NET port of the [esprima.org](http://esprima.org) project.
It is a standard-compliant [ECMAScript](http://www.ecma-international.org/publications/standards/Ecma-262.htm)
parser (also popularly known as
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

### Benchmarks

Here is a list of common JavaScript libraries and the time it takes to parse them, 
compared to the time it took for the same script using the original Esprima in Chrome.

| Script | Size | Esprima .NET | Esprima (Chrome) |
| --- | --- | --- | --- |
| underscore-1.5.2 | 43 KB | 2.4 ms | 3.1 ms |
| backbone-1.1.0 | 60 KB | 2.9 ms | 3.5 ms |
| mootools-1.4.5 | 163 KB | 18.7 ms | 16.2 ms | 
| jquery-1.9.1 | 271 KB | 22.8 ms | 19.0 ms |
| yui-3.12.0| 341 KB | 17.2 ms | 16.2 ms |
| jquery.mobile-1.4.2 | 456 K | 43.3 ms | 46.9 ms | 
| angular-1.2.5 | 721 KB | 29.3 ms | 37.2 ms |
