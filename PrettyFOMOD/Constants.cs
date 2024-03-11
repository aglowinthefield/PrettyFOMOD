namespace PrettyFOMOD;

public static class Constants
{
    public static class Filenames
    {
        public const string ModuleFile = "ModuleConfig.xml";
        public const string DummyFile = "ModuleDummy.xml";

        public static string BackupFileName()
        {
            var date = DateTime
                .UtcNow
                .ToString("s", System.Globalization.CultureInfo.InvariantCulture)
                .Replace(":", "-"); 
            return $"ModuleConfigBackup-{date}.xml";
        }
    }
}