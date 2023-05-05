using NUnit.Framework;
using Serilog;
using System.Text;
using System.Text.RegularExpressions;
using TauCode.Extensions;
using TauCode.Infrastructure.Time;
using TauCode.IO;

namespace TauCode.Working.Tests.Labor;

[TestFixture]
public partial class LaborerTests
{
    private ILogger _logger = null!;
    private StringWriterWithEncoding _writer = null!;

    private Dictionary<string, string> _testLogs = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _testLogs = new Dictionary<string, string>();

        var resourceNames = new string[]
        {
            "Dispose.Logs.txt",
        };

        foreach (var resourceName in resourceNames)
        {
            var tests = this.LoadTestLogs(resourceName);

            foreach (var test in tests)
            {
                _testLogs.Add(test.Key, test.Value);
            }
        }
    }

    [SetUp]
    public void SetUp()
    {
        _writer = new StringWriterWithEncoding(Encoding.UTF8);
        _logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.TextWriter(
                _writer,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}]{ObjectTag} {Message}{NewLine}{Exception}")
            .CreateLogger();

        TimeProvider.Reset();
    }

    private string GetLog()
    {
        return _writer.ToString();
    }

    private string[] GetLogLines()
    {
        const string timeSample = "2023-05-02 11:46:33.819 ";
        var timeLength = timeSample.Length;

        var logText = _writer.ToString();
        var logLines = logText
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x[timeLength..])
            .ToArray();

        return logLines;
    }

    private string[] GetExpectedLog(string testName)
    {
        var logText = _testLogs[testName];
        var logLines = logText
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .ToArray();

        return logLines;
    }

    private Dictionary<string, string> LoadTestLogs(string resourceName)
    {
        var result = new Dictionary<string, string>();
        var text = this.GetType().Assembly.GetResourceText(resourceName, true);

        var matches = Regex
            .Matches(text, "=== ([a-zA-Z0-9]+_[a-zA-Z0-9]+_[a-zA-Z0-9]+) ===\r\n")
            .ToArray();

        for (var i = 0; i < matches.Length; i++)
        {
            var match = matches[i];
            var testName = match.Groups["1"].Value;

            var logStart = match.Index + match.Length;
            var logEnd = text.Length;

            if (i < matches.Length - 1)
            {
                var nextMatch = matches[i + 1];

                logEnd = nextMatch.Index;
            }

            var logLength = logEnd - logStart;
            var log = text.Substring(logStart, logLength).Trim();

            result.Add(testName, log);
        }

        return result;
    }
}