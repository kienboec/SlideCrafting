using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace SlideCrafting.Logger
{
    public static class ILogExtensions
    {
        public static void ErrorAll(this ILog log, string message, Exception exc)
        {
            void WriteError(Exception innerExc)
            {
                log.Error(message, innerExc);
            }

            if (exc is AggregateException aExc)
            {
                foreach (var innerExc in aExc.InnerExceptions)
                {
                    WriteError(innerExc);
                }
                return;
            }

            WriteError(exc);
        }
    }
}
