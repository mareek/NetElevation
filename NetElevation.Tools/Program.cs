using System;
using System.IO;
using System.Linq;
using NetElevation.Core;

namespace NetElevation.Tools
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!args.Any())
            {
                ShowHelp();
                return;
            }

            var command = args[0];
            switch (command)
            {
                case "init" when args.Length == 2:
                    InitRepository(args[1]);
                    break;
                default:
                    ShowHelp();
                    break;
            }
        }

        private static void ShowHelp()
        {
            Console.WriteLine();
            Console.WriteLine("Usage: NetElevation.tools.exe COMMAND [OPTIONS]");
            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine("\tinit\t Create or update de tiles.json file of the targeted directory");
            Console.WriteLine();
        }
        private static void InitRepository(string directoryPath)
        {
            var directory = new DirectoryInfo(directoryPath);
            if (!directory.Exists)
            {
                Console.WriteLine($"Directory \"{directoryPath}\" does not exists");
                return;
            }

            Console.WriteLine($"Initializing \"{directoryPath}\"");

            var repository = new TileRepository(directory);
            repository.InitRepository(true);
        }
    }
}
