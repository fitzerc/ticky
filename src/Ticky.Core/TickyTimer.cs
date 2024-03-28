using System.Diagnostics;
using FluentResults;

namespace Ticky.Core;

public class TickyTimer
{
    private Stopwatch? _activeStopwatch;
    private Timer? _activeTimer;

    private Action<TimeSpan>? _activeOnTick;
    private TimeSpan? _activeDelay;
    private TimeSpan? _activePeriod;

    public bool IsRunning() => _activeTimer is not null;

    public Result Start(Action<TimeSpan> onTick, TimeSpan? delay = null, TimeSpan? period = null)
    {
        try
        {
            _activeStopwatch = Stopwatch.StartNew();
            _activeOnTick = onTick;
            StartTimer(delay, period, _activeOnTick);

            return Result.Ok();
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e));
        }
    }

    private Result StartTimer(TimeSpan? delay, TimeSpan? period, Action<TimeSpan> onTick)
    {
        if (_activeStopwatch is null)
        {
            return Result.Fail(ErrorStrings.NullStopwatchOnStart);
        }

        _activeDelay = delay ?? TickyTimerSettings.Delay;
        _activePeriod = period ?? TickyTimerSettings.Period;

        try
        {
            _activeTimer ??= new Timer(
                _ => onTick(_activeStopwatch.Elapsed),
                null,
                _activeDelay.Value,
                _activePeriod.Value);

            return Result.Ok();
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e));
        }
    }

    public Result Pause()
    {
        if (_activeStopwatch is null)
        {
            return Result.Fail(ErrorStrings.NullStopwatchOnPause);
        }

        try
        {
            _activeStopwatch.Stop();
            ClearTimer();

            return Result.Ok();
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e));
        }
    }

    public Result Unpause()
    {
        if (_activeStopwatch is null)
        {
            return Result.Fail(ErrorStrings.NullStopwatchOnUnpause);
        }

        if (_activeOnTick is null)
        {
            return Result.Fail(ErrorStrings.NullOnTickOnUnpause);
        }

        try
        {
            _activeStopwatch.Start();
            var startTimerResult = StartTimer(_activeDelay, _activePeriod, _activeOnTick);

            if (startTimerResult.IsFailed)
            {
                return startTimerResult;
            }

            return Result.Ok();
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e));
        }
    }

    public Result<TimeSpan> Stop()
    {
        try
        {
            if (_activeStopwatch is null)
            {
                return Result.Fail(ErrorStrings.NullStopwatchOnStop);
            }

            var elapsed = _activeStopwatch.Elapsed;

            ClearStopwatch();
            ClearTimer();

            return Result.Ok(elapsed);
        }
        catch (Exception e)
        {
            ClearTimer();
            return Result.Fail(new ExceptionalError(e));
        }
    }

    private void ClearStopwatch()
    {
        _activeOnTick = null;
        _activeStopwatch?.Stop();
        _activeStopwatch = null;
        _activeDelay = null;
        _activePeriod = null;
    }

    private void ClearTimer()
    {
        _activeTimer?.Dispose();
        _activeTimer = null;
    }

    public static class ErrorStrings
    {
#pragma warning disable CA2211
        public static string NullStopwatchOnStart = "cannot start timer when active stopwatch is null";
        public static string NullStopwatchOnPause = "cannot pause when active stopwatch is null";
        public static string NullStopwatchOnUnpause = "cannot unpause when active stopwatch is null";
        public static string NullOnTickOnUnpause = "cannot unpause when active onTick is null";
        public static string NullStopwatchOnStop = "cannot stop when active stopwatch is null";
#pragma warning restore CA2211
    }
}

public static class TickyTimerSettings
{
    public static TimeSpan Delay { get; set; } = TimeSpan.Zero;
    public static TimeSpan Period { get; set; } = TimeSpan.FromMilliseconds(200);
}