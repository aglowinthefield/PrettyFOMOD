using System.Xml.Serialization;

namespace Glow.PrettyFOMOD
{
    
    [XmlRoot(ElementName="Groups")]
    public class Groups { 

        [XmlElement(ElementName="element")] 
        public string Element { get; set; } 
    }

    [XmlRoot(ElementName="fomod")]
    public class FomodInfo { 

        [XmlElement(ElementName="Name")] 
        public string Name { get; set; } 

        [XmlElement(ElementName="Author")] 
        public string Author { get; set; } 

        [XmlElement(ElementName="Version")] 
        public string Version { get; set; } 

        [XmlElement(ElementName="Website")] 
        public string Website { get; set; } 

        [XmlElement(ElementName="Description")] 
        public string Description { get; set; } 

        [XmlElement(ElementName="Groups")] 
        public Groups Groups { get; set; } 
    }
        
}