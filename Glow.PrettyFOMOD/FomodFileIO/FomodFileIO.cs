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
        var filePath = config.Test ? Path.Combine(GetCwdPath()!, @"test\fomod") : Path.Combine(GetCwdPath()!, "fomod");
        
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
        Directory.CreateDirectory(config.Test
            ? Path.Combine(GetCwdPath()!, @"test\generator\fomod")
            : Path.Combine(GetCwdPath()!, "fomod"));
    }

    public static FomodInfo OpenFomodInfoFile(PrettyFomodConfig config)
    {
        var filename = config.Test ? Constants.Filenames.DummyInfoFile : Constants.Filenames.InfoFile;
        var path = Path.Combine(GetFomodPath(config), filename);

        if (!File.Exists(path))
        {
            File.Create(path);
            return new FomodInfo();
        }
        
        var doc = new XmlDocument();
        try
        {
            doc.Load(path);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error parsing FOMOD info file. Starting from scratch.");
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
        
        Console.WriteLine($"Backing up {Constants.Filenames.ModuleFile} to " + backupPath);
        File.Copy(fomodFilePath, backupPath, false);
    }
    
    public static void SaveFomod(ModuleConfiguration moduleConfiguration, PrettyFomodConfig config)
    {
        CliUtils.PrintSeparator();
        Console.WriteLine("Saving FOMOD. Here goes nothin'!");

        var fomodPath = GetFomodPath(config);

        var filePath = config.UseDummyFileNames
            ? Path.Combine(fomodPath, Constants.Filenames.DummyModuleFile)
            : Path.Combine(fomodPath, Constants.Filenames.ModuleFile);
        
        // TODO: Dunno if this is totally wise.
        File.Delete(filePath);
        
        var serializer = new XmlSerializer(typeof(ModuleConfiguration));
        using (var writer = new StreamWriter(filePath))
        {
            serializer.Serialize(writer, moduleConfiguration);
        }
        
        Console.WriteLine($"FOMOD saved to {filePath}");
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
        File.Delete(filePath);
        
        var serializer = new XmlSerializer(typeof(FomodInfo));
        using (var writer = new StreamWriter(filePath))
        {
            serializer.Serialize(writer, fomodInfo);
        }
        
        Console.WriteLine($"FOMOD Info saved to {filePath}");
    }
    
    public static string GetCwdPath()
    {
        return Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName!;
    }
}