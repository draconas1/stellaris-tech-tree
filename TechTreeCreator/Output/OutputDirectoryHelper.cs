using System;
using System.IO;

namespace TechTreeCreator.Output {
    public class OutputDirectoryHelper {
        
        public string Root { get; }
        public string Data => Path.Combine(Root, "data");
        public string Images => Path.Combine(Root, "images"); 
        
        public OutputDirectoryHelper(string root) {
            Root = root;
        }

        public string GetImagesPath(string imageType) {
            return Path.Combine(Images, imageType);
        }
    }
}