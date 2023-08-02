using System.Windows;
using System.Windows.Input;
using ModManager.ViewModels;

namespace ModManager;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = InstallationViewModel.Instance;
    }

    private void OnExitPressed(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            Wnd.DragMove();
    }
}