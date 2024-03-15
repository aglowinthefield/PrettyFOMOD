namespace Glow.PrettyFOMOD.Helpers;

public static class Constants
{
    public static class Filenames
    {
        public const string ModuleFile = "ModuleConfig.xml";
        public const string DummyModuleFile = "ModuleDummy.xml";

        public const string InfoFile = "info.xml";
        public const string DummyInfoFile = "infoDummy.xml";

        public static string BackupFileName()
        {
            var date = DateTime
                .UtcNow
                .ToString("s", System.Globalization.CultureInfo.InvariantCulture)
                .Replace(":", "-"); 
            return $"ModuleConfigBackup-{date}.xml";
        }
        
    }
    public static readonly string[] ExcludedMasters =
    [
        "Skyrim.esm",
        "Update.esm",
        "HearthFires.esm",
        "Dragonborn.esm",
        "Dawnguard.esm",
        "ccBGSSSE001-Fish.esm",
        "ccQDRSSE001-SurvivalMode.esl",
        "ccBGSSSE037-Curios.esl",
        "ccBGSSSE025-AdvDSGS.esm",
    ];
}