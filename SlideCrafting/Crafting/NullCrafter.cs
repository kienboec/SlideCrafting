using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace SlideCrafting.Crafting
{
    public class NullCrafter : ICrafter
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(NullCrafter));

        public async Task<List<string>> Craft(CancellationToken token)
        {
            for (int i = 0; i < 10 && !token.IsCancellationRequested; i++)
            {
                await Task.Delay(1000, token);
                _logger.Info("crafting in null crafter");
            }

            return new List<string>();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }
    }
}
