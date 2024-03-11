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
    
    public static class ElementNames
    {
        public const string Dependencies = "dependencies";
        public const string FileDependency = "fileDependency";
        public const string Pattern = "pattern";
        public const string Patterns = "patterns";
        public const string DependencyType = "dependencyType";
        public const string TypeDescriptor = "typeDescriptor";
        public const string DefaultType = "defaultType";
        public const string Type = "type";
    }
    public static class AttributeNames
    {
        public const string Operator = "operator";
        public const string File = "file";
        public const string State = "state";
        public const string Name = "name";

    }
    public static class AttributeValues
    {
        public const string OperatorAnd = "And";
        public const string FileStateActive = "Active";
        public const string TypeNameOptional = "Optional";
        public const string PatternTypeRecommended = "Recommended";
        public const string PatternTypeOptional = "Optional";
    }
    
}