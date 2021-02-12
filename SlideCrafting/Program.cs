﻿using System;
using System.IO;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SlideCrafting.Config;
using SlideCrafting.Crafting;
using SlideCrafting.FileSystemWatcherHandling;
using SlideCrafting.Messenger;
using SlideCrafting.WebServer;
using Exception = System.Exception;

namespace SlideCrafting
{
    class Program
    {
        static void Main(string[] args)
        {
            var log = LogManager.GetLogger(typeof(Program));

            try
            {
                // https://docs.microsoft.com/en-us/dotnet/core/extensions/generic-host
                Host.CreateDefaultBuilder(args) // // see code ... add commandline, current dir, apply settings,...
                    .ConfigureLogging((context, builder) =>
                    {
                        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                        // builder.AddLog4Net(context.Configuration.GetValue<string>("LoggerConfig", "Log4Net.config.xml"));
                        // builder.SetMinimumLevel(LogLevel.Trace);

                        XmlConfigurator.Configure(
                            new FileInfo(
                                context.Configuration.GetValue<string>("LoggerConfig", "Log4Net.config.xml")));

                        log.Debug("Logging configured");
                    })
                    .ConfigureServices((context, services) =>
                    {
                        services
                            .Configure<SlideCraftingConfig>(context.Configuration,
                                options => { options.BindNonPublicProperties = false; })

                            .AddSingleton<IMessenger, CustomMessenger>()
                            .AddSingleton<IWatcher, FSWatcher>()

                            .AddSingleton<ICrafter, FileHandlingCrafter>()
                            .AddSingleton<IWebInterface, SlideCraftingWebServer>()

                            .AddHostedService<SlideCraftingService>()
                            ;

                        log.Debug("Services configured");
                    })
                    .UseConsoleLifetime()
                    .Build()
                    .Run();
            }
            catch (Exception exc)
            {
                log.Fatal("unhandled exception caught", exc);
            }
        }
    }
}
