using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using MuTest.Core.Model;

namespace MuTest.Core.Common
{
    public class CodeAnalysisProjectLoader
    {
        public Project Load(string projectPath)
        {
            return GetCompiledProject(projectPath, out _);
        }

        private static Project GetCompiledProject(string projectPath, out List<Diagnostic> errors)
        {
            var workspace = CreateWorkspace();
            var project = workspace.OpenProjectAsync(projectPath).Result;

            var compilation = project.GetCompilationAsync().Result;
            if (compilation == null)
            {
                throw new CodeAnalysisProjectLoadException(
                    $"Could not get compilation object from project {projectPath}");
            }
            errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
            if (!errors.Any())
            {
                var newCompilationOptions =
                    project.CompilationOptions.WithMetadataImportOptions(MetadataImportOptions.All);
                project = project.WithCompilationOptions(newCompilationOptions);
                return project;
            }

            // Retry by copying MSBuild dlls to executable directory. (Due to roslyn bug: https://github.com/dotnet/roslyn/issues/36414)
            try
            {
                CopyMsBuildDllsToExecutionDir();
                var compiledProject = GetCompiledProject(projectPath, out errors);
                if (!errors.Any())
                {
                    return compiledProject;
                }
                var errorsText = string.Join(Environment.NewLine, errors);

                throw new CodeAnalysisProjectLoadException(
                    $"There were some errors during loading of project located on {project.FilePath}. Please ensure that the project builds successfully in Visual Studio. Errors: {errorsText}");
            }
            finally
            {
                DeleteMsBuildDlls();
            }
        }

        private static MSBuildWorkspace CreateWorkspace()
        {
            var properties = new Dictionary<string, string>()
            {
                { "AutoGenerateBindingRedirects", "true" },
                { "GenerateBindingRedirectsOutputType", "true" },
                { "AlwaysCompileMarkupFilesInSeparateDomain", "false" } // Due to: https://github.com/dotnet/roslyn/issues/29780
            };
            var workspace = MSBuildWorkspace.Create(properties);
            workspace.LoadMetadataForReferencedProjects = true;
            return workspace;
        }

        private static readonly string[] MsBuildDlls =
        {
            "Microsoft.Build.dll",
            "Microsoft.Build.Engine.dll",
            "Microsoft.Build.Framework.dll",
            "Microsoft.Build.Tasks.Core.dll",
            "Microsoft.Build.Utilities.Core.dll"
        };

        private static void CopyMsBuildDllsToExecutionDir()
        {
            var currentPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var msBuildFolderPath = MSBuildLocator.QueryVisualStudioInstances().First().MSBuildPath;
            foreach (var msBuildDll in MsBuildDlls)
            {
                CopyMsBuildDll(msBuildDll, msBuildFolderPath, currentPath);
            }
        }

        private static void CopyMsBuildDll(string dllName, string msBuildFolderPath, string targetPath)
        {
            var sourceFilePath = Path.Combine(msBuildFolderPath, dllName);
            var targetFilePath = Path.Combine(targetPath, dllName);
            File.Copy(sourceFilePath, targetFilePath, true);
        }

        private static void DeleteMsBuildDlls()
        {
            foreach (var msBuildDll in MsBuildDlls)
            {
                if (File.Exists(msBuildDll))
                {
                    File.Delete(msBuildDll);
                }
            }
        }
    }
}