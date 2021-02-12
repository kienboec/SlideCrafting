using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SlideCrafting.WebServer
{
    public class SlideCraftingWebServer : IWebInterface
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(SlideCraftingWebServer));

        private Task WebServerTask { get; set; } = null;
        private HttpListener _listener = null;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (!HttpListener.IsSupported)
            {
                _logger.Fatal("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                throw new NotSupportedException("HttpListener not supported");
            }

            _listener = new HttpListener();
            foreach (string s in new string[] { "http://*:8080/" })
            {
                _listener.Prefixes.Add(s);
            }
            _listener.Start();
            _logger.Debug("Start HttpListener");

            string responseString = "<HTML><BODY> SlideCrafting: Resource not found</BODY></HTML>";
            byte[] defaultResponseBuffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            WebServerTask = Task.Run(async () =>
            {
                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        _logger.Debug("Listening...");
                        HttpListenerContext context = await _listener.GetContextAsync();

                        HttpListenerRequest request = context.Request;
                        HttpListenerResponse response = context.Response;

                        byte[] buffer;
                        switch (request.RawUrl)
                        {
                            case "":
                            case "/":
                            case "/index.html":
                                buffer = await File.ReadAllBytesAsync("View/index.html", cancellationToken);
                                break;
                            case "/favicon.ico":
                                buffer = await File.ReadAllBytesAsync("View/favicon.ico", cancellationToken);
                                break;
                            default:
                                buffer = defaultResponseBuffer;
                                break;
                        }

                        response.ContentLength64 = buffer.Length;
                        var output = response.OutputStream;
                        await output.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
                        output.Close();
                    }

                }
                catch (Exception exc)
                {
                    _logger.Error("Error in HttpListener", exc);
                }
                finally
                {
                    _listener.Stop();
                    _logger.Debug("Stop HttpListener");
                }

            }, cancellationToken);
        }

        public void Stop(CancellationToken cancellationToken)
        {
            _listener.Stop();
        }
    }
}
