using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Platform.Storage;
using Glow.PrettyFOMOD.GUI.ViewModels;

namespace Glow.PrettyFOMOD.GUI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        var options = new FolderPickerOpenOptions { Title = "Select mod folder", AllowMultiple = false };
        var folders = await this.StorageProvider.OpenFolderPickerAsync(options);
        if (folders.Count < 1)
        {
            return;
        }
        var folder = folders.ElementAt(0);
        ((DataContext as MainWindowViewModel)!).SelectedFolder = folder.TryGetLocalPath();
    }
}