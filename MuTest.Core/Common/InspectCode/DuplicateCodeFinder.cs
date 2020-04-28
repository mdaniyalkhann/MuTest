using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;
using MuTest.Core.Common.Settings;

namespace MuTest.Core.Common.InspectCode
{
    public class DuplicateCodeFinder
    {
        private const string DuplicateFinderTemplate = "DuplicateFinderTemplate.xsl";
        private readonly MuTestSettings _settings;
        private readonly string _classLocation;

        public event EventHandler<DataReceivedEventArgs> OutputDataReceived;

        public string OutputHtml { get; set; }

        public decimal DiscardCost { get; set; } = 50;

        public bool IncludePartialClasses { get; set; }

        public DuplicateCodeFinder(MuTestSettings settings, string classLocation)
        {
            if (string.IsNullOrWhiteSpace(classLocation))
            {
                throw new ArgumentNullException(nameof(classLocation));
            }

            _classLocation = classLocation;
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        private void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs args)
        {
            OnOutputDataReceived(args);
        }

        protected virtual void OnOutputDataReceived(DataReceivedEventArgs args)
        {
            OutputDataReceived?.Invoke(this, args);
        }

        public async Task FindDuplicateCode()
        {
            try
            {
                if (!File.Exists(_settings.DuplicateFinderToolPath))
                {
                    return;
                }

                if (!File.Exists(_classLocation))
                {
                    return;
                }

                if (!File.Exists(DuplicateFinderTemplate))
                {
                    return;
                }


                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(_classLocation);
                var classWildCard = $"{fileNameWithoutExtension}*.cs";
                if (IncludePartialClasses)
                {

                    if (fileNameWithoutExtension != null && fileNameWithoutExtension.Contains("."))
                    {
                        classWildCard = $"{fileNameWithoutExtension.Split('.').First()}*.cs";
                    }
                }
                else
                {
                    classWildCard = Path.GetFileName(_classLocation);
                }


                var outputFilePath = $"{_settings.TestsResultDirectory}{fileNameWithoutExtension}_{DateTime.Now:yyyyMdhhmmss}.xml";
                var arguments = new StringBuilder($" \"{Path.GetDirectoryName(_classLocation)}\\{classWildCard}\"")
                    .Append($" -o=\"{outputFilePath}\"")
                    .Append($" --discard-cost={DiscardCost}")
                    .Append(" --show-text");

                var processInfo = new ProcessStartInfo(_settings.DuplicateFinderToolPath)
                {
                    Arguments = $" {arguments}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                await Task.Run(() =>
                {
                    using (var process = new Process
                    {
                        StartInfo = processInfo,
                        EnableRaisingEvents = true
                    })
                    {
                        process.OutputDataReceived += ProcessOnOutputDataReceived;
                        process.ErrorDataReceived += ProcessOnOutputDataReceived;
                        process.Start();
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();
                        process.WaitForExit();

                        process.OutputDataReceived -= ProcessOnOutputDataReceived;
                        process.ErrorDataReceived -= ProcessOnOutputDataReceived;
                        if (File.Exists(outputFilePath))
                        {
                            var xslTransform = new XslCompiledTransform();
                            xslTransform.Load(DuplicateFinderTemplate);

                            var htmlOutput = new StringBuilder();

                            using (TextWriter htmlWriter = new StringWriter(htmlOutput))
                            {
                                using (XmlReader reader = XmlReader.Create(outputFilePath))
                                {
                                    xslTransform.Transform(reader, null, htmlWriter);
                                    OutputHtml = htmlOutput.ToString();
                                }
                            }
                        }
                    }
                });
            }
            catch (Exception exp)
            {
                Trace.TraceError("Unable to find Duplicate Code {0}", exp);
            }
        }
    }
}