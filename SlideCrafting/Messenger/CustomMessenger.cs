using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace SlideCrafting.Messenger
{
    public class CustomMessenger : IMessenger
    {
        private readonly Dictionary<string, Action<string, object>> _subscribers = new Dictionary<string, Action<string, object>>();
        private readonly ILog _log = LogManager.GetLogger(typeof(CustomMessenger));

        public void Publish(string type, string message, object data)
        {
            if (data is FileSystemEventArgs fsArg)
            {
                _log.Debug($"event raised: {type} - {message} ({fsArg.FullPath})");
            }
            else
            {
                _log.Debug($"event raised: {type} - {message}");
            }

            Task.Run(() => _subscribers[type]?.Invoke(message, data));
        }

        public void Subscribe(string type, Action<string, object> action)
        {
            _subscribers.Add(type, action);
        }
    }
}
