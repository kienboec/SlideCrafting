using System;
using System.Collections.Generic;
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
            _logger.Info("Crafting index files:");
            indexFiles.ForEach(x => _logger.Info($"   - {x}"));

            if (_callUpdate)
            {

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
                await Task.FromResult(indexFiles);
            }

            foreach (var file in indexFiles)
            {
                PandocProcess process = new PandocProcess(_config.Value, "beamer", file, inputFiles[file], exerciseFiles[file]);
                await process.Start(token);
            }

            return await Task.FromResult(indexFiles);
        }
    }
}
