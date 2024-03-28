using FluentResults;
using Ticky.Core;
using Ticky.Core.Data;

namespace Ticky.Data;

public class FileDataWriter : ITickyDataWriter
{
    public async Task<Result> WriteTimeEntryAsync(TimeEntry entry)
    {
        throw new NotImplementedException();
    }
}