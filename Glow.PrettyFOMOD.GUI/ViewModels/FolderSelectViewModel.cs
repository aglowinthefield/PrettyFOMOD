using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI;

namespace Glow.PrettyFOMOD.GUI.ViewModels;

public class FolderSelectViewModel : ViewModelBase
{

    public string Greeting => "Welcome to PrettyFOMOD! Select your mod folder to continue.";

    private string? _SelectedFolder;

    public string? SelectedFolder
    {
        get => _SelectedFolder;
        set => this.RaiseAndSetIfChanged(ref _SelectedFolder, value);
    }
    
}