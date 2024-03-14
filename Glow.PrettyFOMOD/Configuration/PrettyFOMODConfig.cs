namespace Glow.PrettyFOMOD.Configuration;

public class PrettyFomodConfig
{
    public bool Test { get; set; }
    public bool UseDummyFileNames { get; set; }
    public bool SmartConditions { get; set;  }
    public bool GenerateFull { get; set; }

    public override string ToString()
    {
        return $"[\n\t" +
               $"Test = {Test}" +
               $"\n\tUse Dummy Filenames = {UseDummyFileNames}" +
               $"\n\tGenerate Full = {GenerateFull}" +
               $"\n\tGenerate Smart Conditions = {SmartConditions}" +
               $"\n]";
    }
}