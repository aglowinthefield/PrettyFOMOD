using System.Collections.ObjectModel;
using Glow.PrettyFOMOD.Configuration;
using Glow.PrettyFOMOD.FomodFileIO;
using Glow.PrettyFOMOD.Helpers;
using Sharprompt;

namespace Glow.PrettyFOMOD.CLI;

public class FomodCreator(PrettyFomodConfig config)
{
    private readonly ModuleConfiguration _configuration = new();
    private readonly string _cwd = (config.Test)
        ? Path.Combine(FomodFileIo.GetCwdPath(config), @"test\generator")
        : FomodFileIo.GetCwdPath(config);

    public void Run()
    {
        
        FomodFileIo.SetupEmptyFomodDirectory(config);
        
        // Create info later
        CreateInfoFile(config);
        CliUtils.WriteStepwiseText($"Using current working directory: {_cwd}");
        
        CreateSkeletonConfiguration();

        // If any ESPs in this base folder.
        // TODO: Do recursive scan later.
        if (!HasEspInFolder(_cwd)) return;
        
        // Generate cache of possibly installed masters from these sources to avoid the blocked master problem.
        
        // create install step for this folder
        AddInstallStep(new InstallStep()
        {
            Name = "General",
            OptionalFileGroups = CreateGroupsFromBaseDirectory() 
        });
        
        FomodFileIo.SaveFomod(_configuration, config);
    }

    private void CreateSkeletonConfiguration()
    {
        _configuration.ModuleName = new ModuleTitle { Value = "" };
        _configuration.InstallSteps = new StepList();
        _configuration.ModuleImage = new HeaderImage() { Path = "" };
    }

    private void AddInstallStep(InstallStep installStep)
    {
        _configuration.InstallSteps.InstallStep.Add(installStep);
    }

    public void Save()
    {
        FomodFileIo.SaveFomod(_configuration, config);
    }

    private static bool HasEspInFolder(string path)
    {
        var files = Directory.GetFiles(path);
        var hasPlugins = files.Any(FomodUtils.IsPluginFileName);
        return hasPlugins;
    }

    private GroupList CreateGroupsFromBaseDirectory()
    {
        // this is where we might want to group by common master or some similar heuristic
        
        
        // Gather all ESPs
        var espPaths = Directory.GetFiles(_cwd).Where(FomodUtils.IsPluginFileName);
        
        // generate the cache here
        HashSet<string> fomodProvidedMasterEspCache = [];
        var enumerable = espPaths.ToList();
        foreach (var espPath in enumerable)
        {
            fomodProvidedMasterEspCache.Add(FomodUtils.GetEspFilenameFromPath(espPath));
        }
        
        var groupPlugins = new PluginList() {
            Order = OrderEnum.Explicit,
            Plugin = new Collection<Plugin>(enumerable
                .Select(path => CreatePluginFromEspPath(path, fomodProvidedMasterEspCache))
                .ToList())
        };
        
        var groupList = new GroupList()
        {
            Order = OrderEnum.Explicit,
            Group =
            {
                new Group()
                {
                    Name = "General",
                    Type = GroupType.SelectAny,
                    Plugins = groupPlugins
                }
            }
        };
        return groupList;
    }

    private Plugin CreatePluginFromEspPath(string espPath, HashSet<string> fomodMasterCache)
    {
        
        var relativeEspPath = Path.GetRelativePath(_cwd, espPath);
        var masters = FomodUtils.GetMasters(espPath);
        return new Plugin()
        {
            Name = NormalizePluginName(espPath),
            Description = "",
            Files = {
                new FileList()
                {
                    File =
                    {
                        new FileSystemItem()
                        {
                            Source = relativeEspPath,
                            Destination = FomodUtils.GetEspFilenameFromPath(espPath)
                        }
                    }
                }
            },
            Image = null,
            TypeDescriptor = new PluginTypeDescriptor()
            {
                Type = new PluginType()
                {
                    Name = PluginTypeEnum.Optional // This is just boilerplate stuff AFAIK.
                },
                // NOTE: we aren't using a cache now but we probably want to recursively scan mods to make sure
                // we don't encounter clashing masters with installed plugins from the same mod.
                DependencyType = FomodUtils.GenerateRecommendedConditionNodeForMasters(masters.ToList(), fomodMasterCache),
            }
        };
    }

    private static string NormalizePluginName(string espPath)
    {
        var patchName = FomodUtils
            .GetEspFilenameFromPath(espPath)
            .Replace(Path.GetExtension(espPath), "");
        
        // TODO: This for now just returns the filename without an extension. Smartly decoupling it from, say, master names could be interesting. 
        return patchName;
    }

    private static void CreateInfoFile(PrettyFomodConfig config)
    {
        CliUtils.WriteHeaderText("Generating info.xml. This is just metadata for your mod.");
        
        var fomodInfo = FomodFileIo.OpenFomodInfoFile(config);
        fomodInfo.Name =        Prompt.Input<string>("Mod Name: ", defaultValue: fomodInfo.Name);
        fomodInfo.Author =      Prompt.Input<string>("Author: ", defaultValue: fomodInfo.Author);
        fomodInfo.Description = Prompt.Input<string>("Description: ", defaultValue: fomodInfo.Description);
        fomodInfo.Website =     Prompt.Input<string>("Website: ", defaultValue: fomodInfo.Website);
        fomodInfo.Version =     Prompt.Input<string>("Version: ", defaultValue: fomodInfo.Version);
        
        // TODO: Figure out groups.
        FomodFileIo.SaveFomodInfo(fomodInfo, config);
    }
}