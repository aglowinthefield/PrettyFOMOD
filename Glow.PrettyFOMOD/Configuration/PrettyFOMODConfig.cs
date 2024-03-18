using ReactiveUI;

namespace Glow.PrettyFOMOD.Configuration;

public class PrettyFomodConfig : ReactiveObject
{
    private string? _modDirectory = "";
    public string? ModDirectory
    {
        get => _modDirectory;
        set => this.RaiseAndSetIfChanged(ref _modDirectory, value);
    }
    
    public bool Test { get; set; }
    public bool UseDummyFileNames { get; set; }
    public bool SmartConditions { get; set;  }
    public bool GenerateFull { get; set; }

    public override string ToString()
    {
        return $"[" +
               $"\n\tWorking Directory = {ModDirectory}" +
               $"\n\tTest = {Test}" +
               $"\n\tUse Dummy Filenames = {UseDummyFileNames}" +
               $"\n\tGenerate Full = {GenerateFull}" +
               $"\n\tGenerate Smart Conditions = {SmartConditions}" +
               $"\n]";
    }
}