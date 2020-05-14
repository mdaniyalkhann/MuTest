using System.Collections.Generic;
using System.Xml.Serialization;

namespace MuTest.Cpp.CLI.Model
{
    [XmlRoot(ElementName = "testcase")]
    public class Testcase
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "status")]
        public string Status { get; set; }
        [XmlAttribute(AttributeName = "time")]
        public string Time { get; set; }
        [XmlAttribute(AttributeName = "classname")]
        public string Classname { get; set; }
    }

    [XmlRoot(ElementName = "testsuite")]
    public class Testsuite
    {
        [XmlElement(ElementName = "testcase")]
        public List<Testcase> Testcase { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "tests")]
        public string Tests { get; set; }
        [XmlAttribute(AttributeName = "failures")]
        public string Failures { get; set; }
        [XmlAttribute(AttributeName = "disabled")]
        public string Disabled { get; set; }
        [XmlAttribute(AttributeName = "errors")]
        public string Errors { get; set; }
        [XmlAttribute(AttributeName = "time")]
        public string Time { get; set; }
    }

    [XmlRoot(ElementName = "testsuites")]
    public class Testsuites
    {
        [XmlElement(ElementName = "testsuite")]
        public List<Testsuite> Testsuite { get; set; }
        [XmlAttribute(AttributeName = "tests")]
        public string Tests { get; set; }
        [XmlAttribute(AttributeName = "failures")]
        public string Failures { get; set; }
        [XmlAttribute(AttributeName = "disabled")]
        public string Disabled { get; set; }
        [XmlAttribute(AttributeName = "errors")]
        public string Errors { get; set; }
        [XmlAttribute(AttributeName = "timestamp")]
        public string Timestamp { get; set; }
        [XmlAttribute(AttributeName = "time")]
        public string Time { get; set; }
        [XmlAttribute(AttributeName = "random_seed")]
        public string Random_seed { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

}