using System;
using System.Collections.Generic;
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
                Console.WriteLine("usage: CRLF2LFPatcher <path}+");
                return;
            }

            foreach (string fileName in args)
            {
                if (!File.Exists(fileName))
                {
                    Console.WriteLine("file does not exists: " + fileName);
                    Console.WriteLine();
                    Console.WriteLine("usage: CRLF2LFPatcher <path>");
                    return;
                }
            }

            foreach (string fileName in args)
            {
                var contents = File.ReadAllText(fileName, Encoding.UTF8);
                contents = contents.Replace("\r\n", "\n");
                File.WriteAllText(fileName, contents);
                Console.WriteLine($"patched win to unix newline: {fileName}");
            }
        }
    }
}
