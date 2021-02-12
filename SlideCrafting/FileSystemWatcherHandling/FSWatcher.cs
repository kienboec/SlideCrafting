using System.IO;
using log4net;
using Microsoft.Extensions.Options;
using SlideCrafting.Messenger;

namespace SlideCrafting.FileSystemWatcherHandling
{
    public class FSWatcher : IWatcher
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(FSWatcher));
        private readonly IMessenger _messenger;

        private FileSystemWatcher _watcher;

        public FSWatcher(IMessenger messenger)
        {
            _messenger = messenger;
        }

        public void Watch(string folder)
        {
            _log.Debug("start initializing watcher");

            _watcher = new FileSystemWatcher(folder) { IncludeSubdirectories = true };
            _watcher.Deleted += (sender, e) => _messenger.Publish("recraft", "file deleted", e);
            _watcher.Changed += (sender, e) => _messenger.Publish("recraft", "file changed", e);
            _watcher.Created += (sender, e) => _messenger.Publish("recraft", "file created", e);
            _watcher.Renamed += (sender, e) => _messenger.Publish("recraft", "file renamed", e);
            _watcher.Error += (sender, e) => _log.Error("Watcher detected error", e.GetException());
            _watcher.EnableRaisingEvents = true;

            _log.Debug("stop initializing watcher");
        }
    }
}
