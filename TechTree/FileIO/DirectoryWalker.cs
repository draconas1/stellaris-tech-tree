using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TechTree.FileIO
{
    class DirectoryWalker
    {

        public static List<FileInfo> FindFilesInDirectoryTree(string root, string includeFileMask, IEnumerable<string> excludedFileNames)
        {
            var result = new List<FileInfo>();
            FindFilesInDirectoryTree(new DirectoryInfo(root), result, includeFileMask, excludedFileNames);
            return result;
        }

        /// <summary>
        /// Walks a directory tree and updates the list with  all the files found matching the specified mask. Taken from one of the .Net tutorials in walking file structures.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="fileInfos"></param>
        /// <param name="fileMask"></param>
        private static void FindFilesInDirectoryTree(DirectoryInfo root, List<FileInfo> fileInfos, string fileMask, IEnumerable<string> excludedFileNames)
        {
            FileInfo[] files = null;
            DirectoryInfo[] subDirs = null;

            // First, process all the files directly under this folder
            try
            {
                files = root.GetFiles(fileMask);
            }
            // This is thrown if even one of the files requires permissions greater
            // than the application provides.
            catch (UnauthorizedAccessException e)
            {
                // This code just writes out the message and continues to recurse.
                // You may decide to do something different here. For example, you
                // can try to elevate your privileges and access the file again.
                Console.WriteLine(e.Message);
            }
            catch (DirectoryNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }

            if (files != null)
            {
                foreach (var info in files.Where(fileInfo => excludedFileNames.All(x => x != fileInfo.Name))) {
                    fileInfos.Add(info);
                }
                // Now find all the subdirectories under this directory.
                subDirs = root.GetDirectories();

                foreach (DirectoryInfo dirInfo in subDirs)
                {
                    // Resursive call for each subdirectory.
                    FindFilesInDirectoryTree(dirInfo, fileInfos, fileMask, excludedFileNames);
                }
            }
        }
    }
}
