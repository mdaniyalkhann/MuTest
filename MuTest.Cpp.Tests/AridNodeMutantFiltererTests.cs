using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using MuTest.Cpp.CLI.Core;
using MuTest.Cpp.CLI.Core.AridNodes;
using MuTest.Cpp.CLI.Mutants;
using NUnit.Framework;
using Shouldly;

namespace MuTest.Cpp.Tests
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class AridNodeMutantFiltererTests
    {
        private static readonly IAridNodeMutantFilterer AridNodeMutantFilterer =
            new AridNodeMutantFilterer(new AridNodeFilterProvider());

        [Test]
        [TestCase(7, "cout")]
        [TestCase(12,"cin.clear()")]
        [TestCase(17, "cout.clear()")]
        [TestCase(22, "printf")]
        [TestCase(28, "fprintf")]
        [TestCase(34, "sprintf")]
        [TestCase(38, "assert")]
        [TestCase(42, "malloc")]
        [TestCase(46, "calloc")]
        [TestCase(50, "realloc")]
        public void Check_Nodes_ShouldBeArid(int lineNumber, string description)
        {
            // Arrange
            var defaultMutants = GetMutantsOnLineOfSampleCppClass(lineNumber);

            // Act
            var filteredMutants = AridNodeMutantFilterer.FilterMutants(defaultMutants).ToList();

            // Assert
            var customMessage = $"{description} node on line {lineNumber} should be arid.";
            (filteredMutants.Count < defaultMutants.Count).ShouldBeTrue(customMessage);
        }

        private static List<CppMutant> GetMutantsOnLineOfSampleCppClass(int lineNumber)
        {
            var sampleCppClassPath = GetSampleCppClassAbsolutePath();
            return CppMutantOrchestrator.GetDefaultMutants(sampleCppClassPath)
                .Where(m => m.Mutation.LineNumber == lineNumber).ToList();
        }

        private static string GetSampleCppClassAbsolutePath()
        {
            var root = GetSampleCppClassesRootDirectory();
            const string sampleClassRelativePath = "Sample.cpp";
            return Path.Combine(root, sampleClassRelativePath);
        }

        public static string GetSampleCppClassesRootDirectory()
        {
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var assemblyFile = new FileInfo(assemblyLocation);
            var rootFullPath = assemblyFile.Directory?.Parent?.Parent?.Parent?.FullName;
            if (rootFullPath == null)
            {
                throw new ConfigurationErrorsException("Root path was not found");
            }

            return Path.Combine(rootFullPath, "MuTest.Cpp.Tests.Samples");
        }
    }
}