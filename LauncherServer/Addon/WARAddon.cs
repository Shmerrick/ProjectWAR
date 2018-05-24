using System.Collections.Generic;
using System.Xml.Serialization;

namespace AuthenticationServer.Addon
{
    [XmlRoot(ElementName = "VersionSettings")]
    public class aVersionSettings
    {
        [XmlAttribute(AttributeName = "gameVersion")]
        public string aGameVersion { get; set; }
        [XmlAttribute(AttributeName = "windowsVersion")]
        public string aWindowsVersion { get; set; }
        [XmlAttribute(AttributeName = "savedVariablesVersion")]
        public string aSavedVariablesVersion { get; set; }
    }

    [XmlRoot(ElementName = "Author")]
    public class aAuthor
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "email")]
        public string Email { get; set; }
    }

    [XmlRoot(ElementName = "Description")]
    public class aDescription
    {
        [XmlAttribute(AttributeName = "text")]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "Category")]
    public class aCategory
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    [XmlRoot(ElementName = "Categories")]
    public class aCategories
    {
        [XmlElement(ElementName = "Category")]
        public List<aCategory> Category { get; set; }
    }

    [XmlRoot(ElementName = "Career")]
    public class aCareer
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    [XmlRoot(ElementName = "Careers")]
    public class aCareers
    {
        [XmlElement(ElementName = "Career")]
        public List<aCareer> Career { get; set; }
    }

    [XmlRoot(ElementName = "WARInfo")]
    public class aWARInfo
    {
        [XmlElement(ElementName = "Categories")]
        public aCategories Categories { get; set; }
        [XmlElement(ElementName = "Careers")]
        public aCareers Careers { get; set; }
    }

    [XmlRoot(ElementName = "File")]
    public class aFile
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    [XmlRoot(ElementName = "Files")]
    public class aFiles
    {
        [XmlElement(ElementName = "File")]
        public List<aFile> File { get; set; }
    }

    [XmlRoot(ElementName = "CallFunction")]
    public class aCallFunction
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    [XmlRoot(ElementName = "CreateWindow")]
    public class aCreateWindow
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "show")]
        public string Show { get; set; }
    }

    [XmlRoot(ElementName = "OnInitialize")]
    public class aOnInitialize
    {
        [XmlElement(ElementName = "CreateWindow")]
        public List<aCreateWindow> CreateWindow { get; set; }
        [XmlElement(ElementName = "CallFunction")]
        public aCallFunction CallFunction { get; set; }
    }

    [XmlRoot(ElementName = "SavedVariable")]
    public class aSavedVariable
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "global")]
        public string Global { get; set; }
    }

    [XmlRoot(ElementName = "SavedVariables")]
    public class aSavedVariables
    {
        [XmlElement(ElementName = "SavedVariable")]
        public aSavedVariable SavedVariable { get; set; }
    }

    [XmlRoot(ElementName = "OnUpdate")]
    public class aOnUpdate
    {
        [XmlElement(ElementName = "CallFunction")]
        public aCallFunction CallFunction { get; set; }
    }

    [XmlRoot(ElementName = "OnShutdown")]
    public class aOnShutdown
    {
        [XmlElement(ElementName = "CallFunction")]
        public aCallFunction CallFunction { get; set; }
    }

    [XmlRoot(ElementName = "UiMod")]
    public class aUiMod
    {
        [XmlElement(ElementName = "VersionSettings")]
        public aVersionSettings VersionSettings { get; set; }
        [XmlElement(ElementName = "Author")]
        public aAuthor Author { get; set; }
        [XmlElement(ElementName = "Description")]
        public aDescription Description { get; set; }
        [XmlElement(ElementName = "WARInfo")]
        public aWARInfo WARInfo { get; set; }
        [XmlElement(ElementName = "Files")]
        public aFiles Files { get; set; }
        [XmlElement(ElementName = "OnInitialize")]
        public aOnInitialize OnInitialize { get; set; }
        [XmlElement(ElementName = "SavedVariables")]
        public aSavedVariables SavedVariables { get; set; }
        [XmlElement(ElementName = "OnUpdate")]
        public aOnUpdate OnUpdate { get; set; }
        [XmlElement(ElementName = "OnShutdown")]
        public aOnShutdown OnShutdown { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "version")]
        public string Version { get; set; }
        [XmlAttribute(AttributeName = "date")]
        public string Date { get; set; }
        [XmlAttribute(AttributeName = "autoenabled")]
        public string Autoenabled { get; set; }
    }

    [XmlRoot(ElementName = "ModuleFile")]
    public class aModuleFile
    {
        [XmlElement(ElementName = "UiMod")]
        public aUiMod UiMod { get; set; }
        [XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsi { get; set; }
    }

}
