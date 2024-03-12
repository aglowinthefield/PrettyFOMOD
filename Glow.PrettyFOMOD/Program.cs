using System.Xml;
using System.Xml.Serialization;
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
        static void Main(string[] args)
        {
            Warmup.Init();
            
            var config = GetConfig(args);
            Console.WriteLine("Arguments parsed: " + config);

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
            FomodInfoCreator.CreateInfoFile(config);
        }
        
        private static void DoSmartConditions(PrettyFomodConfig config)
        {
            var fomodPath = FomodFileIo.GetFomodPath(config);

            if (config.Test)
            {
                RemoveTestDocument(fomodPath);
            }
            
            var moduleConfiguration = FomodFileIo.OpenFomodFile(fomodPath);
            var cache = FomodXmlUtils.GenerateFomodDestinationEspCache(moduleConfiguration);
            var pluginNodes = FomodXmlUtils.GetPluginNodes(moduleConfiguration);
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
                espPaths.AddRange(fileList.File.Select(fileSystemItem => fileSystemItem.Source).Where(IsPluginFileName));
            }

            List<string> masters = [];
            foreach (var espPath in espPaths.Select(sourcePath => fomodPath.Replace("fomod", sourcePath)))
            {
                masters.AddRange(GetMasters(espPath));
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
            var typeDescriptorNode = FomodXmlUtils.ResetTypeDescriptorForPluginNode(plugin);
            var conditionNode = GenerateRecommendedConditionNodeForMasters(masters, destinationCache);
            Console.WriteLine("Adding recommendations to XML");
            typeDescriptorNode.DependencyType = conditionNode;
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private static DependencyPluginType GenerateRecommendedConditionNodeForMasters(List<string> masters, HashSet<string> destinationCache)
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

        #region Plugin FomodFileIO Parsing
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

            if (args.Any(a => a.ToLower().Equals("-generatefull")))
            {
                config.GenerateFull = true;
                config.SmartConditions = false;
            }
            return config;
        }

        #endregion

        #region FomodFileIO I/O
        
        private static void RemoveTestDocument(string fomodPath)
        {
            var filePath = Path.Combine(fomodPath, Constants.Filenames.DummyModuleFile);
            Console.WriteLine($"Deleting dummy file at path {filePath}");
            File.Delete(filePath);
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

        #endregion
    }
    
}

