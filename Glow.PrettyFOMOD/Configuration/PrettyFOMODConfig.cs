namespace Glow.PrettyFOMOD.Configuration;

public class PrettyFomodConfig
{
    public bool Test { get; set;  }
    public bool SmartConditions { get; set;  }
    public bool GenerateFull { get; set; }

    public override string ToString()
    {
        return $"[\n\t" +
               $"Test = {Test}" +
               $"Generate Full = {GenerateFull}" +
               $"Generate Smart Conditions = {SmartConditions}" +
               $"\n]";
    }
}