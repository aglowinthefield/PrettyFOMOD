using Mutagen.Bethesda.Skyrim;

namespace Glow.PrettyFOMOD.Helpers;

public static class FomodUtils
{
    public static PluginTypeDescriptor ResetTypeDescriptorForPluginNode(Plugin pluginNode)
    {
        Console.WriteLine("Removing existing type descriptor node.");
        
        pluginNode.TypeDescriptor = new PluginTypeDescriptor
        {
            DependencyType = new DependencyPluginType()
            {
                Patterns = [],
                DefaultType = new PluginType() { Name = PluginTypeEnum.Optional }
            }
        };
        return pluginNode.TypeDescriptor;
    }
    
    // ReSharper disable once SuggestBaseTypeForParameter
    public static DependencyPluginType GenerateRecommendedConditionNodeForMasters(List<string> masters, HashSet<string> destinationCache)
    {
        /* Create a node within `plugin.typeDescriptor` marking something as Recommended if masters are present
         Example here:
            <typeDescriptor> 
                <dependencyType> 
                    <defaultType name="Optional"/> 
                    <patterns> 
                        <pattern> 
                            <dependencies operator="And"> 
                                <fileDependency file="Amber Guard.esp" state="Active"/> 
                            </dependencies> 
                            <type name="Recommended"/> 
                        </pattern> 
                    </patterns> 
                </dependencyType> 
            </typeDescriptor> 
            
            In past FOMODs I've created a separate condition to disable the checkbox if the plugin is missing,
            but this seems overly restrictive and not really useful to anyone. 
         */
        
        
        // Working from the inside, out. Create a dependencies node with fileDependency children
        var compositeDependency = new CompositeDependency
        {
            Operator = CompositeDependencyOperator.And
        };

        foreach (var master in masters)
        {
            if (destinationCache.Contains(master))
            {
                continue;
            }
            
            var fileDependency = new FileDependency() { File = master, State = FileDependencyState.Active };
            compositeDependency.FileDependency.Add(fileDependency);
        }
        
        // create pattern. this stuff is all kinda hard to memorize and follow. maybe refactor some of the nonsense out
        var pattern = new DependencyPattern()
        {
            Dependencies = compositeDependency,
            Type = new PluginType() { Name = PluginTypeEnum.Recommended }
        };
        
        var dependencyPluginType = new DependencyPluginType()
        {
            Patterns = { pattern },
            DefaultType = new PluginType() { Name = PluginTypeEnum.Optional }
        };
        
        return dependencyPluginType;
    }

    public static List<Plugin> GetPluginNodes(ModuleConfiguration configuration)
    {
        List<Plugin> plugins = [];

        foreach (var installStep in configuration.InstallSteps.InstallStep)
        {
            foreach (var group in installStep.OptionalFileGroups.Group)
            {
                plugins.AddRange(group.Plugins.Plugin);
            }
        }

        return plugins;
    }

    /*
     * Creates a HashSet of ESP names based on the FOMOD plugin>files listing. This can be used to later
     * reference when we build out fileDependency entries (e.g. masters to use for recommendation). Reason being,
     * if we have a FOMOD that install a 'base' mod, then patches for that base mod, we don't want to include the
     * base mod in the fileDependency list because the user won't have installed it yet!
     */
    public static HashSet<string> GenerateFomodDestinationEspCache(ModuleConfiguration configuration)
    {
        var cache = new HashSet<string>();
        
        foreach (var plugin in GetPluginNodes(configuration))
        {
            if (!plugin.FilesSpecified) continue;
            foreach (var pluginFile in plugin.Files)
            {
                foreach (var fileSystemItem in pluginFile.File)
                {
                    cache.Add(GetEspFilenameFromPath(fileSystemItem.Destination));
                }
            }
        }
        return cache;
    }
    
    public static IEnumerable<string> GetMasters(string espPath)
    {
        using var mod = SkyrimMod.CreateFromBinaryOverlay(espPath, SkyrimRelease.SkyrimSE);
        var allMasters = mod.ModHeader.MasterReferences
            .Select(masterReference => masterReference.Master.FileName)
            .Select(dummy => (string)dummy)
            .ToList();
        allMasters.RemoveAll(m => Constants.ExcludedMasters.Contains(m));
        return allMasters;
    }

    public static string GetEspFilenameFromPath(string path)
    {
        return Path.GetFileName(path);
    }
    
    public static bool IsPluginFileName(string fileName)
    {
        return PluginExtensions.Contains(Path.GetExtension(fileName));
    }

    private static readonly string[] PluginExtensions = [".esm", ".esl", ".esp"];
}