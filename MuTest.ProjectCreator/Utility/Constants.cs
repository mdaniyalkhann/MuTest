namespace MuTest.ProjectCreator.Utility
{
    internal class Constants
    {
        internal const string EnterSolutionPath = "Enter Solution Path: ";
        internal const string EnterUnitTestsOutputPath = "Enter Unit Tests Output Path: ";
        internal const string EnterTemplateName = "Enter Template Name: ";
        internal const string EnterCompanyName = "Enter Company Name: ";
        internal const string EnterPackagesPath = "Enter Packages Relative Path: ";
        internal const string EnterTestProjectFormat = "Enter Test Project Format like `#source-project#.Tests`: ";
        internal const string EnterNUnitVersion = "Enter NUnit Version: ";
        internal const string EnterNUnitAdapterVersion = "Enter NUnit Adapter Version: ";
    
        internal const string InvalidTemplateErrorMessage = "Invalid Template...Please Select Correct Template";
        internal const string InvalidSolutionFileErrorMessage = "Solution File Not Exist Or Invalid File. Please Try Again...";
        internal const string TemplateNotExistErrorMessage = "Template Not Exist. Please Try Again...";
        internal const string EmptyCompanyNameErrorMessage = "Invalid Company Name";
        internal const string InvalidPackagesPathErrorMessage = "Invalid Packages Path";
        internal const string InvalidFormatErrorMessage = "Test Project must contain #source-project# placeholder";
        internal const string InvalidNunitVersion = "Invalid NUnit Version";
        internal const string InvalidNunitAdapterVersion = "Invalid NUnit Adapter Version";
        internal const string DirectoryNotExistErrorMessage = "Directory Not Exist. Please Try Again...";

        internal const string SolutionExtension = ".sln";
        internal const string TestProjectExtension = ".Test";
        internal const string TestsProjectExtension = ".Tests";
        internal const string UnitTestProjectExtension = "Tests.Unit";
        internal const string CSharpProjectExtension = ".csproj";
        internal const string AssemblyInfoClass = "AssemblyInfo.cs";
        internal const string PackagesFile = "packages.config";

        internal const string PropertiesFolder = "Properties";
        internal const string Templates = "Templates";
        internal const string CommonFakes = "CommonFakes";

        internal const string SemanticVersionRegex = @"^((([0-9]+)\.([0-9]+)\.([0-9]+)(?:-([0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?)(?:\+([0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?)$";
    }
}
