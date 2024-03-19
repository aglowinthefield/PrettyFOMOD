using System.Diagnostics;
using System.Xml;
using System.Xml.Serialization;
using Glow.PrettyFOMOD.FomodFileIO;
using Glow.PrettyFOMOD.Helpers;

namespace Glow.PrettyFOMOD.Library;

public class FOMOD
{
    /*
     * Internal representation of FOMOD -- serialized to and from XML.
     */
    private string _modFolder = null!;

    private bool _existedPriorToInit = false;
    private string _fomodFolder = null!;
    private EspFile[] _espFiles = null!;

    public FomodInfo Info { get; set; }
    public ModuleConfiguration ModuleConfiguration { get; set; }
    
    /*
     * Initialize with a directory path
     */
    public FOMOD()
    {
        _espFiles = [];
        _modFolder = "";
        _fomodFolder = "";
        Info = new FomodInfo();
        ModuleConfiguration = new ModuleConfiguration();
    }
    public FOMOD(string modFolder)
    {
        Init(modFolder);
    }

    public void Init(string modFolder)
    {
        _modFolder = modFolder;
        
        // If directory has FOMOD already, read it into _fomod.
        // Otherwise create skeleton configuration into _fomod.
        _fomodFolder = InitializeDirectoryIfNotExists();
        Info = InitializeFomodInfoFile();
        ModuleConfiguration = InitializeFomodModuleConfigurationFile();
        
        // Scan for ESPs in every folder except 'fomod' of course.
        _espFiles = ScanForEspFiles();
    }

    public bool DidExistPriorToInit()
    {
        return _existedPriorToInit;
    }

    #region Initialization
    private string InitializeDirectoryIfNotExists()
    {
        var fomodFolderPath = Path.Combine(_modFolder, "fomod");
        Directory.CreateDirectory(fomodFolderPath);
        return fomodFolderPath;
    }

    // TODO: This is copied from the CLI tool so find a way to consolidate in a refactor.
    private FomodInfo InitializeFomodInfoFile()
    {
        var infoPath = Path.Combine(_fomodFolder, Constants.Filenames.InfoFile);
        if (!File.Exists(infoPath))
        {
            return new FomodInfo();
        }

        var doc = new XmlDocument();
        try
        {
            doc.Load(infoPath);
        }
        catch (Exception)
        {
            Debug.WriteLine("Couldn't read FOMOD info");
            return new FomodInfo();
        }
        
        var serializer = new XmlSerializer(typeof(FomodInfo));
        using XmlReader reader = new XmlNodeReader(doc);
        
        Debug.WriteLine("FOMODInfo successfully deserialized from XML file.");
        _existedPriorToInit = true;
        
        return (FomodInfo)serializer.Deserialize(reader)!;
    }

    private ModuleConfiguration InitializeFomodModuleConfigurationFile()
    {
        var modulePath = Path.Combine(_fomodFolder, Constants.Filenames.ModuleFile);
        if (!File.Exists(modulePath))
        {
            return new ModuleConfiguration();
        }

        var doc = new XmlDocument();
        try
        {
            doc.Load(modulePath);
        }
        catch (Exception)
        {
            Debug.WriteLine("Couldn't read FOMOD");
            return new ModuleConfiguration();
        }
        var serializer = new XmlSerializer(typeof(ModuleConfiguration));
        using XmlReader reader = new XmlNodeReader(doc);
        _existedPriorToInit = true;
        return (ModuleConfiguration)serializer.Deserialize(reader)!;
    }

    private EspFile[] ScanForEspFiles()
    {
        var files = Directory
            .GetFiles(_modFolder, "*", SearchOption.AllDirectories)
            .Where(FomodUtils.IsPluginFileName)
            .ToArray();
        return files.Select(file => new EspFile(file)).ToArray();
    }
    
    #endregion
}