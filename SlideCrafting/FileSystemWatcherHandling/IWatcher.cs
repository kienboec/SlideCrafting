using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlideCrafting.FileSystemWatcherHandling
{
    public interface IWatcher
    {
        void Watch(string folder);
    }
}
