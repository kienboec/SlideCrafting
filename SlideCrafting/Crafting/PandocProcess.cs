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
        private readonly bool _withNotes;
        private readonly bool _renderExercises;
        private string _outFileName;
        private ILog _logger = LogManager.GetLogger(typeof(PandocProcess));

        public int ExitCode { get; private set; }

        public PandocProcess(SlideCraftingConfig config, string type, string metaFile, List<string> inputFiles, List<string> exercises, bool withNotes = false, bool renderExercises = false)
        {
            _config = config;
            _type = type;
            _metaFile = metaFile;
            _inputFiles = inputFiles;
            _exercises = exercises;
            _withNotes = withNotes;
            _renderExercises = renderExercises;
        }

        public async Task<string> Start(CancellationToken token)
        {
            
            var info = new ProcessStartInfo()
            {
                FileName = this._config.PandocApp,
                CreateNoWindow = true,
                WorkingDirectory = _config.WorkFolder,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                ErrorDialog = false
            };


            string indent = "   ";
            _logger.Debug($"Process: {_type} - {_metaFile} (notes: {_withNotes})");
            _logger.Debug(indent + "workdir: " + _config.WorkFolder);
            GetArgs().ForEach(arg => info.ArgumentList.Add(arg));
            _logger.Debug(indent + indent + "args:" + string.Join(" ", info.ArgumentList));

            using (var process = Process.Start(info))
            {
                _logger.Debug("Reading Standard Out/Err:");
                Task.WaitAll(
                    Task.Run(() =>
                    {
                        while (!process.StandardOutput.EndOfStream)
                        {
                            _logger.Info(process.StandardOutput.ReadLine());
                        }
                    }),
                    Task.Run(() =>
                    {

                        while (!process.StandardError.EndOfStream)
                        {
                            _logger.Error(process.StandardError.ReadLine());
                        }
                    }));

                await process.WaitForExitAsync(token);

                ExitCode = process.ExitCode;
                _logger.Debug(indent + $"ExitCode: {this.ExitCode}");
                _logger.Debug(indent + "ExitTime: " + process.ExitTime.ToString("O"));
                _logger.Info(indent + "Generated: " + _outFileName);
            }

            return _outFileName;
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

                    _outFileName = MakeOutName("_slides.pdf");
                    if (_withNotes)
                    {
                        args.Add("-V");
                        args.Add("beameroption:show notes");
                        _outFileName = MakeOutName("_slides_with_notes.pdf");
                    }
                    
                    args.Add("--pdf-engine=" + _config.PdfEngine);
                    
                    break;
                case "pdf":
                    args.Add("--to=pdf");
                    args.Add("--pdf-engine=" + _config.PdfEngine);
                    _outFileName = MakeOutName("_document.pdf");

                    break;
                case "docx":
                    args.Add("--to=docx");
                    _outFileName = MakeOutName("_document.docx");

                    break;
                case "pptx":
                    // office: -reference-doc=
                    args.Add("--reference-doc=" + _config.PptxReference);
                    _outFileName = MakeOutName("_slides.pptx");

                    break;
                default:
                    throw new InvalidEnumArgumentException("type parameter should not be: " + _type);
            }

            
            args.Add("-o");
            args.Add(_outFileName); // dependent of generator

            if (_type == "beamer")
            {
                args.Add("--slide-level=3");
            }

            return args;
        }

        private string MakeOutName(string ending)
        {
            var filename = Path.GetFileName(_metaFile);
            return Path.Combine(_config.DistFolder, filename.Substring(0, filename.Length - _config.IndexFilesExtension.Length) + ending);
        }

        private List<string> GetMetaDataArgs()
        {
            return new List<string>()
            {
                "--metadata-file=" + Path.Combine(_config.WorkFolder, _metaFile)
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
            var beamerTemplateFile = OS.GetFilesOfFolder(_config.BeamerTemplateFolder, ".sty");
            var fileName = beamerTemplateFile?.FirstOrDefault() ?? string.Empty;
            if (beamerTemplateFile.Count == 1 && Path.GetFileName(fileName).StartsWith("beamertheme"))
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
