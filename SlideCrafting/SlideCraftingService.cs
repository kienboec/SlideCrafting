using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SlideCrafting.Config;
using SlideCrafting.Crafting;
using SlideCrafting.FileSystemWatcherHandling;
using SlideCrafting.Logger;
using SlideCrafting.Messenger;
using SlideCrafting.WebServer;

namespace SlideCrafting
{
    public class SlideCraftingService : IHostedService
    {
        private readonly IMessenger _messenger;
        private readonly ICrafter _crafter;
        private readonly IWebInterface _webInterface;
        private readonly IOptions<SlideCraftingConfig> _config;
        private readonly IWatcher _watcher;
        private readonly ILog _logger = LogManager.GetLogger(typeof(SlideCraftingService));

        private readonly SemaphoreSlim _semaphoreSlimExecute = new SemaphoreSlim(1, 1);

        private CancellationTokenSource CancelCurrentCraftingActionTokenSource { get; set; } = null;

        public SlideCraftingService(IMessenger messenger, ICrafter crafter, IWebInterface webInterface, IOptions<SlideCraftingConfig> config, IWatcher watcher)
        {
            try
            {
                _messenger = messenger;
                _crafter = crafter;
                _webInterface = webInterface;
                _watcher = watcher;
                _config = config.AssureOriginExists()
                                .AssureDistributionExists();

                _watcher.Watch(_config.Value.OriginFolder);
                _messenger.Subscribe("recraft", async (message, data) =>
                {
                    _logger.Info(message);
                    await RestartCrafting(data);
                });
            }
            catch (Exception exc)
            {
                _logger.Error("error in service startup", exc);
            }
        }


        private Task RestartCrafting(object data = null)
        {
            return Task.Run(async () =>
            {
                try
                {
                    if (await _semaphoreSlimExecute.WaitAsync(TimeSpan.FromSeconds(5)))
                    {
                        try
                        {
                            CancelCurrentCraftingActionTokenSource = new CancellationTokenSource();
                            _logger.Info("Create Crafting Task");
                            await _crafter.Craft(CancelCurrentCraftingActionTokenSource.Token);
                            _logger.Info("Finished Crafting Task");
                            CancelCurrentCraftingActionTokenSource = null;
                        }
                        finally
                        {
                            _semaphoreSlimExecute.Release();
                        }
                    }
                    else
                    {
                        _logger.Warn("Kill Crafting Task");
                        CancelCurrentCraftingActionTokenSource?.Cancel(true);
                        await RestartCrafting(data);
                    }
                }
                catch (TaskCanceledException exc)
                {
                    _logger.Warn("crafting canceled", exc);
                }
                catch (Exception exc)
                {
                    _logger.ErrorAll("error in craft-action", exc);
                }

            });
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.Register(async () =>
                {
                    await StopAsync(CancellationToken.None);
                    _webInterface.Stop(CancellationToken.None);
                });

                Task.WaitAll(
                    new[]
                    {
                        Task.Run(async () => await RestartCrafting(), cancellationToken),
                        Task.Run(async () => await _webInterface.StartAsync(cancellationToken), cancellationToken)
                    },
                    cancellationToken);
            }
            catch (Exception exc)
            {
                _logger.ErrorAll("error starting craft-action", exc);
            }

            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                CancelCurrentCraftingActionTokenSource.Cancel(true);
                await Task.CompletedTask;
            }
            catch (Exception exc)
            {
                _logger.ErrorAll("error starting craft-action", exc);
            }
        }
    }
}
