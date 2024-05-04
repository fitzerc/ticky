using System.Windows;
using System.Windows.Controls;
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

    private void TextBoxKeyboardFocusChanged(object sender, RoutedEventArgs e) => ((TextBox)sender).SelectAll();
}