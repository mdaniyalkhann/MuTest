using System.Collections.Generic;
using System.Xml.Serialization;

namespace MuTest.Cpp.CLI.Model
{
    [XmlRoot(ElementName = "sources")]
    public class Sources
    {
        [XmlElement(ElementName = "source")] public string Source { get; set; }
    }

    [XmlRoot(ElementName = "line")]
    public class Line
    {
        [XmlAttribute(AttributeName = "number")]
        public string Number { get; set; }

        [XmlAttribute(AttributeName = "hits")] public int Hits { get; set; }
    }

    [XmlRoot(ElementName = "lines")]
    public class Lines
    {
        [XmlElement(ElementName = "line")] public List<Line> Line { get; set; }
    }

    [XmlRoot(ElementName = "class")]
    public class Class
    {
        [XmlElement(ElementName = "methods")] public string Methods { get; set; }
        [XmlElement(ElementName = "lines")] public Lines Lines { get; set; }
        [XmlAttribute(AttributeName = "name")] public string Name { get; set; }

        [XmlAttribute(AttributeName = "filename")]
        public string Filename { get; set; }

        [XmlAttribute(AttributeName = "line-rate")]
        public string Linerate { get; set; }

        [XmlAttribute(AttributeName = "branch-rate")]
        public string BranchRate { get; set; }

        [XmlAttribute(AttributeName = "complexity")]
        public string Complexity { get; set; }
    }

    [XmlRoot(ElementName = "classes")]
    public class Classes
    {
        [XmlElement(ElementName = "class")] public List<Class> Class { get; set; }
    }

    [XmlRoot(ElementName = "package")]
    public class Package
    {
        [XmlElement(ElementName = "classes")] public Classes Classes { get; set; }
        [XmlAttribute(AttributeName = "name")] public string Name { get; set; }

        [XmlAttribute(AttributeName = "line-rate")]
        public string LineRate { get; set; }

        [XmlAttribute(AttributeName = "branch-rate")]
        public string BranchRate { get; set; }

        [XmlAttribute(AttributeName = "complexity")]
        public string Complexity { get; set; }
    }

    [XmlRoot(ElementName = "packages")]
    public class Packages
    {
        [XmlElement(ElementName = "package")] public List<Package> Package { get; set; }
    }

    [XmlRoot(ElementName = "coverage")]
    public class CoberturaCoverageReport
    {
        [XmlElement(ElementName = "sources")] public Sources Sources { get; set; }
        [XmlElement(ElementName = "packages")] public Packages Packages { get; set; }

        [XmlAttribute(AttributeName = "line-rate")]
        public string LineRate { get; set; }

        [XmlAttribute(AttributeName = "branch-rate")]
        public string BranchRate { get; set; }

        [XmlAttribute(AttributeName = "complexity")]
        public string Complexity { get; set; }

        [XmlAttribute(AttributeName = "branches-covered")]
        public string Branchescovered { get; set; }

        [XmlAttribute(AttributeName = "branches-valid")]
        public string BranchesValid { get; set; }

        [XmlAttribute(AttributeName = "timestamp")]
        public string Timestamp { get; set; }

        [XmlAttribute(AttributeName = "lines-covered")]
        public string Linescovered { get; set; }

        [XmlAttribute(AttributeName = "lines-valid")]
        public string LinesValid { get; set; }

        [XmlAttribute(AttributeName = "version")]
        public string Version { get; set; }
    }

}