using Esprima.Ast;
using Esprima.Utils;

namespace Esprima.Tests;

public class JavascriptTextWriterTests
{
    public record class TestCase(Action<JavascriptTextWriter> Write, string ExpectedUnformatted, string ExpectedFormatted) { }

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
            Write: static (JavascriptTextWriter writer) =>
            {
                writer.WriteLineComment("abc", JavascriptTextWriter.TriviaFlags.LeadingNewLineRequired);
                writer.WriteLineComment(" def", JavascriptTextWriter.TriviaFlags.None);
                writer.Finish();
            },
            ExpectedUnformatted: @"//abc
// def",
            ExpectedFormatted: @"//abc
// def
"
        ),

        ["TwoBlockCommentsAtBeginning_1"] = new TestCase(
            Write: static (JavascriptTextWriter writer) =>
            {
                writer.WriteBlockComment(new[] { "abc" }, JavascriptTextWriter.TriviaFlags.LeadingNewLineRequired);
                writer.WriteBlockComment(new[] { " def " }, JavascriptTextWriter.TriviaFlags.None);
                writer.Finish();
            },
            ExpectedUnformatted: @"/*abc*//* def */",
            ExpectedFormatted: @"/*abc*/ /* def */
"
        ),

        ["TwoBlockCommentsAtBeginning_2"] = new TestCase(
            Write: static (JavascriptTextWriter writer) =>
            {
                writer.WriteBlockComment(new[] { "abc" }, JavascriptTextWriter.TriviaFlags.TrailingNewLineRequired);
                writer.WriteBlockComment(new[] { " def " }, JavascriptTextWriter.TriviaFlags.None);
                writer.Finish();
            },
            ExpectedUnformatted: @"/*abc*/
/* def */",
            ExpectedFormatted: @"/*abc*/
/* def */
"
        ),

        ["TwoBlockCommentsAtBeginning_3"] = new TestCase(
            Write: static (JavascriptTextWriter writer) =>
            {
                writer.WriteBlockComment(new[] { "abc" }, JavascriptTextWriter.TriviaFlags.None);
                writer.WriteBlockComment(new[] { " def " }, JavascriptTextWriter.TriviaFlags.SurroundingNewLineRequired);
                writer.Finish();
            },
            ExpectedUnformatted: @"/*abc*/
/* def */",
            ExpectedFormatted: @"/*abc*/
/* def */
"
        ),

        ["MultiLineBlockAndBlockCommentsAtBeginning_1"] = new TestCase(
            Write: static (JavascriptTextWriter writer) =>
            {
                writer.WriteBlockComment(new[] { "!", " * abc", " " }, JavascriptTextWriter.TriviaFlags.None);
                writer.WriteBlockComment(new[] { " def " }, JavascriptTextWriter.TriviaFlags.None);
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
            Write: static (JavascriptTextWriter writer) =>
            {
                writer.WriteBlockComment(new[] { "!", " * abc", " " }, JavascriptTextWriter.TriviaFlags.SurroundingNewLineRequired);
                writer.WriteBlockComment(new[] { " def " }, JavascriptTextWriter.TriviaFlags.SurroundingNewLineRequired);
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
            Write: static (JavascriptTextWriter writer) =>
            {
                writer.WriteLineComment("abc", JavascriptTextWriter.TriviaFlags.None);
                writer.WriteBlockComment(new[] { " def " }, JavascriptTextWriter.TriviaFlags.None);
                writer.Finish();
            },
            ExpectedUnformatted: @"//abc
/* def */",
            ExpectedFormatted: @"//abc
/* def */
"
        ),

        ["BlockAndLineCommentsAtBeginning_1"] = new TestCase(
            Write: static (JavascriptTextWriter writer) =>
            {
                writer.WriteBlockComment(new[] { "abc" }, JavascriptTextWriter.TriviaFlags.None);
                writer.WriteLineComment(" def", JavascriptTextWriter.TriviaFlags.None);
                writer.Finish();
            },
            ExpectedUnformatted: @"/*abc*/// def",
            ExpectedFormatted: @"/*abc*/ // def
"
        ),

        ["BlockAndLineCommentsAtBeginning_2"] = new TestCase(
            Write: static (JavascriptTextWriter writer) =>
            {
                writer.WriteBlockComment(new[] { "abc" }, JavascriptTextWriter.TriviaFlags.TrailingNewLineRequired);
                writer.WriteLineComment(" def", JavascriptTextWriter.TriviaFlags.None);
                writer.Finish();
            },
            ExpectedUnformatted: @"/*abc*/
// def",
            ExpectedFormatted: @"/*abc*/
// def
"
        ),

        ["BlockAndLineCommentsAtBeginning_3"] = new TestCase(
            Write: static (JavascriptTextWriter writer) =>
            {
                writer.WriteBlockComment(new[] { "abc" }, JavascriptTextWriter.TriviaFlags.None);
                writer.WriteLineComment(" def", JavascriptTextWriter.TriviaFlags.LeadingNewLineRequired);
                writer.Finish();
            },
            ExpectedUnformatted: @"/*abc*/
// def",
            ExpectedFormatted: @"/*abc*/
// def
"
        ),

        ["LineCommentBetweenTokens_1"] = new TestCase(
            Write: static (JavascriptTextWriter writer) =>
            {
                var blockStatement = s_dummyAst.Body[0].As<BlockStatement>();
                var functionDeclaration = blockStatement.Body[0].As<FunctionDeclaration>();
                var writeContext = new JavascriptTextWriter.WriteContext(blockStatement, functionDeclaration);
                writer.WriteKeyword("function", JavascriptTextWriter.TokenFlags.LeadingSpaceRecommended, ref writeContext);

                writer.WriteLineComment("abc", JavascriptTextWriter.TriviaFlags.None);

                var identifier = functionDeclaration.Id!;
                writeContext = new JavascriptTextWriter.WriteContext(functionDeclaration, identifier);
                writeContext.SetNodeProperty(nameof(identifier.Name), node => node.As<Identifier>().Name);
                writer.WriteIdentifier(identifier.Name, JavascriptTextWriter.TokenFlags.LeadingSpaceRecommended, ref writeContext);

                writer.Finish();
            },
            ExpectedUnformatted: @"function//abc
func",
            ExpectedFormatted: @"function //abc
func
"
        ),

        ["LineCommentBetweenTokens_2"] = new TestCase(
            Write: static (JavascriptTextWriter writer) =>
            {
                var blockStatement = s_dummyAst.Body[0].As<BlockStatement>();
                var functionDeclaration = blockStatement.Body[0].As<FunctionDeclaration>();
                var writeContext = new JavascriptTextWriter.WriteContext(blockStatement, functionDeclaration);
                writer.WriteKeyword("function", JavascriptTextWriter.TokenFlags.LeadingSpaceRecommended, ref writeContext);

                writer.WriteLineComment("abc", JavascriptTextWriter.TriviaFlags.SurroundingNewLineRequired);

                var identifier = functionDeclaration.Id!;
                writeContext = new JavascriptTextWriter.WriteContext(functionDeclaration, identifier);
                writeContext.SetNodeProperty(nameof(identifier.Name), node => node.As<Identifier>().Name);
                writer.WriteIdentifier(identifier.Name, JavascriptTextWriter.TokenFlags.LeadingSpaceRecommended, ref writeContext);

                writer.Finish();
            },
            ExpectedUnformatted: @"function
//abc
func",
            ExpectedFormatted: @"function
//abc
func
"
        ),

        ["LineCommentBetweenTokens_3"] = new TestCase(
            Write: static (JavascriptTextWriter writer) =>
            {
                var blockStatement = s_dummyAst.Body[0].As<BlockStatement>();
                var writeContext = new JavascriptTextWriter.WriteContext(s_dummyAst, blockStatement);
                writer.StartBlock(blockStatement.Body.Count, ref writeContext);
                writer.StartStatementList(blockStatement.Body.Count, ref writeContext);
                writer.StartStatementListItem(0, 1, JavascriptTextWriter.StatementFlags.MayOmitRightMostSemicolon | JavascriptTextWriter.StatementFlags.IsRightMost, ref writeContext);

                var functionDeclaration = blockStatement.Body[0].As<FunctionDeclaration>();
                writeContext = new JavascriptTextWriter.WriteContext(blockStatement, functionDeclaration);
                writer.WriteLineComment("abc", JavascriptTextWriter.TriviaFlags.None);
                writer.WriteKeyword("function", JavascriptTextWriter.TokenFlags.LeadingSpaceRecommended, ref writeContext);

                writer.Finish();
            },
            ExpectedUnformatted: @"{//abc
function",
            ExpectedFormatted: @"{
  //abc
  function
"
        ),

        ["LineCommentBetweenTokens_4"] = new TestCase(
            Write: static (JavascriptTextWriter writer) =>
            {
                var blockStatement = s_dummyAst.Body[0].As<BlockStatement>();
                var writeContext = new JavascriptTextWriter.WriteContext(s_dummyAst, blockStatement);
                writer.StartBlock(blockStatement.Body.Count, ref writeContext);
                writer.StartStatementList(blockStatement.Body.Count, ref writeContext);
                writer.StartStatementListItem(0, 1, JavascriptTextWriter.StatementFlags.MayOmitRightMostSemicolon | JavascriptTextWriter.StatementFlags.IsRightMost, ref writeContext);

                var functionDeclaration = blockStatement.Body[0].As<FunctionDeclaration>();
                writeContext = new JavascriptTextWriter.WriteContext(blockStatement, functionDeclaration);
                writer.WriteLineComment("abc", JavascriptTextWriter.TriviaFlags.SurroundingNewLineRequired);
                writer.WriteKeyword("function", JavascriptTextWriter.TokenFlags.LeadingSpaceRecommended, ref writeContext);

                writer.Finish();
            },
            ExpectedUnformatted: @"{
//abc
function",
            ExpectedFormatted: @"{
  //abc
  function
"
        ),


        ["BlockCommentBetweenTokens_1"] = new TestCase(
            Write: static (JavascriptTextWriter writer) =>
            {
                var blockStatement = s_dummyAst.Body[0].As<BlockStatement>();
                var functionDeclaration = blockStatement.Body[0].As<FunctionDeclaration>();
                var writeContext = new JavascriptTextWriter.WriteContext(blockStatement, functionDeclaration);
                writer.WriteKeyword("function", JavascriptTextWriter.TokenFlags.LeadingSpaceRecommended, ref writeContext);

                writer.WriteBlockComment(new[] { "abc" }, JavascriptTextWriter.TriviaFlags.None);

                var identifier = functionDeclaration.Id!;
                writeContext = new JavascriptTextWriter.WriteContext(functionDeclaration, identifier);
                writeContext.SetNodeProperty(nameof(identifier.Name), node => node.As<Identifier>().Name);
                writer.WriteIdentifier(identifier.Name, JavascriptTextWriter.TokenFlags.LeadingSpaceRecommended, ref writeContext);

                writer.Finish();
            },
            ExpectedUnformatted: @"function/*abc*/func",
            ExpectedFormatted: @"function /*abc*/ func
"
        ),

        ["BlockCommentBetweenTokens_2a"] = new TestCase(
            Write: static (JavascriptTextWriter writer) =>
            {
                var blockStatement = s_dummyAst.Body[0].As<BlockStatement>();
                var functionDeclaration = blockStatement.Body[0].As<FunctionDeclaration>();
                var writeContext = new JavascriptTextWriter.WriteContext(blockStatement, functionDeclaration);
                writer.WriteKeyword("function", JavascriptTextWriter.TokenFlags.LeadingSpaceRecommended, ref writeContext);

                writer.WriteBlockComment(new[] { "abc" }, JavascriptTextWriter.TriviaFlags.LeadingNewLineRequired);

                var identifier = functionDeclaration.Id!;
                writeContext = new JavascriptTextWriter.WriteContext(functionDeclaration, identifier);
                writeContext.SetNodeProperty(nameof(identifier.Name), node => node.As<Identifier>().Name);
                writer.WriteIdentifier(identifier.Name, JavascriptTextWriter.TokenFlags.LeadingSpaceRecommended, ref writeContext);

                writer.Finish();
            },
            ExpectedUnformatted: @"function
/*abc*/func",
            ExpectedFormatted: @"function
/*abc*/ func
"
        ),

        ["BlockCommentBetweenTokens_2b"] = new TestCase(
            Write: static (JavascriptTextWriter writer) =>
            {
                var blockStatement = s_dummyAst.Body[0].As<BlockStatement>();
                var functionDeclaration = blockStatement.Body[0].As<FunctionDeclaration>();
                var writeContext = new JavascriptTextWriter.WriteContext(blockStatement, functionDeclaration);
                writer.WriteKeyword("function", JavascriptTextWriter.TokenFlags.LeadingSpaceRecommended, ref writeContext);

                writer.WriteBlockComment(new[] { "abc" }, JavascriptTextWriter.TriviaFlags.TrailingNewLineRequired);

                var identifier = functionDeclaration.Id!;
                writeContext = new JavascriptTextWriter.WriteContext(functionDeclaration, identifier);
                writeContext.SetNodeProperty(nameof(identifier.Name), node => node.As<Identifier>().Name);
                writer.WriteIdentifier(identifier.Name, JavascriptTextWriter.TokenFlags.LeadingSpaceRecommended, ref writeContext);

                writer.Finish();
            },
            ExpectedUnformatted: @"function/*abc*/
func",
            ExpectedFormatted: @"function /*abc*/
func
"
        ),

        ["BlockCommentBetweenTokens_2c"] = new TestCase(
            Write: static (JavascriptTextWriter writer) =>
            {
                var blockStatement = s_dummyAst.Body[0].As<BlockStatement>();
                var functionDeclaration = blockStatement.Body[0].As<FunctionDeclaration>();
                var writeContext = new JavascriptTextWriter.WriteContext(blockStatement, functionDeclaration);
                writer.WriteKeyword("function", JavascriptTextWriter.TokenFlags.LeadingSpaceRecommended, ref writeContext);

                writer.WriteBlockComment(new[] { "abc" }, JavascriptTextWriter.TriviaFlags.SurroundingNewLineRequired);

                var identifier = functionDeclaration.Id!;
                writeContext = new JavascriptTextWriter.WriteContext(functionDeclaration, identifier);
                writeContext.SetNodeProperty(nameof(identifier.Name), node => node.As<Identifier>().Name);
                writer.WriteIdentifier(identifier.Name, JavascriptTextWriter.TokenFlags.LeadingSpaceRecommended, ref writeContext);

                writer.Finish();
            },
            ExpectedUnformatted: @"function
/*abc*/
func",
            ExpectedFormatted: @"function
/*abc*/
func
"
        ),

        ["BlockCommentBetweenTokens_3"] = new TestCase(
            Write: static (JavascriptTextWriter writer) =>
            {
                var blockStatement = s_dummyAst.Body[0].As<BlockStatement>();
                var writeContext = new JavascriptTextWriter.WriteContext(s_dummyAst, blockStatement);
                writer.StartBlock(0, ref writeContext);

                writer.WriteBlockComment(new[] { "abc" }, JavascriptTextWriter.TriviaFlags.None);

                writer.StartStatementList(0, ref writeContext);
                writer.EndStatementList(0, ref writeContext);
                writer.EndBlock(blockStatement.Body.Count, ref writeContext);

                writer.Finish();
            },
            ExpectedUnformatted: @"{/*abc*/}",
            ExpectedFormatted: @"{ /*abc*/ }
"
        ),

        ["BlockCommentBetweenTokens_4"] = new TestCase(
            Write: static (JavascriptTextWriter writer) =>
            {
                var blockStatement = s_dummyAst.Body[0].As<BlockStatement>();
                var writeContext = new JavascriptTextWriter.WriteContext(s_dummyAst, blockStatement);
                writer.StartBlock(blockStatement.Body.Count, ref writeContext);
                writer.StartStatementList(blockStatement.Body.Count, ref writeContext);
                writer.StartStatementListItem(0, 1, JavascriptTextWriter.StatementFlags.MayOmitRightMostSemicolon | JavascriptTextWriter.StatementFlags.IsRightMost, ref writeContext);

                var functionDeclaration = blockStatement.Body[0].As<FunctionDeclaration>();
                writeContext = new JavascriptTextWriter.WriteContext(blockStatement, functionDeclaration);
                writer.WriteBlockComment(new[] { "", " * abc", " " }, JavascriptTextWriter.TriviaFlags.SurroundingNewLineRequired);
                writer.WriteKeyword("function", JavascriptTextWriter.TokenFlags.LeadingSpaceRecommended, ref writeContext);

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
  function
"
        ),
    };

    public static IEnumerable<object[]> TestCases => s_testCaseDictionary.Keys.Select(key => new object[] { key });

    [Theory]
    [MemberData(nameof(TestCases))]
    public void ExecuteTestCase_Unformatted(string testCaseName)
    {
        var testCase = s_testCaseDictionary[testCaseName];

        var stringWriter = new StringWriter();
        var writer = new JavascriptTextWriter(stringWriter, JavascriptTextWriterOptions.Default);
        testCase.Write(writer);

        Assert.Equal(testCase.ExpectedUnformatted, stringWriter.ToString());
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void ExecuteTestCase_Formatted(string testCaseName)
    {
        var testCase = s_testCaseDictionary[testCaseName];

        var stringWriter = new StringWriter();
        var writer = new KnRJavascriptTextWriter(stringWriter, KnRJavascriptTextWriterOptions.Default);
        testCase.Write(writer);

        Assert.Equal(testCase.ExpectedFormatted, stringWriter.ToString());
    }
}
