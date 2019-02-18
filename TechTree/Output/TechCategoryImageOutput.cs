using System.Diagnostics;
using System.IO;
using System.Linq;
using CWToolsHelpers.Directories;
using CWToolsHelpers.FileParsing;

namespace TechTree.Output {
    public static class TechCategoryImageOutput {
        public static void OutputCategoryImages(string stellarisRootDirectory, string outputDir) {
            var categoryFile = DirectoryWalker.FindFilesInDirectoryTree(StellarisDirectoryHelper.GetTechnologyDirectory(stellarisRootDirectory), "00_category.txt");
            var catcatFile = new CWParserHelper().ParseParadoxFile(categoryFile.Select(x => x.FullName).ToList());

            if (!catcatFile.Any()) {
                Debug.WriteLine("Could not find 00_category.txt to get all the category icons");
                return;
            }

            var catNode = catcatFile.First();

            foreach (var category in catNode.Value.Nodes) {
                var catName = category.Key;
                var imagePath = category.GetKeyValue("icon");

                ImageOutput.TransformAndOutputImage(Path.Combine(stellarisRootDirectory, imagePath),
                    Path.Combine(outputDir, catName + ".png"));
            }
        }
    }
}