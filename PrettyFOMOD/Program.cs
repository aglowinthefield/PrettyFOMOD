using System.Xml;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;

namespace PrettyFOMOD
{
    class Program
    {
        static void Main(string[] args)
        {
            Warmup.Init();
            Console.WriteLine("Hello, World. We're warmed up.");
            
            var config = GetConfig(args);
            Console.WriteLine("Arguments parsed: " + config);

            // For ease of testing in an IDE, I nest resources in the CWD/test folder. Set your run arg to -test if debugging 
            var fomodPath = config.Test
                ? Path.Combine(GetCwdPath()!, "test\\fomod")
                : Path.Combine(GetCwdPath()!, "fomod");

            if (config.Test)
            {
                RemoveTestDocument(fomodPath);
            }
            
            var doc = OpenFomodFile(fomodPath);

            var steps = GetInstallSteps(doc);
            for (var i = 0; i < steps.Count; i++)
            {
                ProcessInstallStep(steps.Item(i)!, fomodPath);
            }

            BackupModuleConfig(doc, fomodPath);
            SaveFomod(doc, fomodPath, config);
        }


        private static string? GetCwdPath()
        {
            return Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName;
            
        }

        // This region is just for combing through the hierarchy of FOMOD XML files. There is probably a better way
        // to do this with XPath selectors or something, but this works for a standard FOMOD file.
        #region XML Ephemera
        
        private static XmlNodeList GetInstallSteps(XmlDocument fomod)
        {
            // find "config" childnode and return its child nodes
            if (fomod.DocumentElement == null) throw new Exception("Malformed XML");
            var xmlNodeList = fomod.DocumentElement.SelectSingleNode("/config/installSteps")?.ChildNodes;
            if (xmlNodeList != null)
            {
                return xmlNodeList;
            }
            throw new Exception("Malformed XML");
        }
        private static void ProcessInstallStep(XmlNode installStep, string fomodPath)
        {
            var groups = installStep.ChildNodes[0]!.ChildNodes;
            for (var i = 0; i < groups.Count; i++)
            {
                ProcessGroup(groups[i]!, fomodPath);
            }
        }

        private static void ProcessGroup(XmlNode group, string fomodPath)
        {
            // group has a 'plugins' child with each plugin inside
            var plugins = group.ChildNodes[0];
            for (var i = 0; i < plugins!.ChildNodes.Count; i++)
            {
                ProcessPlugin(plugins.ChildNodes[i]!, fomodPath);
            }
        }

        #endregion

        private static void ProcessPlugin(XmlNode plugin, string fomodPath)
        {
            Console.WriteLine("Processing plugin " + plugin.Attributes!["name"]!.Value);
            // TODO: collect multiple source paths. Could have multiple files in here.
            var fileNode = plugin.SelectSingleNode("files/file");
            Console.WriteLine("Processing file node");

            string? sourcePath = null;
            for (var i = 0; i < fileNode!.Attributes!.Count; i++)
            {
                var attr = fileNode.Attributes[i];
                Console.WriteLine(attr.Name + " => " + attr.Value);
                if (attr.Name.Equals("source") && IsPluginFileName(attr.Value))
                {
                    sourcePath = attr.Value;
                }
            }

            if (sourcePath == null)
            {
                return;
            }
            Console.WriteLine("Opening " + sourcePath + " to parse masters");

            var espPath = fomodPath.Replace("fomod", sourcePath);
            var masters = GetMasters(espPath);
            Console.WriteLine("Got masters for " + sourcePath);
            masters.ForEach(Console.WriteLine);
            
            /*   Check if there's anything under typeDescriptor currently. Basic FOMODs will automatically include
                 something like this
                      <typeDescriptor>
                        <type name="Optional" />
                      </typeDescriptor>
                 we want to remove the <type /> element and replace with our generated condition node.
             */

            var typeDescriptorNode = FomodXmlUtils.ResetTypeDescriptorForPluginNode(plugin);
            var conditionNode = GenerateRecommendedConditionNodeForMasters(plugin.OwnerDocument!, masters);
            Console.WriteLine("Adding recommendations to XML");
            typeDescriptorNode.AppendChild(conditionNode);
        }

        private static XmlElement GenerateRecommendedConditionNodeForMasters(XmlDocument document, List<string> masters)
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

            var dependenciesNode = document.CreateElement(Constants.ElementNames.Dependencies);
            dependenciesNode.SetAttribute(Constants.AttributeNames.Operator, Constants.AttributeValues.OperatorAnd);
            foreach (var master in masters)
            {
                var fileDependencyNode = document.CreateElement(Constants.ElementNames.FileDependency);
                fileDependencyNode.SetAttribute(Constants.AttributeNames.File, master);
                fileDependencyNode.SetAttribute(Constants.AttributeNames.State, Constants.AttributeValues.FileStateActive);

                dependenciesNode.AppendChild(fileDependencyNode);
            }

            var patternNode = document.CreateElement(Constants.ElementNames.Pattern);
            patternNode.AppendChild(dependenciesNode);
            
            // Also append <type name="Recommended"/> to patternNode
            var patternTypeNode = document.CreateElement(Constants.ElementNames.Type);
            patternTypeNode.SetAttribute(Constants.AttributeNames.Name,
                Constants.AttributeValues.PatternTypeRecommended);
            patternNode.AppendChild(patternTypeNode); 

            var patternsNode = document.CreateElement(Constants.ElementNames.Patterns);
            patternsNode.AppendChild(patternNode);
            

            var dependencyTypeNode = document.CreateElement(Constants.ElementNames.DependencyType);
            var defaultTypeNode = document.CreateElement(Constants.ElementNames.DefaultType);
            defaultTypeNode.SetAttribute(Constants.AttributeNames.Name, Constants.AttributeValues.TypeNameOptional);

            dependencyTypeNode.AppendChild(defaultTypeNode);
            dependencyTypeNode.AppendChild(patternsNode);

            return dependencyTypeNode;
        }

        #region Plugin File Parsing
        private static List<string> GetMasters(string espPath)
        {
            using var mod = SkyrimMod.CreateFromBinaryOverlay(espPath, SkyrimRelease.SkyrimSE);
            var allMasters = mod.ModHeader.MasterReferences
                .Select(masterReference => masterReference.Master.FileName)
                .Select(dummy => (string)dummy)
                .ToList();
            allMasters.RemoveAll(m => ExcludedMasters.Contains(m));
            return allMasters;
        }

        #endregion

        #region CLI arguments

        private static PrettyFomodConfig GetConfig(string[] args)
        {
            var config = new PrettyFomodConfig();
            if (args.Length < 1)
            {
                return config;
            }

            if (args.Any(a => a.Equals("-test")))
            {
                config.Test = true;
            }
            return config;
        }

        #endregion

        #region File I/O
        
        private static XmlDocument OpenFomodFile(string fomodDirectoryPath)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(Path.Combine(fomodDirectoryPath, "ModuleConfig.xml"));
            return doc;
        }

        private static void BackupModuleConfig(XmlDocument document, string fomodPath)
        {
            PrintSeparator();
            string fomodFilePath = Path.Combine(fomodPath, Constants.Filenames.ModuleFile);
            
            var backupPath = fomodFilePath.Replace(
                Constants.Filenames.ModuleFile,
                Constants.Filenames.BackupFileName());
            
            Console.WriteLine($"Backing up {Constants.Filenames.ModuleFile} to " + backupPath);
            document.Save(@backupPath);
        }

        private static void RemoveTestDocument(string fomodPath)
        {
            var filePath = Path.Combine(fomodPath, Constants.Filenames.DummyFile);
            Console.WriteLine($"Deleting dummy file at path {filePath}");
            File.Delete(filePath);
        }

        private static void SaveFomod(XmlDocument document, string fomodPath, PrettyFomodConfig config)
        {
            PrintSeparator();
            Console.WriteLine("Saving FOMOD. Here goes nothin'!");

            var filePath = config.Test
                ? Path.Combine(fomodPath, Constants.Filenames.DummyFile)
                : Path.Combine(fomodPath, Constants.Filenames.ModuleFile);
            
            document.Save(filePath);
            Console.WriteLine($"FOMOD saved to {filePath}");
        }
        
        #endregion

        #region Constants

        // No need to add these to FOMOD conditions. They're expected to be there.
        private static readonly List<string> ExcludedMasters = new List<string>()
            .Append("Skyrim.esm")
            .Append("Update.esm")
            .Append("HearthFires.esm")
            .Append("Dragonborn.esm")
            .Append("Dawnguard.esm")
            .ToList();

        #endregion

        #region Helper Fns

        private static bool IsPluginFileName(string fileName)
        {
            return fileName.Contains(".esp")
                   || fileName.Contains(".esl")
                   || fileName.Contains(".esm");
        }

        private static void PrintSeparator()
        {
            Console.WriteLine("---------------");
            Console.WriteLine();
        }

        #endregion
    }
    
}

