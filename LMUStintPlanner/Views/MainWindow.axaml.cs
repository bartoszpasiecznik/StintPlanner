using Avalonia.Controls;
using LMUStintPlanner.ViewModels;

namespace LMUStintPlanner.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
