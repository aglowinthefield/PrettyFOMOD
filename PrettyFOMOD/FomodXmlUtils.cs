using System.Xml;

namespace PrettyFOMOD;

public static class FomodXmlUtils
{
    public static XmlNode ResetTypeDescriptorForPluginNode(XmlNode pluginNode)
    {
            var ownerDoc = pluginNode.OwnerDocument!;
            var typeDescriptorNode = pluginNode.SelectSingleNode(Constants.ElementNames.TypeDescriptor);
            
            if (typeDescriptorNode == null)
            {
                typeDescriptorNode = ownerDoc.CreateElement(Constants.ElementNames.TypeDescriptor);
                pluginNode.AppendChild(typeDescriptorNode);
            }
            
            Console.WriteLine("Removing existing type descriptor node.");
            typeDescriptorNode.RemoveAll(); // Clear contents of existing <typeDescriptor />
            Console.WriteLine("Generating conditions from listed ESP masters.");
            return typeDescriptorNode;
    }

    public static List<XmlNode> GetPluginNodes(XmlDocument document)
    {
        List<XmlNode> pluginNodes = [];
        // find "config" childnode and return its child nodes
        if (document.DocumentElement == null) throw new Exception("Malformed XML");
        var installSteps = document.DocumentElement.SelectSingleNode("/config/installSteps")?.ChildNodes;

        if (installSteps == null)
        {
            throw new Exception("Malformed XML");
        }
        
        // TODO: This is like CS101 style iteration, clean this up later.
        // I'm no expert at LINQ expressions, so Rider's conversion offer is tempting but I'll pass for now.
        foreach (XmlNode installStep in installSteps)
        {
            var groups = installStep.ChildNodes[0]!.ChildNodes;
            foreach (XmlNode group in groups)
            {
                var plugins = group.ChildNodes[0];
                foreach (XmlNode plugin in plugins!)
                {
                    pluginNodes.Add(plugin);
                }
            }
        }

        return pluginNodes;
    }

    /*
     * Creates a HashSet of ESP names based on the FOMOD plugin>files listing. This can be used to later
     * reference when we build out fileDependency entries (e.g. masters to use for recommendation). Reason being,
     * if we have a FOMOD that install a 'base' mod, then patches for that base mod, we don't want to include the
     * base mod in the fileDependency list because the user won't have installed it yet!
     */
    public static HashSet<string> GenerateFomodSourceESPCache(XmlDocument document)
    {
        var cache = new HashSet<string>();

        var pluginNodes = GetPluginNodes(document);
        foreach (var pluginNode in pluginNodes)
        {
            // pluginNode.
        }

        return cache;
    }
}