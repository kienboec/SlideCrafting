using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using SlideCrafting.WebServer;

namespace SlideCrafting.Tests
{
    public class WebserverTests
    {
        [Test]
        public async Task StartServer_IntegrationTest()
        {
            await new SlideCraftingWebServer().StartAsync(CancellationToken.None);
        }
    }
}
