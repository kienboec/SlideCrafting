using System;
using System.IO;
using System.Linq;
using System.Text;

namespace CRLF2LFPatcher
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("wrong usage...");
                Console.WriteLine();
                Console.WriteLine("usage: CRLF2LFPatcher <path>");
                return;
            }

            if (!File.Exists(args.First()))
            {
                Console.WriteLine("file does not exists: " + args.First());
                Console.WriteLine();
                Console.WriteLine("usage: CRLF2LFPatcher <path>");
                return;
            }

            var contents = File.ReadAllText(args.First(), Encoding.UTF8);
            contents = contents.Replace("\r\n", "\n");
            File.WriteAllText(args.First(), contents);
        }
    }
}
