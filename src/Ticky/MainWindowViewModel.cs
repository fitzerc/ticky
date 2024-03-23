using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Ticky;

public partial class MainWindowViewModel : ObservableObject
{
    private Stopwatch? _activeStopwatch = null;
    private Timer? _activeTimer = null;

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
        if (_activeStopwatch is not null && _activeStopwatch.IsRunning)
        {
            return;
        }

        _activeStopwatch ??= new();
        _activeStopwatch.Start();
        StartTimer();
    }

    private void StartTimer()
        => _activeTimer ??= new Timer(_ => OnTick(_activeStopwatch.Elapsed), null, TimeSpan.Zero, TimeSpan.FromMilliseconds(200));

    private void OnTick(TimeSpan ts)
        => Time = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}";

    [RelayCommand]
    private void Pause()
    {
        _activeStopwatch?.Stop();
        ClearTimer();
    }

    [RelayCommand]
    private void Stop()
    {
        _activeStopwatch?.Stop();
        _activeStopwatch = null;
        ClearTimer();
    }

    private void ClearTimer()
    {
        _activeTimer?.Dispose();
        _activeTimer = null;
    }
}