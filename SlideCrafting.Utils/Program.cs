using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace SlideCrafting.Utils
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

            if (args.First() == "unixlf")
            {
                OS.ChangeWindowsLinefeedToLinuxLinefeed(args.Skip(1).ToList());
            }
        }
    }
}
