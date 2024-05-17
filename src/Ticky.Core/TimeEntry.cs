using System.Globalization;
using System.Text;

namespace Ticky.Core;

public class TimeEntry(
    string project,
    string task,
    string tag,
    TimeSpan elapsed,
    DateTime startTime,
    DateTime endTime)
{
    public TimeEntry(string project,
        string task,
        string tag,
        TimeSpan elapsed) : this(project, task, tag, elapsed, DateTime.Now.Subtract(elapsed), DateTime.Now) {}

    public string Project { get; set; } = project;
    public string Task { get; set; } = task;
    public string Tag { get; set; } = tag;
    public TimeSpan Elapsed { get; set; } = elapsed;
    public DateTime StartTime { get; set; } = startTime;
    public DateTime EndTime { get; set; } = endTime;

    public static TimeEntry FromCsvString(string csvString)
    {
        var dateFormat = "yyyy-MM-dd HH:mm:ss.fff";
        var projIdx = 0;
        var taskIdx = 1;
        var tagIdx = 2;
        var elapsedIdx = 3;
        var startTimeIdx = 4;
        var endTimeIdx = 5;

        if (string.IsNullOrEmpty(csvString))
        {
            throw new ArgumentNullException(nameof(csvString));
        }

        var entries = csvString.Split(',');

        if (entries.Length != 6)
        {
            throw new ArgumentException($"{nameof(csvString)} should be in {GetPropNamesCsv()} format");
        }

        var project = entries[projIdx];
        var task = entries[taskIdx];
        var tag = entries[tagIdx];
        TimeSpan? elapsed = null;

        if (double.TryParse(entries[elapsedIdx], out var tmpElapsed))
        {
            elapsed = TimeSpan.FromMilliseconds(tmpElapsed);
        }

        if (elapsed is null)
        {
            throw new ArgumentException("unable to parse Elapsed to double");
        }

        var start = DateTime.ParseExact(entries[startTimeIdx], dateFormat, CultureInfo.InvariantCulture);
        var end = DateTime.ParseExact(entries[endTimeIdx], dateFormat, CultureInfo.InvariantCulture);

        return new TimeEntry(project, task, tag, elapsed.Value, start, end);
    }

    public static string GetPropNamesCsv()
    {
        var comma = ',';

        var sb = new StringBuilder(nameof(Project))
            .Append(comma)
            .Append(nameof(Task))
            .Append(comma)
            .Append(nameof(Tag))
            .Append(comma)
            .Append(nameof(StartTime))
            .Append(comma)
            .Append(nameof(EndTime))
            .Append(comma)
            .Append(nameof(Elapsed));

        return sb.ToString();
    }
}

public static class TimeEntryExtensions
{
    public static string ToCsv(this TimeEntry te)
    {
        var dateFormat = "yyyy-MM-dd HH:mm:ss.fff";
        var comma = ',';
        var sb = new StringBuilder(te.Project)
            .Append(comma)
            .Append(te.Task)
            .Append(comma)
            .Append(te.Tag)
            .Append(comma)
            .Append(te.StartTime.ToString(dateFormat))
            .Append(comma)
            .Append(te.EndTime.ToString(dateFormat))
            .Append(comma)
            .Append(te.Elapsed.ToString(@"hh\:mm\:ss"));

        return sb.ToString();
    }
}
