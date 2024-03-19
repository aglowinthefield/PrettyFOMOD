using Mutagen.Bethesda.Skyrim;

namespace Glow.PrettyFOMOD.Library;

public class EspFile
{
    /*
     * e.g. "CoolMod.esp"
     */
    public string Filename { get; set; }
    
    /*
     * e.g. "D:\var\Mod\CoolMod.esp"
     */
    public string FullFilePath { get; set; }
    
    /*
     * The ESP names of the masters as read by Mutagen.
     * e.g. ["CoolModFramework.esp", "Skyrim.esm"]
     */
    public string[] Masters { get; set; }

    public EspFile(string fullFilePath)
    {
     FullFilePath = fullFilePath;
     Filename = Path.GetFileName(fullFilePath);
     Masters = SkyrimMod
      .CreateFromBinaryOverlay(fullFilePath, SkyrimRelease.SkyrimSE)
      .ModHeader
      .MasterReferences
      .Select(m => m.Master.FileName.String)
      .ToArray();
    }

}