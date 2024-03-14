using Glow.PrettyFOMOD.FomodFileIO;

namespace Glow.PrettyFOMOD.Configuration;

public static class FomodInfoCreator
{
    /*
     * 1. Check for existing info and populate FomodInfo class
     * 2. For each field in there, collect new information.
     */

    public static void CreateInfoFile(PrettyFomodConfig config)
    {
        var fomodInfo = FomodFileIo.OpenFomodInfoFile(config);
        fomodInfo.Name = ReadLine.Read("Mod Name: ", fomodInfo.Name);
        fomodInfo.Author = ReadLine.Read("Author: ", fomodInfo.Author);
        fomodInfo.Description = ReadLine.Read("Description: ", fomodInfo.Description);
        fomodInfo.Website = ReadLine.Read("Website: ", fomodInfo.Website);
        fomodInfo.Version = ReadLine.Read("Version: ", fomodInfo.Version);
        
        // TODO: Figure out groups.
        FomodFileIo.SaveFomodInfo(fomodInfo, config);
    }
}