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
| underscore-1.5.2 | 43 KB | 3.9 ms | 6.6 ms |
| backbone-1.1.0 | 60 KB | 4.4 ms | 6.1 ms |
| mootools-1.4.5 | 163 KB | 33.2 ms | 29.8 ms | 
| jquery-1.9.1 | 271 KB | 44.2 ms | 36.2 ms |
| yui-3.12.0| 341 KB | 27.6 ms | 33.4 ms |
| jquery.mobile-1.4.2 | 456 K | 65.4 ms | 159.1 ms | 
| angular-1.5.6 | 1158 KB | 94.9 ms | 98.4 ms |