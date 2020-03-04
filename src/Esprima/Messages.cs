namespace Esprima
{
    // Error messages should be identical to V8.
    public static class Messages
    {
        public const string BadImportCallArity = "Unexpected token";
        public const string BadGetterArity = "Getter must not have any formal parameters";
        public const string BadSetterArity = "Setter must have exactly one formal parameter";
        public const string BadSetterRestParameter = "Setter function argument must not be a rest parameter";
        public const string ConstructorIsAsync = "Class constructor may not be an async method";
        public const string ConstructorSpecialMethod = "Class constructor may not be an accessor";
        public const string DeclarationMissingInitializer = "Missing initializer in {0} declaration";
        public const string DefaultRestParameter = "Unexpected token =";
        public const string DefaultRestProperty = "Unexpected token =";
        public const string DuplicateBinding = "Duplicate binding {0}";
        public const string DuplicateConstructor = "A class may only have one constructor";
        public const string DuplicateProtoProperty = "Duplicate __proto__ fields are not allowed in object literals";
        public const string ForInOfLoopInitializer = "'{0} loop variable declaration may not have an initializer";
        public const string GeneratorInLegacyContext = "Generator declarations are not allowed in legacy contexts";
        public const string IllegalBreak = "Illegal break statement";
        public const string IllegalContinue = "Illegal continue statement";
        public const string IllegalExportDeclaration = "Unexpected token";
        public const string IllegalImportDeclaration = "Unexpected token";
        public const string IllegalLanguageModeDirective = "Illegal 'use strict' directive in function with non-simple parameter list";
        public const string IllegalReturn = "Illegal return statement";
        public const string InvalidEscapedReservedWord = "Keyword must not contain escaped characters";
        public const string InvalidHexEscapeSequence = "Invalid hexadecimal escape sequence";
        public const string InvalidLHSInAssignment = "Invalid left-hand side in assignment";
        public const string InvalidLHSInForIn = "Invalid left-hand side in for-in";
        public const string InvalidLHSInForLoop = "Invalid left-hand side in for-loop";
        public const string InvalidModuleSpecifier = "Unexpected token";
        public const string InvalidRegExp = "Invalid regular expression";
        public const string LetInLexicalBinding = "let is disallowed as a lexically bound name";
        public const string MissingFromClause = "Unexpected token";
        public const string MultipleDefaultsInSwitch = "More than one default clause in switch statement";
        public const string NewlineAfterThrow = "Illegal newline after throw";
        public const string NoAsAfterImportNamespace = "Unexpected token";
        public const string NoCatchOrFinally = "Missing catch or finally after try";
        public const string ParameterAfterRestParameter = "Rest parameter must be last formal parameter";
        public const string PropertyAfterRestProperty = "Unexpected token";
        public const string Redeclaration = "{0} \"{1}\" has already been declared";
        public const string StaticPrototype = "Classes may not have static property named prototype";
        public const string StrictCatchVariable = "Catch variable may not be eval or arguments in strict mode";
        public const string StrictDelete = "Delete of an unqualified identifier in strict mode.";
        public const string StrictFunction = "In strict mode code, functions can only be declared at top level or inside a block";
        public const string StrictFunctionName = "Function name may not be eval or arguments in strict mode";
        public const string StrictLHSAssignment = "Assignment to eval or arguments is not allowed in strict mode";
        public const string StrictLHSPostfix = "Postfix increment/decrement may not have eval or arguments operand in strict mode";
        public const string StrictLHSPrefix = "Prefix increment/decrement may not have eval or arguments operand in strict mode";
        public const string StrictModeWith = "Strict mode code may not include a with statement";
        public const string StrictOctalLiteral = "Octal literals are not allowed in strict mode.";
        public const string StrictParamDupe = "Strict mode function may not have duplicate parameter names";
        public const string StrictParamName = "Parameter name eval or arguments is not allowed in strict mode";
        public const string StrictReservedWord = "Use of future reserved word in strict mode";
        public const string StrictVarName = "Variable name may not be eval or arguments in strict mode";
        public const string TemplateOctalLiteral = "Octal literals are not allowed in template strings.";
        public const string UnexpectedEOS = "Unexpected end of input";
        public const string UnexpectedIdentifier = "Unexpected identifier";
        public const string UnexpectedNumber = "Unexpected number";
        public const string UnexpectedReserved = "Unexpected reserved word";
        public const string UnexpectedString = "Unexpected string";
        public const string UnexpectedTemplate = "Unexpected quasi {0}";
        public const string UnexpectedToken = "Unexpected token {0}";
        public const string UnexpectedTokenIllegal = "Unexpected token ILLEGAL";
        public const string UnknownLabel = "Undefined label \"{0}\"";
        public const string UnterminatedRegExp = "Invalid regular expression= missing /";
    }
}
