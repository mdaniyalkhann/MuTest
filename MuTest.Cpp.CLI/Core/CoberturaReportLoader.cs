using System.IO;
using System.Xml.Serialization;
using MuTest.Cpp.CLI.Model;

namespace MuTest.Cpp.CLI.Core
{
    public static class CoberturaReportLoader
    {
        public static CoberturaCoverageReport LoadFromFile(this string filePath)
        {
            var serializer = new XmlSerializer(typeof(CoberturaCoverageReport));
            var xmlText = File.ReadAllText(filePath);
            var stringReader = new StringReader(xmlText);
            return (CoberturaCoverageReport)serializer.Deserialize(stringReader);
        }
    }
}