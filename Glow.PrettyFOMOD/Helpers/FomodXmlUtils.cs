namespace Glow.PrettyFOMOD.Helpers;

public static class FomodXmlUtils
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

    private static string GetEspFilenameFromPath(string path)
    {
        return path.Split("\\").Last();
    }
}