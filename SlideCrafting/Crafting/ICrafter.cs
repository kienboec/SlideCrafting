using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SlideCrafting.Crafting
{
    public interface ICrafter
    {
        Task<List<string>> Craft(CancellationToken token);
    }
}
