using NUnit.Framework;
using Serilog;
using System.Text;
using TauCode.Infrastructure.Time;
using TauCode.IO;

namespace TauCode.Working.Tests;

[TestFixture]
public partial class WorkerTests
{
    private ILogger _logger = null!;
    private StringWriterWithEncoding _writer = null!;

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

    private string CurrentLog => _writer.ToString();

    private static string CutLog(string log)
    {
        var prefixLength = "2022-08-17 17:48:57.833 ".Length;

        var cutLines = log
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Substring(prefixLength))
            .ToList();

        var cutLog = string.Join(Environment.NewLine, cutLines);

        return cutLog;
    }
}