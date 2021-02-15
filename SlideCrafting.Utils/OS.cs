using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace SlideCrafting.Utils
{
    public class OS
    {
        public static void ChangeWindowsLinefeedToLinuxLinefeed(List<string> fileNames)
        {
            foreach (string fileName in fileNames)
            {
                if (!File.Exists(fileName))
                {
                    Console.WriteLine("file does not exists: " + fileName);
                    Console.WriteLine();
                    Console.WriteLine("usage: CRLF2LFPatcher <path>");
                    return;
                }
            }

            foreach (string fileName in fileNames)
            {
                var contents = File.ReadAllText(fileName, Encoding.UTF8);
                contents = contents.Replace("\r\n", "\n");
                File.WriteAllText(fileName, contents);
                Console.WriteLine($"patched win to unix newline: {fileName}");
            }
        }

        public static void CreateFolder(string distFolder, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                throw new TaskCanceledException("Create folder request on already canceled task");
            }

            Directory.CreateDirectory(distFolder);
        }

        public static void MoveAllFilesFromFolderToFolder(string fromFolder, string toFolder, CancellationToken token, bool shouldRethrow = false)
        {
            foreach (var file in Directory.EnumerateFiles(fromFolder))
            {
                if (token.IsCancellationRequested)
                {
                    throw new TaskCanceledException("move all files request on already canceled task");
                }

                try
                {
                    File.Move(file, Path.Combine(toFolder, Path.GetFileName(file)), overwrite: true);
                }
                catch (Exception exc)
                {
                    if (shouldRethrow)
                    {
                        throw new FileLoadException("can not load file to move (" + file + ")", file, exc);
                    }
                }
            }

            foreach (var sourceFolder in Directory.EnumerateDirectories((fromFolder)).ToList())
            {
                if (sourceFolder.TrimEnd(Path.DirectorySeparatorChar).EndsWith(".git")) 
                {
                    continue;
                }

                if (token.IsCancellationRequested)
                {
                    throw new TaskCanceledException("move all files request on already canceled task");
                }

                try
                {
                    if(string.Compare(
                        Path.GetFullPath(sourceFolder).TrimEnd(Path.DirectorySeparatorChar),
                        Path.GetFullPath(toFolder).TrimEnd(Path.DirectorySeparatorChar),
                        StringComparison.InvariantCultureIgnoreCase) != 0)
                    {
                        var dirInfo = new DirectoryInfo(sourceFolder);
                        CopyFolder(sourceFolder, toFolder, token, true);
                        RemoveFolder(sourceFolder, token);
                    }
                }
                catch (Exception exc)
                {
                    if (shouldRethrow)
                    {
                        throw new FileLoadException("can not load dir to move (" + sourceFolder + ")", sourceFolder, exc);
                    }
                }
            }
        }

        public static void RemoveFolder(string folder, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                throw new TaskCanceledException("remove folder request on already canceled task");
            }

            try
            {
                Directory.Delete(folder, true);
            }
            catch
            {
                // ignore
            }
        }

        // https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
        public static void CopyFolder(string sourceDirName, string destDirName, CancellationToken token, bool copySubDirs = true)
        {
            if (token.IsCancellationRequested)
            {
                throw new TaskCanceledException("copy files on already canceled task");
            }

            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, true);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    CopyFolder(subdir.FullName, tempPath, token, copySubDirs);
                }
            }
        }

        public static List<string> GetFilesOfFolder(string workFolder, string extensionFilter = null)
        {
            if (!Directory.Exists(workFolder))
            {
                return new List<string>();
            }

            var list = Directory.EnumerateFiles(workFolder)
                .Where(x => extensionFilter == null || x.EndsWith(extensionFilter))
                .ToList();
            list.Sort();
            return list;
        }

        public static List<string> GetInputFilesRecursive(string file, string baseFolder, string extension, string settingKey)
        {
            var returnFiles = new List<string>();
            
            // https://github.com/aaubry/YamlDotNet
            var yaml = new YamlStream();
            if (Path.IsPathFullyQualified(file))
            {
                yaml.Load(new StreamReader(file));
            }
            else
            {
                yaml.Load(new StreamReader(Path.Combine(baseFolder, file)));
            }

            var root = (YamlMappingNode)yaml.Documents[0].RootNode;
            if (!root.Children.ContainsKey(settingKey))
            {
                return returnFiles;
            }

            YamlSequenceNode node = (YamlSequenceNode)root.Children[settingKey];
            var inputFiles = node.Children.Select(x => x.ToString());

            foreach (var inputFile in inputFiles)
            {
                if (inputFile.EndsWith(extension))
                {
                    returnFiles.AddRange(GetInputFilesRecursive(inputFile, baseFolder, extension, settingKey));
                }
                else
                {
                    returnFiles.Add(inputFile);
                }
            }

            return returnFiles;
        }
    }
}
