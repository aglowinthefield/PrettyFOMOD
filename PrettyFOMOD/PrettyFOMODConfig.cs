namespace PrettyFOMOD;

public class PrettyFomodConfig
{
    public bool Test { get; set;  }
    public bool SmartConditions { get; set;  }
    public bool GenerateFull { get; set; }

    public override string ToString()
    {
        return $"[\n\t" +
               $"Test = {this.Test}" +
               $"\n]";
    }
}