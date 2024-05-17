using FluentResults;

namespace Ticky.Core.Data;

public interface ITickyDataWriter
{
    Task<Result> WriteTimeEntryAsync(TimeEntry entry, DateTime? overrideFileDateWith = null);
    Task<Result> ConsolidateFilesAsync();
}