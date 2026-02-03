using System.Windows;
using SchoolFlow.Desktop.ViewModels;

namespace SchoolFlow.Desktop.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += async (s, e) => await viewModel.OnNavigatedToAsync();
    }
}
