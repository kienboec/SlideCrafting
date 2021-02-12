using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SlideCrafting.WebServer
{
    public interface IWebInterface
    {
        Task StartAsync(CancellationToken cancellationToken);
        void Stop(CancellationToken cancellationToken);
    }
}
