using System.Collections.ObjectModel;
using Glow.PrettyFOMOD.Configuration;
using Glow.PrettyFOMOD.FomodFileIO;
using Glow.PrettyFOMOD.Helpers;

namespace Glow.PrettyFOMOD.CLI;

public class FomodCreator(PrettyFomodConfig config)
{
    private readonly ModuleConfiguration _configuration = new();
    private readonly string _cwd = (config.Test) ? Path.Combine(FomodFileIo.GetCwdPath(), @"test\generator") : FomodFileIo.GetCwdPath();

    public void Run()
    {
        FomodFileIo.SetupEmptyFomodDirectory(config);
        
        // Create info later
        FomodInfoCreator.CreateInfoFile(config);
        
        CreateSkeletonConfiguration();

        // If any esps in this base folder.
        // TODO: Do recursive scan later.
        if (!HasEspInFolder(_cwd)) return;
        
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

    private ModuleConfiguration AddInstallStep(InstallStep installStep)
    {
        _configuration.InstallSteps.InstallStep.Add(installStep);
        return _configuration;
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
        
        var groupPlugins = new PluginList() {
            Order = OrderEnum.Explicit,
            Plugin = new Collection<Plugin>(espPaths.Select(CreatePluginFromEspPath).ToList())
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

    private Plugin CreatePluginFromEspPath(string espPath)
    {
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
                            Source = espPath,
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
                DependencyType = FomodUtils.GenerateRecommendedConditionNodeForMasters(masters.ToList(), []),
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
    
}