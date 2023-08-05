using System.Diagnostics;
using System.Text;
using Esprima;
using Esprima.Tests.Helpers;

var baseDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\.."));
const string inputFileName = "testcases.template.txt";
const string outputFileName = "testcases.txt";
var utf8EncodingWithoutBOM = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

using var reader = new StreamReader(Path.Combine(baseDir, inputFileName), Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
using var writer = new StreamWriter(Path.Combine(baseDir, outputFileName), append: false, Encoding.UTF8, bufferSize: 4096);
using var nodeProcess = new Process
{
    StartInfo =
    {
        FileName = "node",
        Arguments = Path.Combine("Generator", "generate-matches.js"),
        WorkingDirectory = baseDir,
        UseShellExecute = false,
        CreateNoWindow = true,
        RedirectStandardError = true,
        RedirectStandardInput = true,
        RedirectStandardOutput = true,
        StandardErrorEncoding = utf8EncodingWithoutBOM,
        StandardInputEncoding = utf8EncodingWithoutBOM,
        StandardOutputEncoding = utf8EncodingWithoutBOM,
    },
    EnableRaisingEvents = true
};

nodeProcess.ErrorDataReceived += (s, e) => Console.Error.WriteLine(e.Data);

nodeProcess.Start();

nodeProcess.BeginErrorReadLine();

writer.WriteLine($"# Generated from {inputFileName} at {DateTime.UtcNow:s}");
writer.WriteLine();

static string DecodeStringIfEscaped(string value) => JavaScriptStringHelper.IsStringLiteral(value)
    ? JavaScriptStringHelper.Decode(value)
    : value;

var scannerOptions = new ScannerOptions { Tolerant = true };
var output = new List<string>();
var lineNumber = 0;
var testCaseCount = 0;
string? line;
while ((line = reader.ReadLine()) is not null)
{
    ++lineNumber;

    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#", StringComparison.Ordinal))
    {
        writer.WriteLine(line);
        continue;
    }

    var parts = line.Split('\t', StringSplitOptions.None);
    string adaptedPattern;
    try
    {
        if (parts.Length < 3)
        {
            throw new InvalidOperationException("Incomplete input data.");
        }

        var pattern = DecodeStringIfEscaped(parts[0]);
        var flags = DecodeStringIfEscaped(parts[1]);

        var regexParser = new Scanner.RegExpParser(pattern, flags, scannerOptions);
        try { adaptedPattern = regexParser.ParseCore(out _, out _) ?? ")inconvertible("; }
        catch (ParserException) { adaptedPattern = ")syntax-error("; }
        var encodedDotnetPattern = JavaScriptStringHelper.Encode(adaptedPattern, addDoubleQuotes: false);
        if (adaptedPattern != encodedDotnetPattern)
        {
            adaptedPattern = "\"" + encodedDotnetPattern + "\"";
        }
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Invalid input data at line {lineNumber}: {line}");
        Console.Error.WriteLine(ex);
        return -1;
    }

    nodeProcess.StandardInput.WriteLine(line);
    var matchesJson = nodeProcess.StandardOutput.ReadLine();
    if (matchesJson is null)
    {
        Console.Error.WriteLine($"Generating matches failed at line {lineNumber}: {line}");
        nodeProcess.WaitForExit(2500);
        return -1;
    }

    var matches = JavaScriptStringHelper.ParseAsExpression(matchesJson);

    line = parts[0] + "\t" + parts[1] + "\t" + adaptedPattern + "\t" + parts[2] + "\t" + matchesJson;
    if (parts.Length > 3)
    {
        line += "\t" + string.Join("\t", parts.Skip(3));
    }

    writer.WriteLine(line);
    testCaseCount++;
}

writer.Flush();

Console.WriteLine($"Generated {testCaseCount} fixtures.");

return 0;
