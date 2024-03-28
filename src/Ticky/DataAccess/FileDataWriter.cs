﻿using System.IO;
using FluentResults;
using Ticky.Core;
using Ticky.Core.Data;

namespace Ticky.DataAccess;

public class FileDataWriter : ITickyDataWriter
{
    private readonly string _appDataFolder;

    public FileDataWriter()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _appDataFolder = Path.Combine(localAppData, nameof(Ticky));
    }

    public async Task<Result> WriteTimeEntryAsync(TimeEntry entry)
    {
        try
        {
            if (!Directory.Exists(_appDataFolder))
            {
                Directory.CreateDirectory(_appDataFolder);
            }

            var filePath = $"{_appDataFolder}/ticky-{DateTime.Now:yyyy-MM-dd}.csv";

            if (!File.Exists(filePath))
            {
                await File.AppendAllLinesAsync(filePath, [TimeEntry.GetPropNamesCsv()]);
            }

            await File.AppendAllLinesAsync(filePath, [entry.ToCsv()]);

            return Result.Ok();
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e));
        }
    }
}