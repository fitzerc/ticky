using System.Text;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ticky.Core;
using Ticky.Core.Data;

namespace Ticky;

public partial class MainWindowViewModel(ITickyDataWriter dataWriter) : ObservableObject
{
    private bool CanExecuteStart() => _timer is null || !_timer.IsRunning();
    private bool CanExecutePause() => _timer is not null && _timer.IsRunning();
    private bool CanExecuteStop() => _timer is not null;

    [ObservableProperty] private string _enteredProject = "Project";
    [ObservableProperty] private string _enteredTask = "Task";
    [ObservableProperty] private string _enteredTag = "Tag";

    private TickyTimer? _timer;

    public string FooterText { get; private set; } = "Placeholder Text";
    private string _time = "00:00:00";

    public string Time
    {
        get => _time;
        set => SetProperty(ref _time, value);
    }

    //TODO: move this to a setter so it happens when it needs to?
    private void NotifyExecuteChanged()
    {
        StartCommand.NotifyCanExecuteChanged();
        PauseCommand.NotifyCanExecuteChanged();
        StopCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteStart))]
    private void Start()
    {
        switch (_timer)
        {
            case null:
                _timer = new TickyTimer();
                _ = _timer.Start(OnTick);
                break;
            case var t when !t.IsRunning():
                _ = _timer.Unpause();
                break;
        }

        NotifyExecuteChanged();
    }

    private void OnTick(TimeSpan ts) => Time = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}";

    [RelayCommand(CanExecute = nameof(CanExecutePause))]
    private void Pause()
    {
        _ = _timer?.Pause();
        NotifyExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteStop))]
    private async Task Stop()
    {
        //Pause in case we need to wait for user input
        _ = _timer?.Pause();
        NotifyExecuteChanged();

        var inputValidated = ValidateInput();

        if (!inputValidated)
        {
            return;
        }

        var stopTimerResult = _timer?.Stop();
        NotifyExecuteChanged();

        switch (stopTimerResult)
        {
            case null:
                //TODO: handle StopTimer error
                break;
            case not null:
                _timer = null;

                var te = new TimeEntry(
                    EnteredProject,
                    EnteredTask,
                    EnteredTag,
                    stopTimerResult.Value);

                await CommitEntryAsync(te);

                EnteredProject = "Project";
                EnteredTask = "Task";
                EnteredTag = "Tag";

                break;
        }
    }

    private bool ValidateInput()
    {
        var sb = new StringBuilder();

        if (_enteredProject == "Project" || _enteredTask == "Task" || _enteredTag == "Tag")
        {
            sb.Append("Project, task, or tag are default values.");
            sb.Append(Environment.NewLine);
        }

        if (string.IsNullOrEmpty(_enteredProject))
        {
            sb.Append("Project is empty.");
            sb.Append(Environment.NewLine);
        }

        if (string.IsNullOrEmpty(_enteredTask))
        {
            sb.Append("Task is empty.");
            sb.Append(Environment.NewLine);
        }

        if (string.IsNullOrEmpty(_enteredTag))
        {
            sb.Append("Task is empty.");
            sb.Append(Environment.NewLine);
        }

        var validationError = sb.ToString();

        if (!string.IsNullOrEmpty(validationError))
        {
            var result = MessageBox.Show(
                validationError + "Do you want to save time entry as is?", // The text to display
                "Confirmation", // The title of the message box
                MessageBoxButton.YesNo, // The buttons to show
                MessageBoxImage.Question); // The icon to show

            if (result == MessageBoxResult.Yes)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    private async Task CommitEntryAsync(TimeEntry te)
    {
        try
        {
            await dataWriter.WriteTimeEntryAsync(te);
        }
        catch
        {
            //TODO: handle error
        }
    }
}