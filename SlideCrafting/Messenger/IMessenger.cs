using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlideCrafting.Messenger
{
    public interface IMessenger
    {
        void Publish(string type, string message, object data);
        void Subscribe(string type, Action<string, object> action);
    }
}
