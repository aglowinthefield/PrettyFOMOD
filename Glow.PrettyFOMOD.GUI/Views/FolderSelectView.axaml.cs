using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using Glow.PrettyFOMOD.GUI.ViewModels;

namespace Glow.PrettyFOMOD.GUI.Views;

public partial class FolderSelectView : ReactiveUserControl<FolderSelectViewModel> 
{
    public FolderSelectView()
    {
        InitializeComponent();
    }

    private async void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        var window = GetWindow();
        var options = new FolderPickerOpenOptions { Title = "Select mod folder", AllowMultiple = false };
        var folders = await window.StorageProvider.OpenFolderPickerAsync(options);
        if (folders.Count < 1)
        {
            return;
        }
        var folder = folders.ElementAt(0);
        var folderSelectViewModel = ViewModel;
        if (folderSelectViewModel != null) folderSelectViewModel.SelectedFolder = folder.TryGetLocalPath();
    }
    
    Window GetWindow() => this.VisualRoot as Window ?? throw new NullReferenceException("Invalid Owner");
}