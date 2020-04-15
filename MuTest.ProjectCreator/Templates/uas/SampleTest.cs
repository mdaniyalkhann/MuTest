using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace {TEST_PROJECT}
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class SampleTest
    {
        [Test]
        public void Sample_WhenCalled_LoadAssembly()
        {
            LoadAssemblies(GetReferencedDllFilesInfoFromBaseDirectory());
        }

        public static string[] GetReferencedDllFilesInfoFromBaseDirectory()
        {
            return Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "{SOURCE_PROJECT_LIBRARY}");
        }

        /// <summary>
        /// Gets classes in list of assemblies
        /// </summary>
        public static void LoadAssemblies(string[] assemblyNames)
        {
            foreach (var assemblyName in assemblyNames)
            {
                try
                {
                    AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(assemblyName));
                }
                catch (Exception exp)
                {
                    Debug.WriteLine(exp.Message);
                }
            }
        }
    }
}
