using System.Diagnostics;
using System.IO;
using System.Linq;
using CWToolsHelpers.Directories;
using CWToolsHelpers.FileParsing;

namespace TechTree.Output {
    public static class TechCategoryImageOutput {
        public static void OutputCategoryImages(string stellarisRootdirectory, string outputDir) {
            var categoryFile = DirectoryWalker.FindFilesInDirectoryTree(StellarisDirectoryHelper.GetTechnologyDirectory(stellarisRootdirectory), "00_category.txt");
            var catcatFile = new CWParserHelper().ParseParadoxFile(categoryFile.Select(x => x.FullName).ToList());

            if (!catcatFile.Any()) {
                Debug.WriteLine("Could not find 00_category.txt to get all the category icons");
                return;
            }

            var catNode = catcatFile.First();

            foreach (var category in catNode.Nodes) {
                var catName = category.Key;
                var imagePath = category.GetKeyValue("icon");

                ImageOutput.TransformAndOutputImage(Path.Combine(stellarisRootdirectory, imagePath),
                    Path.Combine(outputDir, catName + ".png"));
            }
        }
    }
}