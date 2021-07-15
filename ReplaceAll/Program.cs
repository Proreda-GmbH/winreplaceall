using System;
using System.IO;
using System.Linq;

namespace ReplaceAll
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("replaceall usage: replaceall directory toReplace replacement");
            var directory = args[0];
            var toReplace = args[1];
            var replacement = args[2];
            Console.Write($"Replacing all '{toReplace}' by '{replacement}' in '{directory}' files, file names and directories.\nPress 'c' to continue.");
            var key = char.ToString(Console.ReadKey().KeyChar);
            if (key != "c")
            {
                return;
            }

            // Replace file contents
            var fileNamesWithPath = Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories).ToArray();
            Console.WriteLine($"Replacing content of {fileNamesWithPath.Length} files...");
            foreach (var fileNameWithPath in fileNamesWithPath)
            {
                Console.Write(".");
                File.WriteAllText(fileNameWithPath, File.ReadAllText(fileNameWithPath).Replace(toReplace, replacement));
            }
            Console.WriteLine(" done.");

            // Replace file Names
            var fileNames = fileNamesWithPath.ToDictionary(fnwp => fnwp, fnwp => Path.GetFileName(fnwp));
            var fileNamesToReplace = fileNames.Where(kvp => kvp.Value.Contains(toReplace)).Select(kvp => kvp.Key).ToArray();
            Console.WriteLine($"Replacing names of {fileNamesToReplace.Length} files...");
            foreach (var fileNameToReplace in fileNamesToReplace)
            {
                Console.Write(".");
                var newFileName = Path.Combine(Path.GetDirectoryName(fileNameToReplace), fileNames[fileNameToReplace].Replace(toReplace, replacement));
                File.Delete(newFileName);
                File.Move(fileNameToReplace, newFileName);
            }
            Console.WriteLine(" done.");

            // Replace directory names
            var directoryPaths = fileNamesWithPath.Select(fnwp => Path.GetDirectoryName(fnwp)).Distinct().OrderByDescending(dn => dn.Length).ToArray();
            var directoryNames = directoryPaths.ToDictionary(dp => dp, dp => dp.TrimEnd(Path.DirectorySeparatorChar).Split(System.IO.Path.DirectorySeparatorChar).Last());
            var directoryNamesToReplace = directoryNames.Where(dn => dn.Value.Contains(toReplace)).Select(dn => dn.Key).ToArray();
            Console.WriteLine($"Replacing names of {directoryNamesToReplace.Length} directories...");
            foreach (var directoryNameToReplace in directoryNamesToReplace)
            {
                Console.Write(".");
                var directoryNameStrippedOfLastPart = directoryNameToReplace.Substring(0, directoryNameToReplace.Length - directoryNames[directoryNameToReplace].Length);
                var newDirectoryName = $"{directoryNameStrippedOfLastPart}{directoryNames[directoryNameToReplace].Replace(toReplace, replacement)}";
                try
                {
                    Directory.Delete(newDirectoryName, true);
                }
                catch (Exception)
                {
                    // ignored
                }

                Directory.Move(directoryNameToReplace, newDirectoryName);
            }
            Console.WriteLine(" done.");
        }
    }
}