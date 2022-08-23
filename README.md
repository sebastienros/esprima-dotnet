| :mega: Important notices |
|--------------|
|If you are upgrading from an older version, please note that version 3 ships with numerous breaking changes to the public API because virtually all areas of the library have been revised.<br />Documentation of the previous major version is available [here](https://github.com/sebastienros/esprima-dotnet/tree/v2.1.3). |

[![Build](https://github.com/sebastienros/esprima-dotnet/actions/workflows/staging.yml/badge.svg)](https://github.com/sebastienros/esprima-dotnet/actions/workflows/staging.yml)
[![NuGet](https://img.shields.io/nuget/v/esprima.svg)](https://www.nuget.org/packages/esprima)
[![MyGet](https://img.shields.io/myget/esprimadotnet/v/esprima?label=MyGet)](https://www.myget.org/feed/esprimadotnet/package/nuget/Esprima)

**Esprima .NET** (BSD license) is a .NET port of the [esprima.org](http://esprima.org) project.
It is a standard-compliant [ECMAScript](http://www.ecma-international.org/publications/standards/Ecma-262.htm)
parser (also popularly known as
[JavaScript](https://en.wikipedia.org/wiki/JavaScript)).

### Features

- Full support for ECMAScript 2019 ([ECMA-262 10th Edition](http://www.ecma-international.org/publications/standards/Ecma-262.htm))
- Sensible [syntax tree format](https://github.com/estree/estree/blob/master/es5.md) as standardized by [ESTree project](https://github.com/estree/estree)
- Experimental support for [JSX](https://facebook.github.io/jsx/), a syntax extension for [React](https://facebook.github.io/react/)
- Optional tracking of syntax node location (index-based and line-column)
- Heavily tested

### API

Esprima can be used to perform [lexical analysis](https://en.wikipedia.org/wiki/Lexical_analysis) (tokenization) or [syntactic analysis](https://en.wikipedia.org/wiki/Parsing) (parsing) of a JavaScript program.

A simple C# example:

```csharp
var parser = new JavaScriptParser();
var program = parser.ParseScript("const answer = 42");
```

You can control the behavior of the parser by initializing and passing a `ParserOptions` to the parser's constructor. (For the available options, see the XML comments of the `ParserOptions` class.)

Instead of `ParseScript`, you may use `ParseModule` or `ParseExpression` to make the parser treat the input as an ES6 module or as a plain JavaScript expression respectively.

In case the input is syntactically correct, each of these methods returns the root node of the resulting *abstract syntax tree (AST)*, which you can freely analyze or transform. The library provides the `AstVisitor` and `AstRewriter` visitor classes to help you with such tasks.

When the input contains a severe syntax error, a `ParserException` is thrown. By catching it you can get details about the error. There are syntax errors though which can be tolerated by the parser. Such errors are ignored by default. You can record them by setting `ParserOptions.ErrorHandler` to an instance of `CollectingErrorHandler`. Alternatively, you can set `ParserOptions.Tolerant` to false to make the parser throw exceptions also in the case of tolerable syntax errors.

The library is able to write the AST (except for comments) back to JavaScript code:

```csharp
var code = program.ToJavaScriptString(format: true);
```

It is also possible to serialize the AST into a JSON representation:

```csharp
var astJson = program.ToJsonString(indent: "    ");
```

Considering the example above this call will return the following JSON:

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
    "sourceType": "script",
    "strict": false
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
