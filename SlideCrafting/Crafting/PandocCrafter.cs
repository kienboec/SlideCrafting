using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Microsoft.Extensions.Options;
using SlideCrafting.Config;
using SlideCrafting.Utils;

namespace SlideCrafting.Crafting
{
    public class PandocCrafter : FileHandlingCrafter
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(PandocCrafter));
        private bool _callUpdate;

        public void UpdateTemplateInNextRun()
        {
            _callUpdate = true;
        }

        public PandocCrafter(IOptions<SlideCraftingConfig> config) : base(config)
        {
            UpdateTemplateInNextRun();
        }

        protected override async Task<List<string>> ExecuteCraftingCommand(CancellationToken token)
        {
            var indexFiles = OS.GetFilesOfFolder(_config.Value.WorkFolder, _config.Value.IndexFilesExtension);
            var generatedFiles = new List<string>();

            _logger.Info("Crafting index files:");
            indexFiles.ForEach(x => _logger.Info($"   - {x}"));

            if (_callUpdate)
            {
                Process.Start(_config.Value.UpdateBeamerTemplateScript);
                _callUpdate = false;
            }

            var inputFiles =
                indexFiles
                    .Select(file =>
                        new KeyValuePair<string, List<string>>(
                            file,
                            OS.GetInputFilesRecursive(
                                file,
                                _config.Value.WorkFolder,
                                _config.Value.ListFilesExtension,
                                _config.Value.YamlKeyInputFiles)))
                    .ToDictionary(x => x.Key, x => x.Value);

            var exerciseFiles =
                indexFiles
                    .Select(file =>
                        new KeyValuePair<string, List<string>>(
                            file,
                            OS.GetInputFilesRecursive(
                                file,
                                _config.Value.WorkFolder,
                                _config.Value.ListFilesExtension,
                                _config.Value.YamlKeyExerciseFiles)))
                    .ToDictionary(x => x.Key, x => x.Value);

            if (token.IsCancellationRequested)
            {
                return await Task.FromResult(indexFiles);
            }

            foreach (var file in indexFiles)
            {
                bool success = false;
                
                for (int i = 0; i < _config.Value.AttemptsOnError && !success; i++)
                {
                    var pandocProcesses = new List<PandocProcess>
                    {
                        new PandocProcess(_config.Value, "beamer", file, inputFiles[file], exerciseFiles[file], withNotes: true),
                        new PandocProcess(_config.Value, "beamer", file, inputFiles[file], exerciseFiles[file], withNotes: false),
                        new PandocProcess(_config.Value, "pdf", file, inputFiles[file], exerciseFiles[file]),
                        new PandocProcess(_config.Value, "pdf", file, inputFiles[file], exerciseFiles[file], renderExercises: true),
                        new PandocProcess(_config.Value, "pptx", file, inputFiles[file], exerciseFiles[file]),
                        new PandocProcess(_config.Value, "docx", file, inputFiles[file], exerciseFiles[file]),
                        new PandocProcess(_config.Value, "docx", file, inputFiles[file], exerciseFiles[file], renderExercises: true),


                    };

                    foreach (var pandocProcess in pandocProcesses)
                    {
                        
                        generatedFiles.Add(await pandocProcess.Start(token));
                        if (pandocProcess.ExitCode == 0)
                        {
                            success = true;
                        }
                    }
                }
            }

            return await Task.FromResult(generatedFiles);
        }
    }
}
