namespace Glow.PrettyFOMOD.GUI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public FolderSelectViewModel FolderSelectViewModel { get; } = new FolderSelectViewModel();

}