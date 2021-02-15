using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Microsoft.Extensions.Options;
using SlideCrafting.Config;
using SlideCrafting.Utils;

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
            try
            {
                OS.CreateFolder(_config.Value.DistFolder, token);
                OS.CreateFolder(_config.Value.ArchiveFolder, token);
                OS.MoveAllFilesFromFolderToFolder(
                    _config.Value.DistFolder, 
                    _config.Value.ArchiveFolder,
                    token,
                    true);
                OS.RemoveFolder(_config.Value.WorkFolder, token);
                OS.CopyFolder(_config.Value.OriginFolder, _config.Value.WorkFolder, token);

                List<string> outputFileNames = new List<string>();
                outputFileNames.AddRange(await ExecuteCraftingCommand(token));
                return await Task.FromResult(outputFileNames);
            }
            catch (Exception exc)
            {
                _logger.Error("Crafting failed", exc);
                return await Task.FromResult<List<string>>(null);
            }
        }

        protected virtual async Task<List<string>> ExecuteCraftingCommand(CancellationToken token)
        {
            OS.CopyFolder(_config.Value.WorkFolder, _config.Value.DistFolder, token, true);
            return await Task.FromResult(Directory.GetFiles(_config.Value.DistFolder).ToList());
        }

    }
}
