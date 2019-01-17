using System.IO;
using System.Linq;

namespace TechTree.Extensions
{
    public static class DirectoryInfoExtensions
    {
        public static DirectoryInfo getChildDirectory(this DirectoryInfo directoryInfo, string subDirectoryName)
        {
            return directoryInfo.EnumerateDirectories().First(info => info.Name == subDirectoryName);
        }
    }
}
