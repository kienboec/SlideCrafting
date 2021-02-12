using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        public static void CreateFolder(string distFolder)
        {
            Directory.CreateDirectory(distFolder);
        }

        public static void MoveAllFilesFromFolderToFolder(string fromFolder, string toFolder, bool shouldRethrow = false) {
            foreach (var file in Directory.EnumerateFiles(fromFolder))
            {
                try
                {
                    File.Move(file, Path.Combine(toFolder, Path.GetFileName(file)), overwrite: true);
                }
                catch
                {
                    try
                    {
                        File.Copy(file, Path.Combine(toFolder, Path.GetFileName(file)), overwrite: true);
                    }
                    catch (Exception exc)
                    {
                        if (shouldRethrow)
                        {
                            throw new FileLoadException("can not load file to move or copy (" + file + ")", file, exc);
                        }
                    }
                }
            }

            foreach (var sourceFolder in Directory.EnumerateDirectories((fromFolder)))
            {
                if (!sourceFolder.StartsWith(toFolder))
                {
                    var dirInfo = new DirectoryInfo(sourceFolder);
                    var targetFolder = Path.Combine(toFolder, dirInfo.Name);
                    CreateFolder(targetFolder);
                    MoveAllFilesFromFolderToFolder(sourceFolder, targetFolder);
                    RemoveFolder(sourceFolder);
                }
            }
        }

        public static void RemoveFolder(string folder)
        {
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
        public static void CopyFolder(string sourceDirName, string destDirName, bool copySubDirs = true)
        {
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
                file.CopyTo(tempPath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    CopyFolder(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }

        public static List<string> GetFilesOfFolder(string workFolder, string extensionFilter = null)
        {
            var list = Directory.EnumerateFiles(workFolder)
                .Where(x => extensionFilter == null || Path.GetExtension(x) == extensionFilter)
                .ToList();
            list.Sort();
            return list;
        }

        public static List<string> GetInputFilesRecursive(string file, string extension, string settingKey)
        {
            // https://github.com/aaubry/YamlDotNet
            var yaml = new YamlStream();
            yaml.Load(new StreamReader(file));

            var root = (YamlMappingNode)yaml.Documents[0].RootNode;
            YamlSequenceNode node = (YamlSequenceNode)root.Children[settingKey];
            var inputFiles = node.Children.Select(x => x.ToString());

            var returnFiles = new List<string>();
            foreach (var inputFile in inputFiles)
            {
                if (Path.GetExtension(inputFile) == extension)
                {
                    returnFiles.AddRange(GetInputFilesRecursive(inputFile, extension, settingKey));
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
