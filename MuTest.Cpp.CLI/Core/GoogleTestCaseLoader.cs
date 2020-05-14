using System.IO;
using System.Xml.Serialization;
using MuTest.Cpp.CLI.Model;

namespace MuTest.Cpp.CLI.Core
{
    public static class GoogleTestCaseLoader
    {
        public static Testsuites LoadTestsFromFile(this string filePath)
        {
            var serializer = new XmlSerializer(typeof(Testsuites));
            var xmlText = File.ReadAllText(filePath);
            var stringReader = new StringReader(xmlText);
            return (Testsuites)serializer.Deserialize(stringReader);
        }
    }
}