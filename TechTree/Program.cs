using System;
using CWTools.Parser;
using System.Text;
using Microsoft.FSharp.Collections;
using System.Collections.Generic;
using System.Linq;
using CWTools.Process;
using Microsoft.FSharp.Core;
using System.IO;
using CWTools.CSharp;
using static CWTools.Parser.Types;
using static CWTools.Utilities.Position;
using TechTree.FileIO;
using TechTree.CWParser;
using static CWTools.Localisation.STLLocalisation;
using CWTools.Localisation;
using CWTools.Common;
using TechTree.DTO;
using Newtonsoft.Json;

namespace TechTree
{
   


   
    class Program
    {
        public const string STELLARIS_ROOT_WINDOWS = "C:/Games/SteamLibrary/steamapps/common/Stellaris";

        static void Main(string[] args)
        {
            //Support UTF-8
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            //setup localisation 
            ILocalisationAPI localisation = Localisation.GetLocalisationAPI(STELLARIS_ROOT_WINDOWS, STLLang.English);

            // setup parser
            var dirHelper = new StellarisDirectoryHelper(STELLARIS_ROOT_WINDOWS);
            var parser = new TechTreeParser(localisation);

            // trawl the technology files first as they are core
            List<FileInfo> technologyFiles = DirectoryWalker.FindFilesInDirectoryTree(dirHelper.Technology, "*.txt"); ;


            var visResults = parser.ParseTechFiles(technologyFiles);


            /// Json outputter
            JsonSerializer serializer = new JsonSerializer();
            serializer.Formatting = Formatting.Indented;

            using (StreamWriter sw = new StreamWriter(@"d:\json.txt"))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, visResults);
            }

            Console.WriteLine("done");
            Console.ReadLine();
        }
    }
}
