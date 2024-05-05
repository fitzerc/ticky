using System.Globalization;
using System.IO;
using Ticky.Core;
using Ticky.Core.Data;

namespace Ticky.Conversion;

public interface IVersionConverter
{
    public Task Version1ToVersion2();
}

public class VersionConverter : IVersionConverter
{
    private readonly ITickyDataWriter _writer;
    private readonly string _appDataFolder;

    public VersionConverter(ITickyDataWriter writer)
    {
        _writer = writer;
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _appDataFolder = Path.Combine(localAppData, nameof(Ticky));
    }

    public async Task Version1ToVersion2()
    {
        string[] csvFiles = Directory.GetFiles(_appDataFolder, "*.csv");

        foreach (var csvFile in csvFiles.Where(f => !f.Contains("ticky-v2")))
        {
            var lines = await File.ReadAllLinesAsync(csvFile);
            var timeEntries = new List<TimeEntry>();

            foreach (var line in lines.Skip(1))
            {
                timeEntries.Add(V1StringToTimeEntry(line));
            }

            foreach (var entry in timeEntries)
            {
                var fileName = Path.GetFileName(csvFile);
                var splitFileName = fileName.Split('.')[0].Split('-');

                var y = int.Parse(splitFileName[1]);
                var m = int.Parse(splitFileName[2]);
                var d = int.Parse(splitFileName[3]);

                await _writer.WriteTimeEntryAsync(entry, new DateTime(y, m, d));
            }
        }

        foreach (var csvFile in csvFiles.Where(f => !f.Contains("ticky-v2")))
        {
            File.Delete(csvFile);
        }
    }

    public TimeEntry V1StringToTimeEntry(string v1Str)
    {
        var v1Split = v1Str.Split(',');

        try
        {
            var startTime = DateTime.ParseExact(v1Split[4].ToString(), "yyyy-MM-dd HH:mm:ss.fff",
                CultureInfo.InvariantCulture);
            var endTime = DateTime.ParseExact(v1Split[5].ToString(), "yyyy-MM-dd HH:mm:ss.fff",
                CultureInfo.InvariantCulture);

            return new TimeEntry
            (
                v1Str[0].ToString(),
                v1Str[1].ToString(),
                v1Str[2].ToString(),
                endTime - startTime,
                startTime,
                endTime
            );
        }
        catch (Exception e)
        {
            return new TimeEntry("", "", "", TimeSpan.MaxValue);
        }
    }
}