using System.Windows;
using Ticky.DataAccess;

namespace Ticky;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        //TODO: move to DI container
        var viewModel = new MainWindowViewModel(new FileDataWriter());
        DataContext = viewModel;
    }
}