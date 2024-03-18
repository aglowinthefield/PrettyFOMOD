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

    private List<string> _allEsps = [];                             // Full paths of every ESP in the directory
    private List<string> _allEspFileNames = [];                     // Filename of every ESP in the directory
    private ISkyrimModDisposableGetter[] _allMods = [];             // Mutagen representation of every ESP in the directory
    private ISkyrimModDisposableGetter[] _consistencyPatches = [];  // Patches with 2+ masters in the same mod

    public void Run()
    {
        FomodFileIo.SetupEmptyFomodDirectory(config);
        
        var fomodInfo = CreateInfoFile(config);
        CliUtils.WriteStepwiseText($"Using current working directory: {_cwd}");
        
        CreateSkeletonConfiguration(fomodInfo);

        // TODO: Do recursive scan later.
        if (!HasEspInFolder(_cwd)) return;
        
        // Gather all ESPs and mods
        _allEsps = Directory.GetFiles(_cwd).Where(FomodUtils.IsPluginFileName).ToList();
        _allEspFileNames = _allEsps.Select(Path.GetFileName).ToList()!;
        _allMods = PopulateMods();
        
        // create install step for this folder
        AddInstallStep(new InstallStep()
        {
            Name = "General",
            OptionalFileGroups = CreateGroupsFromEspPaths(_allEsps) 
        });
        
        FomodFileIo.SaveFomod(_configuration, config);
    }

    private void CreateSkeletonConfiguration(FomodInfo fomodInfo)
    {
        _configuration.ModuleName = new ModuleTitle { Value = fomodInfo.Name };
        _configuration.InstallSteps = new StepList();
        _configuration.ModuleImage = new HeaderImage() { Path = "" };
    }

    private ISkyrimModDisposableGetter[] PopulateMods()
    {
        return _allEsps.Select(path => SkyrimMod.CreateFromBinaryOverlay(path, SkyrimRelease.SkyrimSE)).ToArray();
    }

    private void AddInstallStep(InstallStep installStep)
    {
        _configuration.InstallSteps.InstallStep.Add(installStep);
    }

    private static bool HasEspInFolder(string path)
    {
        var files = Directory.GetFiles(path);
        var hasPlugins = files.Any(FomodUtils.IsPluginFileName);
        return hasPlugins;
    }

    private GroupList CreateGroupsFromEspPaths(IEnumerable<string> espPaths)
    {
        
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

    /*
     * 
     */
    private ISkyrimModDisposableGetter[] GetConsistencyPatches()
    {
        // select mods where two or more of their masters are mods within the set
        // i.e. lux - jks bannered mare, lux - eeks whiterun, *LUX - JK EEK BANNERED MARE*
        return _allMods.Where(mod =>
            mod
                .ModHeader
                .MasterReferences
                .Count(master => _allEspFileNames.Contains(master.Master.FileName)) > 2)
            .ToArray();
    }

    /*
     * Smart Organization is a heuristic-based feature to group all ESPs, regardless of where they're located in the
     * folder structure, by things like name or common masters. This is an experimental feature that might blow up the
     * scope of this utility but worth documenting so I don't forget.
     *
     * Another option would be to somehow look up the ESP category using the Nexus API to create groups that way.
     *
     * A third option would be to just give the user control over how things are organized, but again that would certainly
     * blow up the scope of this tool and make me liable for some pretty sh*tty looking FOMODs lol. Doing this via an expressive
     * declared folder structure would probably be a reasonable compromise between manual organization and time-saving.
     */
    private void DoSmartOrganization()
    {
        
    }
    
    /*
     * FOMODs will be divided according to the folder hierarchy provided in the CWD
     *
     * Base directory
     * |---- [...ESP files]               == 'General' step, group, plugins
     * |---- subDirectory/                == create step for subdirectory
     * |-------- [...ESP files]           == create plugins for step w/ generic groups
     * |-------- subDirectory/            == create group for step
     * |------------ [...ESP files]       == create plugin for group
     * |------------ subDirectory/        == ???
     *
     * TODO: Come up with a way to make sure we aren't using folders like 'textures/' and 'meshes/' as steps. Generally,
     * this project needs some wisdom around how to handle asset organization via FOMOD. Look for some good examples somewhere.
     */
    
    private void ParseForDirectory(string dirPath, int directoryLevel)
    {
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
                            Destination = "", // No reason to have any specific destinations. Plugins resolve to data/
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

    private static FomodInfo CreateInfoFile(PrettyFomodConfig config)
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
        return fomodInfo;
    }
}