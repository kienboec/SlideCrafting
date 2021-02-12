using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RSG;
using SlideCrafting.Config;
using SlideCrafting.Logger;
using SlideCrafting.Messenger;
using SlideCrafting.Utils;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SlideCrafting.Crafting
{
    public class FileHandlingCrafter : ICrafter
    {
        private ManualResetEvent _mre = new ManualResetEvent(false);

        private readonly ILog _logger = LogManager.GetLogger(typeof(FileHandlingCrafter));
        protected readonly IOptions<SlideCraftingConfig> _config;
        

        public FileHandlingCrafter(IOptions<SlideCraftingConfig> config)
        {
            _config = config;
        }

        public async Task<List<string>> Craft(CancellationToken token)
        {
            OS.CreateFolder(_config.Value.DistFolder);
            OS.CreateFolder(_config.Value.ArchiveFolder);
            OS.MoveAllFilesFromFolderToFolder(_config.Value.DistFolder, _config.Value.ArchiveFolder);
            OS.RemoveFolder(_config.Value.WorkFolder);
            OS.CopyFolder(_config.Value.OriginFolder, _config.Value.WorkFolder);

            List<string> outputFileNames = new List<string>();
            outputFileNames.AddRange(await ExecuteCraftingCommand(token));
            return await Task.FromResult(outputFileNames);

            /*
            var inputFiles = OS.GetFilesOfFolderRecursive(_config.Value.WorkFolder, _config.Value.ListFilesExtension, _config.Value.YamlKeyInputFiles);
            var exerciseFiles = OS.GetFilesOfFolderRecursive(_config.Value.WorkFolder, _config.Value.ListFilesExtension, _config.Value.YamlKeyExerciseFiles);

            Environment.CurrentDirectory = _config.Value.WorkFolder;
            if (token.IsCancellationRequested)
            {
                return await Task.FromResult(outputFileNames);
            }

            foreach (var project in inputFiles.Keys)
            {
                var pandocMetadataArg = "--metadata-file=" + Path.Combine(_config.Value.WorkFolder, project);
                var projectName = project.Substring(0, project.Length - _config.Value.IndexFilesExtension.Length);
            }

            return await Task.FromResult(outputFileNames);*/

        }

        protected virtual async Task<List<string>> ExecuteCraftingCommand(CancellationToken token)
        {
            OS.CopyFolder(_config.Value.WorkFolder, _config.Value.DistFolder);
            return await Task.FromResult(Directory.GetFiles(_config.Value.DistFolder).ToList());
        }

    }
}
