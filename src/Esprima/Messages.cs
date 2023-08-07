﻿namespace Esprima;

// Error messages should be identical to V8.
// TODO: Replace the messages marked with "temporary" with the actual V8 messages once they become available (see https://github.com/v8/v8/blob/main/src/common/message-template.h).
internal static class Messages
{
    public const string ArgumentsNotAllowedInClassField = "'arguments' is not allowed in class field initializer or static initialization block";
    public const string AsyncFunctionInSingleStatementContext = "Async functions can only be declared at the top level or inside a block.";
    public const string BadGetterArity = "Getter must not have any formal parameters";
    public const string BadSetterArity = "Setter must have exactly one formal parameter";
    public const string BadSetterRestParameter = "Setter function argument must not be a rest parameter";
    public const string CannotUseAwaitInClassStaticBlock = "Cannot use await in class static initialization block";
    public const string CannotUseImportMetaOutsideAModule = "Cannot use 'import.meta' outside a module";
    public const string CannotUseImportWithNew = "Cannot use new with import";
    public const string ConstructorIsAccessor = "Class constructor may not be an accessor";
    public const string ConstructorIsAsync = "Class constructor may not be an async method";
    public const string ConstructorIsField = "Classes may not have a field named 'constructor'";
    public const string ConstructorIsGenerator = "Class constructor may not be a generator";
    public const string DeclarationMissingInitializer = "Missing initializer in {0} declaration";
    public const string DefaultRestParameter = "Unexpected token =";
    public const string DefaultRestProperty = "Unexpected token =";
    public const string DuplicateBinding = "Duplicate binding {0}";
    public const string DuplicateConstructor = "A class may only have one constructor";
    public const string DuplicateParameter = "Duplicate parameter name not allowed in this context";
    public const string DuplicateProtoProperty = "Duplicate __proto__ fields are not allowed in object literals";
    public const string DuplicateKeyInImportAttributes = "Import attributes has duplicate key '{0}'"; // temporary
    public const string ForInOfLoopInitializer = "'{0} loop variable declaration may not have an initializer";
    public const string GeneratorInLegacyContext = "Generator declarations are not allowed in legacy contexts";
    public const string IllegalBreak = "Illegal break statement";
    public const string IllegalContinue = "Illegal continue statement";
    public const string IllegalExportDeclaration = "Unexpected token";
    public const string IllegalImportDeclaration = "Unexpected token";
    public const string IllegalLanguageModeDirective = "Illegal 'use strict' directive in function with non-simple parameter list";
    public const string IllegalReturn = "Illegal return statement";
    public const string InvalidDecoratorMemberExpression = "Invalid decorator member expression"; // temporary
    public const string InvalidEscapedReservedWord = "Keyword must not contain escaped characters";
    public const string InvalidHexEscapeSequence = "Invalid hexadecimal escape sequence";
    public const string InvalidLHSInAssignment = "Invalid left-hand side in assignment";
    public const string InvalidLHSInForIn = "Invalid left-hand side in for-in";
    public const string InvalidLHSInForLoop = "Invalid left-hand side in for-loop";
    public const string InvalidModuleSpecifier = "Unexpected token";
    public const string InvalidOptionalChainFromNewExpression = "Invalid optional chain from new expression";
    public const string InvalidRegExpFlags = "Invalid regular expression flags";
    public const string InvalidTaggedTemplateOnOptionalChain = "Invalid tagged template on optional chain";
    public const string InvalidUnicodeEscapeSequence = "Invalid Unicode escape sequence";
    public const string LetInLexicalBinding = "let is disallowed as a lexically bound name";
    public const string MissingFromClause = "Unexpected token";
    public const string MultipleDefaultsInSwitch = "More than one default clause in switch statement";
    public const string NewlineAfterThrow = "Illegal newline after throw";
    public const string NewTargetNotAllowedHere = "new.target expression is not allowed here";
    public const string NoAsAfterImportNamespace = "Unexpected token";
    public const string NoCatchOrFinally = "Missing catch or finally after try";
    public const string NoSemicolonAfterDecorator = "Decorators must not be followed by a semicolon.";
    public const string NumericSeparatorAfterLeadingZero = "Numeric separator can not be used after leading 0";
    public const string NumericSeparatorNotAllowedHere = "Numeric separator is not allowed here";
    public const string NumericSeparatorOneUnderscore = "Numeric separator must be exactly one underscore";
    public const string ParameterAfterRestParameter = "Rest parameter must be last formal parameter";
    public const string PrivateFieldNoDelete = "Private fields can not be deleted";
    public const string PrivateFieldOutsideClass = "Private field '{0}' must be declared in an enclosing class";
    public const string PropertyAfterRestProperty = "Unexpected token";
    public const string Redeclaration = "{0} \"{1}\" has already been declared";
    public const string RegExpDuplicateCaptureGroupName = "Invalid regular expression: /{0}/{1}: Duplicate capture group name";
    public const string RegExpIncompleteQuantifier = "Invalid regular expression: /{0}/{1}: Incomplete quantifier";
    public const string RegExpInvalidCaptureGroupName = "Invalid regular expression: /{0}/{1}: Invalid capture group name";
    public const string RegExpInvalidEscape = "Invalid regular expression: /{0}/{1}: Invalid escape";
    public const string RegExpInvalidDecimalEscape = "Invalid regular expression: /{0}/{1}: Invalid decimal escape";
    public const string RegExpInvalidCharacterClass = "Invalid regular expression: /{0}/{1}: Invalid character class";
    public const string RegExpInvalidClassEscape = "Invalid regular expression: /{0}/{1}: Invalid class escape";
    public const string RegExpInvalidGroup = "Invalid regular expression: /{0}/{1}: Invalid group";
    public const string RegExpInvalidPropertyName = "Invalid regular expression: /{0}/{1}: Invalid property name";
    public const string RegExpInvalidPropertyNameInCharacterClass = "Invalid regular expression: /{0}/{1}: Invalid property name in character class";
    public const string RegExpInvalidNamedCaptureReferenced = "Invalid regular expression: /{0}/{1}: Invalid named capture referenced";
    public const string RegExpInvalidNamedReference = "Invalid regular expression: /{0}/{1}: Invalid named reference";
    public const string RegExpInvalidQuantifier = "Invalid regular expression: /{0}/{1}: Invalid quantifier";
    public const string RegExpInvalidUnicodeEscape = "Invalid regular expression: /{0}/{1}: Invalid Unicode escape";
    public const string RegExpLoneQuantifierBrackets = "Invalid regular expression: /{0}/{1}: Lone quantifier brackets";
    public const string RegExpNothingToRepeat = "Invalid regular expression: /{0}/{1}: Nothing to repeat";
    public const string RegExpNumbersOutOfOrderInQuantifier = "Invalid regular expression: /{0}/{1}: numbers out of order in {{}} quantifier";
    public const string RegExpRangeOutOfOrderInCharacterClass = "Invalid regular expression: /{0}/{1}: Range out of order in character class";
    public const string RegExpUnmatchedOpenParen = "Invalid regular expression: /{0}/{1}: Unmatched ')'";
    public const string RegExpUnterminatedCharacterClass = "Invalid regular expression: /{0}/{1}: Unterminated character class";
    public const string RegExpUnterminatedGroup = "Invalid regular expression: /{0}/{1}: Unterminated group";
    public const string StaticPrototype = "Classes may not have static property named prototype";
    public const string StrictCatchVariable = "Catch variable may not be eval or arguments in strict mode";
    public const string StrictDecimalWithLeadingZero = "Decimals with leading zeros are not allowed in strict mode.";
    public const string StrictDelete = "Delete of an unqualified identifier in strict mode.";
    public const string StrictFunction = "In strict mode code, functions can only be declared at top level or inside a block";
    public const string StrictFunctionName = "Function name may not be eval or arguments in strict mode";
    public const string StrictLHSAssignment = "Assignment to eval or arguments is not allowed in strict mode";
    public const string StrictLHSPostfix = "Postfix increment/decrement may not have eval or arguments operand in strict mode";
    public const string StrictLHSPrefix = "Prefix increment/decrement may not have eval or arguments operand in strict mode";
    public const string StrictModeWith = "Strict mode code may not include a with statement";
    public const string StrictOctalEscape = "Octal escape sequences are not allowed in strict mode.";
    public const string StrictOctalLiteral = "Octal literals are not allowed in strict mode.";
    public const string StrictParamName = "Parameter name eval or arguments is not allowed in strict mode";
    public const string StrictReservedWord = "Use of future reserved word in strict mode";
    public const string StrictVarName = "Variable name may not be eval or arguments in strict mode";
    public const string TemplateEscape89 = "\\8 and \\9 are not allowed in template strings.";
    public const string TemplateOctalLiteral = "Octal literals are not allowed in template strings.";
    public const string UndefinedUnicodeCodePoint = "Undefined Unicode code-point";
    public const string UnexpectedEOS = "Unexpected end of input";
    public const string UnexpectedIdentifier = "Unexpected identifier";
    public const string UnexpectedNumber = "Unexpected number";
    public const string UnexpectedPrivateField = "Unexpected private field";
    public const string UnexpectedReserved = "Unexpected reserved word";
    public const string UnexpectedString = "Unexpected string";
    public const string UnexpectedSuper = "'super' keyword unexpected here";
    public const string UnexpectedTemplate = "Unexpected quasi {0}";
    public const string UnexpectedToken = "Unexpected token {0}";
    public const string UnexpectedTokenIllegal = "Unexpected token ILLEGAL";
    public const string UnknownLabel = "Undefined label \"{0}\"";
    public const string UnterminatedRegExp = "Invalid regular expression: missing /";
    public const string InvalidUnicodeKeyword = "The `{0}` contextual keyword must not contain Unicode escape sequences.";
}
