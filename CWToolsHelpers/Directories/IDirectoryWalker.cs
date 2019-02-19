using System.Collections.Generic;
using System.IO;

namespace CWToolsHelpers.Directories {
    /// <summary>
    /// Interface with an instance method such that I can write some unit tests around it.
    /// </summary>
    public interface IDirectoryWalker {
        List<string> FindFilesInDirectoryTree(string root, string includeFileMask,
            IEnumerable<string> excludedFileNames = null);
    }
}