using System.Xml;

namespace PrettyFOMOD;

public class FomodXmlUtils
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
}