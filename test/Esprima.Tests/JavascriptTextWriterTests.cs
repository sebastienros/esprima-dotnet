using System.Text.RegularExpressions;
using Esprima.Ast;
using Esprima.Utils;

namespace Esprima.Tests;

public class JavaScriptTextWriterTests
{
    public record class TestCase(Action<JavaScriptTextWriter> Write, string ExpectedUnformatted, string ExpectedFormatted) { }

    private static readonly Program s_dummyAst = new Script(
        NodeList.Create<Statement>(new[]
        {
            new BlockStatement(NodeList.Create<Statement>(new[]
            {
                new FunctionDeclaration(new Identifier("func"), default, new BlockStatement(default), false, false, false)
            }))
        }),
        false);

    private static readonly Dictionary<string, TestCase> s_testCaseDictionary = new()
    {
        ["TwoLineCommentsAtBeginning"] = new TestCase(
            Write: static (JavaScriptTextWriter writer) =>
            {
                writer.WriteLineComment("abc", JavaScriptTextWriter.TriviaFlags.LeadingNewLineRequired);
                writer.WriteLineComment(" def", JavaScriptTextWriter.TriviaFlags.None);
                writer.Finish();
            },
            ExpectedUnformatted: @"//abc
// def",
            ExpectedFormatted: @"//abc
// def
"
        ),

        ["TwoBlockCommentsAtBeginning_1"] = new TestCase(
            Write: static (JavaScriptTextWriter writer) =>
            {
                writer.WriteBlockComment(new[] { "abc" }, JavaScriptTextWriter.TriviaFlags.LeadingNewLineRequired);
                writer.WriteBlockComment(new[] { " def " }, JavaScriptTextWriter.TriviaFlags.None);
                writer.Finish();
            },
            ExpectedUnformatted: @"/*abc*//* def */",
            ExpectedFormatted: @"/*abc*/ /* def */
"
        ),

        ["TwoBlockCommentsAtBeginning_2"] = new TestCase(
            Write: static (JavaScriptTextWriter writer) =>
            {
                writer.WriteBlockComment(new[] { "abc" }, JavaScriptTextWriter.TriviaFlags.TrailingNewLineRequired);
                writer.WriteBlockComment(new[] { " def " }, JavaScriptTextWriter.TriviaFlags.None);
                writer.Finish();
            },
            ExpectedUnformatted: @"/*abc*/
/* def */",
            ExpectedFormatted: @"/*abc*/
/* def */
"
        ),

        ["TwoBlockCommentsAtBeginning_3"] = new TestCase(
            Write: static (JavaScriptTextWriter writer) =>
            {
                writer.WriteBlockComment(new[] { "abc" }, JavaScriptTextWriter.TriviaFlags.None);
                writer.WriteBlockComment(new[] { " def " }, JavaScriptTextWriter.TriviaFlags.SurroundingNewLineRequired);
                writer.Finish();
            },
            ExpectedUnformatted: @"/*abc*/
/* def */",
            ExpectedFormatted: @"/*abc*/
/* def */
"
        ),

        ["MultiLineBlockAndBlockCommentsAtBeginning_1"] = new TestCase(
            Write: static (JavaScriptTextWriter writer) =>
            {
                writer.WriteBlockComment(new[] { "!", " * abc", " " }, JavaScriptTextWriter.TriviaFlags.None);
                writer.WriteBlockComment(new[] { " def " }, JavaScriptTextWriter.TriviaFlags.None);
                writer.Finish();
            },
            ExpectedUnformatted: @"/*!
 * abc
 *//* def */",
            ExpectedFormatted: @"/*!
 * abc
 */ /* def */
"
        ),

        ["MultiLineBlockAndBlockCommentsAtBeginning_2"] = new TestCase(
            Write: static (JavaScriptTextWriter writer) =>
            {
                writer.WriteBlockComment(new[] { "!", " * abc", " " }, JavaScriptTextWriter.TriviaFlags.SurroundingNewLineRequired);
                writer.WriteBlockComment(new[] { " def " }, JavaScriptTextWriter.TriviaFlags.SurroundingNewLineRequired);
                writer.Finish();
            },
            ExpectedUnformatted: @"/*!
 * abc
 */
/* def */",
            ExpectedFormatted: @"/*!
 * abc
 */
/* def */
"
        ),

        ["LineAndBlockCommentsAtBeginning"] = new TestCase(
            Write: static (JavaScriptTextWriter writer) =>
            {
                writer.WriteLineComment("abc", JavaScriptTextWriter.TriviaFlags.None);
                writer.WriteBlockComment(new[] { " def " }, JavaScriptTextWriter.TriviaFlags.None);
                writer.Finish();
            },
            ExpectedUnformatted: @"//abc
/* def */",
            ExpectedFormatted: @"//abc
/* def */
"
        ),

        ["BlockAndLineCommentsAtBeginning_1"] = new TestCase(
            Write: static (JavaScriptTextWriter writer) =>
            {
                writer.WriteBlockComment(new[] { "abc" }, JavaScriptTextWriter.TriviaFlags.None);
                writer.WriteLineComment(" def", JavaScriptTextWriter.TriviaFlags.None);
                writer.Finish();
            },
            ExpectedUnformatted: @"/*abc*/// def",
            ExpectedFormatted: @"/*abc*/ // def
"
        ),

        ["BlockAndLineCommentsAtBeginning_2"] = new TestCase(
            Write: static (JavaScriptTextWriter writer) =>
            {
                writer.WriteBlockComment(new[] { "abc" }, JavaScriptTextWriter.TriviaFlags.TrailingNewLineRequired);
                writer.WriteLineComment(" def", JavaScriptTextWriter.TriviaFlags.None);
                writer.Finish();
            },
            ExpectedUnformatted: @"/*abc*/
// def",
            ExpectedFormatted: @"/*abc*/
// def
"
        ),

        ["BlockAndLineCommentsAtBeginning_3"] = new TestCase(
            Write: static (JavaScriptTextWriter writer) =>
            {
                writer.WriteBlockComment(new[] { "abc" }, JavaScriptTextWriter.TriviaFlags.None);
                writer.WriteLineComment(" def", JavaScriptTextWriter.TriviaFlags.LeadingNewLineRequired);
                writer.Finish();
            },
            ExpectedUnformatted: @"/*abc*/
// def",
            ExpectedFormatted: @"/*abc*/
// def
"
        ),

        ["LineCommentBetweenTokens_1"] = new TestCase(
            Write: static (JavaScriptTextWriter writer) =>
            {
                var blockStatement = s_dummyAst.Body[0].As<BlockStatement>();
                var functionDeclaration = blockStatement.Body[0].As<FunctionDeclaration>();
                var writeContext = new JavaScriptTextWriter.WriteContext(blockStatement, functionDeclaration);
                writer.WriteKeyword("function", JavaScriptTextWriter.TokenFlags.LeadingSpaceRecommended, ref writeContext);

                writer.WriteLineComment("abc", JavaScriptTextWriter.TriviaFlags.None);

                var identifier = functionDeclaration.Id!;
                writeContext = new JavaScriptTextWriter.WriteContext(functionDeclaration, identifier);
                writeContext.SetNodeProperty(nameof(identifier.Name), node => node.As<Identifier>().Name);
                writer.WriteIdentifier(identifier.Name, JavaScriptTextWriter.TokenFlags.LeadingSpaceRecommended, ref writeContext);

                writer.Finish();
            },
            ExpectedUnformatted: @"function//abc
func",
            ExpectedFormatted: @"function //abc
func"
        ),

        ["LineCommentBetweenTokens_2"] = new TestCase(
            Write: static (JavaScriptTextWriter writer) =>
            {
                var blockStatement = s_dummyAst.Body[0].As<BlockStatement>();
                var functionDeclaration = blockStatement.Body[0].As<FunctionDeclaration>();
                var writeContext = new JavaScriptTextWriter.WriteContext(blockStatement, functionDeclaration);
                writer.WriteKeyword("function", JavaScriptTextWriter.TokenFlags.LeadingSpaceRecommended, ref writeContext);

                writer.WriteLineComment("abc", JavaScriptTextWriter.TriviaFlags.SurroundingNewLineRequired);

                var identifier = functionDeclaration.Id!;
                writeContext = new JavaScriptTextWriter.WriteContext(functionDeclaration, identifier);
                writeContext.SetNodeProperty(nameof(identifier.Name), node => node.As<Identifier>().Name);
                writer.WriteIdentifier(identifier.Name, JavaScriptTextWriter.TokenFlags.LeadingSpaceRecommended, ref writeContext);

                writer.Finish();
            },
            ExpectedUnformatted: @"function
//abc
func",
            ExpectedFormatted: @"function
//abc
func"
        ),

        ["LineCommentBetweenTokens_3"] = new TestCase(
            Write: static (JavaScriptTextWriter writer) =>
            {
                var blockStatement = s_dummyAst.Body[0].As<BlockStatement>();
                var writeContext = new JavaScriptTextWriter.WriteContext(s_dummyAst, blockStatement);
                writer.StartBlock(blockStatement.Body.Count, ref writeContext);
                writer.StartStatementList(blockStatement.Body.Count, ref writeContext);
                writer.StartStatementListItem(0, 1, JavaScriptTextWriter.StatementFlags.MayOmitRightMostSemicolon | JavaScriptTextWriter.StatementFlags.IsRightMost, ref writeContext);

                var functionDeclaration = blockStatement.Body[0].As<FunctionDeclaration>();
                writeContext = new JavaScriptTextWriter.WriteContext(blockStatement, functionDeclaration);
                writer.WriteLineComment("abc", JavaScriptTextWriter.TriviaFlags.None);
                writer.WriteKeyword("function", JavaScriptTextWriter.TokenFlags.LeadingSpaceRecommended, ref writeContext);

                writer.Finish();
            },
            ExpectedUnformatted: @"{//abc
function",
            ExpectedFormatted: @"{
  //abc
  function"
        ),

        ["LineCommentBetweenTokens_4"] = new TestCase(
            Write: static (JavaScriptTextWriter writer) =>
            {
                var blockStatement = s_dummyAst.Body[0].As<BlockStatement>();
                var writeContext = new JavaScriptTextWriter.WriteContext(s_dummyAst, blockStatement);
                writer.StartBlock(blockStatement.Body.Count, ref writeContext);
                writer.StartStatementList(blockStatement.Body.Count, ref writeContext);
                writer.StartStatementListItem(0, 1, JavaScriptTextWriter.StatementFlags.MayOmitRightMostSemicolon | JavaScriptTextWriter.StatementFlags.IsRightMost, ref writeContext);

                var functionDeclaration = blockStatement.Body[0].As<FunctionDeclaration>();
                writeContext = new JavaScriptTextWriter.WriteContext(blockStatement, functionDeclaration);
                writer.WriteLineComment("abc", JavaScriptTextWriter.TriviaFlags.SurroundingNewLineRequired);
                writer.WriteKeyword("function", JavaScriptTextWriter.TokenFlags.LeadingSpaceRecommended, ref writeContext);

                writer.Finish();
            },
            ExpectedUnformatted: @"{
//abc
function",
            ExpectedFormatted: @"{
  //abc
  function"
        ),


        ["BlockCommentBetweenTokens_1"] = new TestCase(
            Write: static (JavaScriptTextWriter writer) =>
            {
                var blockStatement = s_dummyAst.Body[0].As<BlockStatement>();
                var functionDeclaration = blockStatement.Body[0].As<FunctionDeclaration>();
                var writeContext = new JavaScriptTextWriter.WriteContext(blockStatement, functionDeclaration);
                writer.WriteKeyword("function", JavaScriptTextWriter.TokenFlags.LeadingSpaceRecommended, ref writeContext);

                writer.WriteBlockComment(new[] { "abc" }, JavaScriptTextWriter.TriviaFlags.None);

                var identifier = functionDeclaration.Id!;
                writeContext = new JavaScriptTextWriter.WriteContext(functionDeclaration, identifier);
                writeContext.SetNodeProperty(nameof(identifier.Name), node => node.As<Identifier>().Name);
                writer.WriteIdentifier(identifier.Name, JavaScriptTextWriter.TokenFlags.LeadingSpaceRecommended, ref writeContext);

                writer.Finish();
            },
            ExpectedUnformatted: @"function/*abc*/func",
            ExpectedFormatted: @"function /*abc*/ func"
        ),

        ["BlockCommentBetweenTokens_2a"] = new TestCase(
            Write: static (JavaScriptTextWriter writer) =>
            {
                var blockStatement = s_dummyAst.Body[0].As<BlockStatement>();
                var functionDeclaration = blockStatement.Body[0].As<FunctionDeclaration>();
                var writeContext = new JavaScriptTextWriter.WriteContext(blockStatement, functionDeclaration);
                writer.WriteKeyword("function", JavaScriptTextWriter.TokenFlags.LeadingSpaceRecommended, ref writeContext);

                writer.WriteBlockComment(new[] { "abc" }, JavaScriptTextWriter.TriviaFlags.LeadingNewLineRequired);

                var identifier = functionDeclaration.Id!;
                writeContext = new JavaScriptTextWriter.WriteContext(functionDeclaration, identifier);
                writeContext.SetNodeProperty(nameof(identifier.Name), node => node.As<Identifier>().Name);
                writer.WriteIdentifier(identifier.Name, JavaScriptTextWriter.TokenFlags.LeadingSpaceRecommended, ref writeContext);

                writer.Finish();
            },
            ExpectedUnformatted: @"function
/*abc*/func",
            ExpectedFormatted: @"function
/*abc*/ func"
        ),

        ["BlockCommentBetweenTokens_2b"] = new TestCase(
            Write: static (JavaScriptTextWriter writer) =>
            {
                var blockStatement = s_dummyAst.Body[0].As<BlockStatement>();
                var functionDeclaration = blockStatement.Body[0].As<FunctionDeclaration>();
                var writeContext = new JavaScriptTextWriter.WriteContext(blockStatement, functionDeclaration);
                writer.WriteKeyword("function", JavaScriptTextWriter.TokenFlags.LeadingSpaceRecommended, ref writeContext);

                writer.WriteBlockComment(new[] { "abc" }, JavaScriptTextWriter.TriviaFlags.TrailingNewLineRequired);

                var identifier = functionDeclaration.Id!;
                writeContext = new JavaScriptTextWriter.WriteContext(functionDeclaration, identifier);
                writeContext.SetNodeProperty(nameof(identifier.Name), node => node.As<Identifier>().Name);
                writer.WriteIdentifier(identifier.Name, JavaScriptTextWriter.TokenFlags.LeadingSpaceRecommended, ref writeContext);

                writer.Finish();
            },
            ExpectedUnformatted: @"function/*abc*/
func",
            ExpectedFormatted: @"function /*abc*/
func"
        ),

        ["BlockCommentBetweenTokens_2c"] = new TestCase(
            Write: static (JavaScriptTextWriter writer) =>
            {
                var blockStatement = s_dummyAst.Body[0].As<BlockStatement>();
                var functionDeclaration = blockStatement.Body[0].As<FunctionDeclaration>();
                var writeContext = new JavaScriptTextWriter.WriteContext(blockStatement, functionDeclaration);
                writer.WriteKeyword("function", JavaScriptTextWriter.TokenFlags.LeadingSpaceRecommended, ref writeContext);

                writer.WriteBlockComment(new[] { "abc" }, JavaScriptTextWriter.TriviaFlags.SurroundingNewLineRequired);

                var identifier = functionDeclaration.Id!;
                writeContext = new JavaScriptTextWriter.WriteContext(functionDeclaration, identifier);
                writeContext.SetNodeProperty(nameof(identifier.Name), node => node.As<Identifier>().Name);
                writer.WriteIdentifier(identifier.Name, JavaScriptTextWriter.TokenFlags.LeadingSpaceRecommended, ref writeContext);

                writer.Finish();
            },
            ExpectedUnformatted: @"function
/*abc*/
func",
            ExpectedFormatted: @"function
/*abc*/
func"
        ),

        ["BlockCommentBetweenTokens_3"] = new TestCase(
            Write: static (JavaScriptTextWriter writer) =>
            {
                var blockStatement = s_dummyAst.Body[0].As<BlockStatement>();
                var writeContext = new JavaScriptTextWriter.WriteContext(s_dummyAst, blockStatement);
                writer.StartBlock(0, ref writeContext);

                writer.WriteBlockComment(new[] { "abc" }, JavaScriptTextWriter.TriviaFlags.None);

                writer.StartStatementList(0, ref writeContext);
                writer.EndStatementList(0, ref writeContext);
                writer.EndBlock(blockStatement.Body.Count, ref writeContext);

                writer.Finish();
            },
            ExpectedUnformatted: @"{/*abc*/}",
            ExpectedFormatted: @"{ /*abc*/ }"
        ),

        ["BlockCommentBetweenTokens_4"] = new TestCase(
            Write: static (JavaScriptTextWriter writer) =>
            {
                var blockStatement = s_dummyAst.Body[0].As<BlockStatement>();
                var writeContext = new JavaScriptTextWriter.WriteContext(s_dummyAst, blockStatement);
                writer.StartBlock(blockStatement.Body.Count, ref writeContext);
                writer.StartStatementList(blockStatement.Body.Count, ref writeContext);
                writer.StartStatementListItem(0, 1, JavaScriptTextWriter.StatementFlags.MayOmitRightMostSemicolon | JavaScriptTextWriter.StatementFlags.IsRightMost, ref writeContext);

                var functionDeclaration = blockStatement.Body[0].As<FunctionDeclaration>();
                writeContext = new JavaScriptTextWriter.WriteContext(blockStatement, functionDeclaration);
                writer.WriteBlockComment(new[] { "", " * abc", " " }, JavaScriptTextWriter.TriviaFlags.SurroundingNewLineRequired);
                writer.WriteKeyword("function", JavaScriptTextWriter.TokenFlags.LeadingSpaceRecommended, ref writeContext);

                writer.Finish();
            },
            ExpectedUnformatted: @"{
/*
 * abc
 */
function",
            ExpectedFormatted: @"{
  /*
   * abc
   */
  function"
        ),
    };

    public static IEnumerable<object[]> TestCases => s_testCaseDictionary.Keys.Select(key => new object[] { key });

    [Theory]
    [MemberData(nameof(TestCases))]
    public void ExecuteTestCase_Unformatted(string testCaseName)
    {
        var testCase = s_testCaseDictionary[testCaseName];

        var stringWriter = new StringWriter();
        var writer = new JavaScriptTextWriter(stringWriter, JavaScriptTextWriterOptions.Default);
        testCase.Write(writer);

        var expected = Regex.Replace(testCase.ExpectedUnformatted, @"\r\n|\n\r|\n|\r", Environment.NewLine);
        Assert.Equal(expected, stringWriter.ToString());
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void ExecuteTestCase_Formatted(string testCaseName)
    {
        var testCase = s_testCaseDictionary[testCaseName];

        var stringWriter = new StringWriter();
        var writer = new KnRJavaScriptTextFormatter(stringWriter, KnRJavaScriptTextFormatterOptions.Default);
        testCase.Write(writer);

        var expected = Regex.Replace(testCase.ExpectedFormatted, @"\r\n|\n\r|\n|\r", Environment.NewLine);
        Assert.Equal(expected, stringWriter.ToString());
    }
}
