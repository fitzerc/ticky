using System.Text;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentResults;
using Ticky.Conversion;
using Ticky.Core;
using Ticky.Core.Data;

namespace Ticky;

public partial class MainWindowViewModel : ObservableObject
{
    private bool CanExecuteStart() => _timer is null || !_timer.IsRunning();
    private bool CanExecutePause()
    {
        var shouldEnablePause = _timer is not null && !IsDefaultEntryValues() && _timer.IsRunning();

        PauseToolTipVisibility = shouldEnablePause ? Visibility.Hidden : Visibility.Visible;

        return shouldEnablePause;
    }

    private bool CanExecuteStop() => _timer is not null;

    [ObservableProperty] private string _enteredProject = "Project";
    [ObservableProperty] private string _enteredTask = "Task";
    [ObservableProperty] private string _enteredTag = "Tag";
    [ObservableProperty] private Visibility _pauseToolTipVisibility = Visibility.Visible;

    private TickyTimer? _timer;

    public string FooterText { get; private set; } = "Placeholder Text";
    private string _time = "00:00:00";
    private readonly ITickyDataWriter _dataWriter;

    /// <inheritdoc/>
    public MainWindowViewModel(ITickyDataWriter dataWriter)
    {
        _dataWriter = dataWriter;
        PropertyChanged += (_, args) =>
        {
            switch (args.PropertyName)
            {
                case nameof(EnteredProject):
                case nameof(EnteredTask):
                case nameof(EnteredTag):
                    NotifyExecuteChanged();
                    break;
            }
        };
    }

    public string Time
    {
        get => _time;
        set => SetProperty(ref _time, value);
    }

    //TODO: move this to a setter, so it happens when it needs to?
    private void NotifyExecuteChanged()
    {
        StartCommand.NotifyCanExecuteChanged();
        PauseCommand.NotifyCanExecuteChanged();
        StopCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private async Task ConsolidateExistingFiles()
    {
        await Result.Try(
            _dataWriter.ConsolidateFilesAsync,
            (e) =>
            {
                MessageBox.Show(e.Message);
                return new Error(e.Message).CausedBy(e);
            });

        MessageBox.Show("consolidation success");
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
    private async Task Pause()
    {
        await StopAndWriteAsync(false);
    }

    [RelayCommand(CanExecute = nameof(CanExecuteStop))]
    private async Task Stop()
    {
        await StopAndWriteAsync();
    }

    private async Task StopAndWriteAsync(bool resetEntryDetails = true)
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

                if (resetEntryDetails)
                {
                    EnteredProject = "Project";
                    EnteredTask = "Task";
                    EnteredTag = "Tag";
                }

                break;
        }
    }

    private bool IsDefaultEntryValues()
    {
        return EnteredProject == "Project" || EnteredTask == "Task" || EnteredTag == "Tag";
    }

    private bool ValidateInput()
    {
        var sb = new StringBuilder();

        if (IsDefaultEntryValues())
        {
            sb.Append("Project, task, or tag are default values.");
            sb.Append(Environment.NewLine);
        }

        if (string.IsNullOrEmpty(EnteredProject))
        {
            sb.Append("Project is empty.");
            sb.Append(Environment.NewLine);
        }

        if (string.IsNullOrEmpty(EnteredTask))
        {
            sb.Append("Task is empty.");
            sb.Append(Environment.NewLine);
        }

        if (string.IsNullOrEmpty(EnteredTag))
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
            await _dataWriter.WriteTimeEntryAsync(te);
        }
        catch
        {
            //TODO: handle error
        }
    }
}