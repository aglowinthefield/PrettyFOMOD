using System.Xml;
using System.Xml.Serialization;
using Glow.PrettyFOMOD.CLI;
using Glow.PrettyFOMOD.Configuration;
using Glow.PrettyFOMOD.Helpers;

namespace Glow.PrettyFOMOD.FomodFileIO;

public static class FomodFileIo
{
    public static string GetFomodPath(PrettyFomodConfig config)
    {
        // For ease of testing in an IDE, I nest resources in the CWD/test folder. Set your run arg to -test if debugging 
        var filePath = config.Test
            ? Path.Combine(GetCwdPath(config), @"test\fomod")
            : Path.Combine(GetCwdPath(config), "fomod");
        
        if (config is { GenerateFull: true, Test: true })
        {
            filePath = filePath.Replace(@"test\fomod", @"test\generator\fomod");
        }

        return filePath;
    }
    
    public static ModuleConfiguration OpenFomodFile(string fomodDirectoryPath)
    {
        var doc = new XmlDocument();
        doc.Load(Path.Combine(fomodDirectoryPath, "ModuleConfig.xml"));

        var serializer = new XmlSerializer(typeof(ModuleConfiguration));

        using XmlReader reader = new XmlNodeReader(doc);
        return (ModuleConfiguration)serializer.Deserialize(reader)!;
    }
    
    public static void SetupEmptyFomodDirectory(PrettyFomodConfig config)
    {
        var directory = config.Test
                                    ? Path.Combine(GetCwdPath(config), @"test\generator\fomod")
                                    : Path.Combine(GetCwdPath(config), "fomod");
        CliUtils.WriteStepwiseText($"Creating FOMOD directory in path: {directory}");
        Directory.CreateDirectory(directory);
    }

    public static FomodInfo OpenFomodInfoFile(PrettyFomodConfig config)
    {
        var filename = config.UseDummyFileNames ? Constants.Filenames.DummyInfoFile : Constants.Filenames.InfoFile;
        var path = Path.Combine(GetFomodPath(config), filename);

        if (!File.Exists(path))
        {
            return new FomodInfo();
        }
        
        var doc = new XmlDocument();
        try
        {
            doc.Load(path);
        }
        catch (Exception)
        {
            CliUtils.WriteStepwiseText("No FOMOD info file found or invalid format. Starting from scratch.\n");
            return new FomodInfo();
        }

        var serializer = new XmlSerializer(typeof(FomodInfo));
        using XmlReader reader = new XmlNodeReader(doc);
        return (FomodInfo)serializer.Deserialize(reader)!;
    }
    
    public static void BackupModuleConfig(string fomodPath)
    {
        CliUtils.PrintSeparator();
        var fomodFilePath = Path.Combine(fomodPath, Constants.Filenames.ModuleFile);
        
        var backupPath = fomodFilePath.Replace(
            Constants.Filenames.ModuleFile,
            Constants.Filenames.BackupFileName());
        
        CliUtils.WriteStepwiseText($"Backing up {Constants.Filenames.ModuleFile} to " + backupPath);
        File.Copy(fomodFilePath, backupPath, false);
    }
    
    public static void SaveFomod(ModuleConfiguration moduleConfiguration, PrettyFomodConfig config)
    {
        CliUtils.PrintSeparator();

        var fomodPath = GetFomodPath(config);

        var filePath = config.UseDummyFileNames
            ? Path.Combine(fomodPath, Constants.Filenames.DummyModuleFile)
            : Path.Combine(fomodPath, Constants.Filenames.ModuleFile);
        
        // TODO: Dunno if this is totally wise.
        CliUtils.WriteStepwiseText($"Deleting existing FOMOD (it's backed up) at path: {filePath}.");
        File.Delete(filePath);
        
        var serializer = new XmlSerializer(typeof(ModuleConfiguration));
        using (var writer = new StreamWriter(filePath))
        {
            serializer.Serialize(writer, moduleConfiguration);
        }
        
        CliUtils.WriteStepwiseText($"FOMOD saved to {filePath}");
    }

    // private static void SaveFile<T>(T file, PrettyFomodConfig config)
    // {
    //     if (file == null) throw new ArgumentNullException(nameof(file));
    //     
    //     if (file.GetType() == typeof(FomodInfo))
    //     {
    //         
    //     }
    // }

    // TODO Make this generic sometime.
    public static void SaveFomodInfo(FomodInfo fomodInfo, PrettyFomodConfig config)
    {
        
        CliUtils.PrintSeparator();
        Console.WriteLine("Saving FOMOD info file");

        var fomodPath = GetFomodPath(config);
        
        var filePath = config.UseDummyFileNames
            ? Path.Combine(fomodPath, Constants.Filenames.DummyInfoFile)
            : Path.Combine(fomodPath, Constants.Filenames.InfoFile);
        
        // TODO: Dunno if this is totally wise.
        // File.Delete(filePath);
        
        var serializer = new XmlSerializer(typeof(FomodInfo));
        using (var writer = new StreamWriter(filePath, append: false))
        {
            serializer.Serialize(writer, fomodInfo);
        }
        
        Console.WriteLine($"FOMOD Info saved to {filePath}");
    }
    
    public static string GetCwdPath(PrettyFomodConfig config)
    {
        var cwdPath = config.Test
            ? Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName!
            : Directory.GetCurrentDirectory();
        return cwdPath;
    }
}