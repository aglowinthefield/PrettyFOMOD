using System.ComponentModel;

namespace Glow.PrettyFOMOD.GUI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public string Greeting => "Welcome to PrettyFOMOD! Select your mod folder to continue.";
    public string SelectedFolder => "";
}