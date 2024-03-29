using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using Shouldly;

namespace {TEST_PROJECT}
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class SampleTest
    {
        [Test]
        public void Sample_WhenCalled_LoadAssembly()
        {
            // Arrange Act
            var loaded = LoadAssemblies(GetReferencedDllFilesInfoFromBaseDirectory());

            // Assert
            loaded.ShouldBeTrue();
        }

        /// <summary>
        /// Gets classes in list of assemblies
        /// </summary>
        private static bool LoadAssemblies(IEnumerable<string> assemblyNames)
        {
            var assembliesAreLoaded = true;
            foreach (var assemblyName in assemblyNames)
            {
                try
                {
                    AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(assemblyName));
                }
                catch (Exception exp)
                {
                    assembliesAreLoaded = false;
                    Debug.WriteLine(exp.ToString());
                    break;
                }
            }

            return assembliesAreLoaded;
        }

        private static string[] GetReferencedDllFilesInfoFromBaseDirectory()
        {
            return Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "{SOURCE_PROJECT_LIBRARY}");
        }
    }
}