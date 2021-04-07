using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Microsoft.Extensions.Options;
using SlideCrafting.Config;
using SlideCrafting.Messenger;

namespace SlideCrafting.ConsoleInterface
{
    public class ConsoleInterface: IConsoleInterface
    {
        private readonly IOptions<SlideCraftingConfig> _config;
        private readonly IMessenger _messenger;
        private readonly ILog _log = LogManager.GetLogger(typeof(ConsoleInterface));
        private bool _isStopRequested = false;

        public ConsoleInterface(IOptions<SlideCraftingConfig> config, IMessenger messenger)
        {
            _config = config;
            _messenger = messenger;
        }

        public void Start()
        {
            Task.Run(() =>
            {
                while (!this._isStopRequested)
                {
                    var command = Console.ReadLine();
                    switch (command.ToLower())
                    {
                        case "gen":
                        case "generate":
                        case "recraft":
                            _messenger.Publish("recraft", "crafting requested by command line", null);
                            break;
                        default:
                            _log.Warn("unknown command retrieved in console interface: " + (command ?? "null"));
                            break;
                    }
                }

            });
        }

        public void Stop()
        {

        }
    }
}
