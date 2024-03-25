using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ticky.Core;

namespace Ticky;

public partial class MainWindowViewModel : ObservableObject
{
    private TickyTimer? _timer;

    public string FooterText { get; private set; } = "Placeholder Text";

    private string _time = "00:00:00";
    public string Time
    {
        get => _time;
        set => SetProperty(ref _time, value);
    }

    [RelayCommand]
    private void Start()
    {
        if (_timer is null)
        {
            TickyTimerSettings.Period = TimeSpan.FromSeconds(5);
            _timer = new TickyTimer();

            var startResult = _timer.Start(OnTick);
            if (startResult.IsFailed)
            {
                //TODO: handle TimerStart error
            }
        }
        else if (!_timer.IsRunning())
        {
            var unpauseResult = _timer.Unpause();
            if (unpauseResult.IsFailed)
            {
                //TODO: handle TimerUnpause error
            }
        }
    }

    private void OnTick(TimeSpan? ts)
    {
        if (ts.HasValue)
        {
            Time = $"{ts.Value.Hours:00}:{ts.Value.Minutes:00}:{ts.Value.Seconds:00}";
        }
    }

    [RelayCommand]
    private void Pause()
    {
        if (_timer is null)
        {
            //TODO: handle pause _timer is null error
            return;
        }

        var timerPauseResult = _timer.Pause();
        if (timerPauseResult.IsFailed)
        {
            //TODO: handle TimerPause error
        }
    }

    [RelayCommand]
    private void Stop()
    {
        if (_timer is null)
        {
            //TODO: handle stop _timer is null error
            return;
        }

        var stopTimerResult = _timer.Stop();
        if (stopTimerResult.IsFailed)
        {
            //TODO: handle StopTimer error
        }

        _timer = null;
    }
}