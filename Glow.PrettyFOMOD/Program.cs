using System.Xml;
using System.Xml.Serialization;
using Glow.PrettyFOMOD.CLI;
using Glow.PrettyFOMOD.Configuration;
using Glow.PrettyFOMOD.FomodFileIO;
using Glow.PrettyFOMOD.Helpers;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Constants = Glow.PrettyFOMOD.Helpers.Constants;

namespace Glow.PrettyFOMOD
{
    class Program
    {
        static void Main()
        {
            Warmup.Init();

            var config = CliUtils.ConfigFromCli();
            // Console.WriteLine("Arguments parsed: " + config);

            switch (config.GenerateFull)
            {
                case false:
                    DoSmartConditions(config);
                    break;
                case true:
                    DoGenerateFull(config);
                    break;
            }
        }

        private static void DoGenerateFull(PrettyFomodConfig config)
        {
            var fomodCreator = new FomodCreator(config);
            fomodCreator.Run();
        }
        
        private static void DoSmartConditions(PrettyFomodConfig config)
        {
            var fomodPath = FomodFileIo.GetFomodPath(config);

            if (config.Test)
            {
                RemoveTestDocument(fomodPath);
            }
            
            var moduleConfiguration = FomodFileIo.OpenFomodFile(fomodPath);
            var cache = FomodUtils.GenerateFomodDestinationEspCache(moduleConfiguration);
            var pluginNodes = FomodUtils.GetPluginNodes(moduleConfiguration);
            foreach (var pluginNode in pluginNodes)
            {
                ProcessPlugin(pluginNode, fomodPath, cache);
            }

            // serialize back to doc
            FomodFileIo.BackupModuleConfig(fomodPath);
            FomodFileIo.SaveFomod(moduleConfiguration, config);
        }

        
        private static void ProcessPlugin(Plugin plugin, string fomodPath, HashSet<string> destinationCache)
        {
            var pluginName = plugin.Name;
            Console.WriteLine("Processing plugin " + pluginName);

            var fileNodes = plugin.Files;
            if (fileNodes == null)
            {
                Console.WriteLine($"[WARNING] Plugin {plugin.Name} has no files. This might be fine for intro or information pages.");
                return;
            }

            List<string> espPaths = [];

            foreach (var fileList in fileNodes)
            {
                if (!fileList.FileSpecified) return;
                espPaths.AddRange(fileList.File.Select(fileSystemItem => fileSystemItem.Source).Where(FomodUtils.IsPluginFileName));
            }

            List<string> masters = [];
            foreach (var espPath in espPaths.Select(sourcePath => fomodPath.Replace("fomod", sourcePath)))
            {
                masters.AddRange(FomodUtils.GetMasters(espPath));
            }

            if (masters.Count < 2)
            {
                // Console.WriteLine($"Not generating recommendations for {pluginName}. Too few masters.");
                return;
            }
            
            /*   Check if there's anything under typeDescriptor currently. Basic FOMODs will automatically include
                 something like this
                      <typeDescriptor>
                        <type name="Optional" />
                      </typeDescriptor>
                 we want to remove the <type /> element and replace with our generated condition node.
             */
            var typeDescriptorNode = FomodUtils.ResetTypeDescriptorForPluginNode(plugin);
            var conditionNode = FomodUtils.GenerateRecommendedConditionNodeForMasters(masters, destinationCache);
            Console.WriteLine("Adding recommendations to XML");
            typeDescriptorNode.DependencyType = conditionNode;
        }
        
        #region FomodFileIO I/O
        
        private static void RemoveTestDocument(string fomodPath)
        {
            var filePath = Path.Combine(fomodPath, Constants.Filenames.DummyModuleFile);
            Console.WriteLine($"Deleting dummy file at path {filePath}");
            File.Delete(filePath);
        }

        #endregion
    }
    
}

