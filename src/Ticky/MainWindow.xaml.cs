using System.Windows;
using System.Windows.Controls;
using Ticky.Conversion;
using Ticky.DataAccess;

namespace Ticky;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        //TODO: move to DI container
        var dataWriter = new FileDataWriter();
        var viewModel = new MainWindowViewModel(dataWriter, new VersionConverter(dataWriter));
        DataContext = viewModel;
    }

    private void TextBoxKeyboardFocusChanged(object sender, RoutedEventArgs e) => ((TextBox)sender).SelectAll();
}