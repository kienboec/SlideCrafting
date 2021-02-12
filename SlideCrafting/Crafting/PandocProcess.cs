using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using SlideCrafting.Config;
using SlideCrafting.Utils;

namespace SlideCrafting.Crafting
{
    public class PandocProcess
    {
        private readonly SlideCraftingConfig _config;
        private readonly string _type;
        private readonly string _metaFile;
        private readonly List<string> _inputFiles;
        private readonly List<string> _exercises;
        private ILog _logger = LogManager.GetLogger(typeof(PandocProcess));

        public PandocProcess(SlideCraftingConfig config, string type, string metaFile, List<string> inputFiles, List<string> exercises)
        {
            _config = config;
            _type = type;
            _metaFile = metaFile;
            _inputFiles = inputFiles;
            _exercises = exercises;
        }

        public async Task Start(CancellationToken token)
        {
            Process process = new Process();
            
            process.StartInfo = new ProcessStartInfo(this._config.PandocApp)
            {
                CreateNoWindow = true,
                WorkingDirectory = _config.WorkFolder,
                UseShellExecute = true,
            };
            foreach (string arg in GetArgs())
            {
                process.StartInfo.ArgumentList.Add(arg);
            }
            process.Start();
            await process.WaitForExitAsync(token);
            var exitCode = process.ExitCode;
        }

        private List<string> GetArgs()
        {
            var args = new List<string>();

            args.AddRange(_inputFiles);
            args.AddRange(this.GetMetaDataArgs());
            args.AddRange(GetPandocArgs());
            args.AddRange(this.GetFilterArgs());

            switch (_type)
            {
                case "beamer":
                    args.Add("--to=beamer"); 
                    args.AddRange(this.GetThemeArgs());

                    args.Add("-V");
                    args.Add("beameroption:show notes");
                    break;
                case "pdf":
                    break;
                case "docx":
                    break;
                case "pptx":
                    // office: -reference-doc=
                    break;
                default:
                    throw new InvalidEnumArgumentException("type parameter should not be: " + _type);
            }

            args.Add("--pdf-engine=" + _config.PdfEngine);
            
            args.Add("-o");
            args.Add(Path.Combine(
                _config.DistFolder, 
                Path.GetFileNameWithoutExtension(_metaFile) +"_slides.pdf")); // dependent of generator

            args.Add("--slide-level=3");

            return args;
        }

        private List<string> GetMetaDataArgs()
        {
            return new List<string>()
            {
                "--metadata-file=", Path.Combine(_config.WorkFolder, _metaFile)
            };

        }

        private List<string> GetThemeArgs()
        {
            var themeArgs = new List<string>();
            var themeName = GetThemeNameOrNull();
            if (themeName != null)
            {
                themeArgs.Add("-V");
                themeArgs.Add("theme:" + themeName);
            }

            return themeArgs;
        }

        private string GetThemeNameOrNull()
        {
            var beamerTemplateFile = OS.GetFilesOfFolder(_config.BeamerTemplateFolder, "sty");
            var fileName = beamerTemplateFile?.FirstOrDefault() ?? string.Empty;
            if (beamerTemplateFile.Count == 1 && fileName.StartsWith("beamertheme"))
            {
                return Path.GetFileNameWithoutExtension(beamerTemplateFile.First()).Substring(11);
            }

            _logger.Warn($"no template found (count of sty files should be 1 but is {beamerTemplateFile.Count}; the 1 file should start with beamertheme as a prefix)");
            return null;
        }

        private List<string> GetFilterArgs()
        {
            List<string> filters = new List<string>();
            if (_config.FilterFlagPlantUml)
            {
                filters.Add("--filter");
                filters.Add("pandoc-plantuml");
            }
            return filters;
        }

        private List<string> GetPandocArgs()
        {
            return new List<string>
            {
                // "--verbose",
                "--from=markdown+raw_tex+header_attributes+implicit_header_references+raw_attribute+inline_code_attributes+fancy_lists+line_blocks",
                "--log=" + _config.PandocLogFile,
                // "--toc", // table of content
                // "--toc-depth=2",
                "--highlight-style=tango",
                // "--listings", // removes highlighting from source code samples
                "--resource-path=.", // : on linux ; on windows as delimiter
                // "--filter", "pandoc-plantuml",
                "-s", // standalone
                "--self-contained",
                "--indented-code-classes=numberLines",
                "--tab-stop=2",
                "--strip-comments",
                "--number-sections",
                // "--dump-args", // dumps only and does not process the request afterwards
                "--fail-if-warnings",
                // "--strip-empty-paragraphs", -> warning deprecated
                "--strip-comments",
                // "--mathml",
                "-M", "date=" + DateTime.Now.ToString("yyyy-MM-dd")

                // -V for variables
                // "-V", "lot", # list of tables
                // "-V", "lof"  # list of figures
            };
        }

    }
}
