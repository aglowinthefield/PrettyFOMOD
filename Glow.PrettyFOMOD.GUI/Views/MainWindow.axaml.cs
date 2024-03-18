using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace Glow.PrettyFOMOD.GUI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        var storage = this.StorageProvider;
        var options = new FolderPickerOpenOptions() { Title = "Select mod folder", AllowMultiple = false };
        var folderResult = storage.OpenFolderPickerAsync(options);

        if (folderResult.Result.Count < 1)
        {
            return;
        }

        var selectedFolder = folderResult.Result.ElementAt(0);

        // throw new System.NotImplementedException();
    }
}