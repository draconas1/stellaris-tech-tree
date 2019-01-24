using System;
using CWTools.Parser;
using System.Text;
using Microsoft.FSharp.Collections;
using System.Collections.Generic;
using System.Linq;
using CWTools.Process;
using Microsoft.FSharp.Core;
using System.IO;
using System.Text.RegularExpressions;
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
using TechTree.Output;

namespace TechTree
{
    class Program {
        public const Boolean MAC = true;
        
        public const string STELLARIS_ROOT_WINDOWS = "C:/Games/SteamLibrary/steamapps/common/Stellaris";
        public const string STELLARIS_ROOT_MAC = "/Users/christian/Library/Application Support/Steam/steamapps/common/Stellaris";
        public const string ROOT_IN_USE = MAC ? STELLARIS_ROOT_MAC : STELLARIS_ROOT_WINDOWS;
        
        
        public const string OUTPUT_WINDOWS = "C:/Users/Draconas/source/repos/stellaris-tech-tree";
        public const string OUTPUT_MAC = "/Users/christian/dev/graph/stellaris-tech-tree";
        public const string OUTPUT_IN_USE = MAC ? OUTPUT_MAC : OUTPUT_WINDOWS;
        static void Main(string[] args)
        {
            //Support UTF-8
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            //setup localisation 
            ILocalisationAPI localisation = Localisation.GetLocalisationAPI(ROOT_IN_USE, STLLang.English);

            // setup parser
            var dirHelper = new StellarisDirectoryHelper(ROOT_IN_USE);
            var scriptedVariablesHelper = new ScriptedVariableParser(dirHelper.ScriptedVariables);
            var parser = new TechTreeParser(localisation, scriptedVariablesHelper.ParseScriptedVariables(), dirHelper.Technology);
            parser.IgnoreFiles.AddRange(new string[] { "00_tier.txt", "00_category.txt" });
           // parser.Areas.Add("physics");
            //parser.ParseFileMask = "00_eng_tech.txt";
           
            // get the results parsed into nice tech tree format
            var model = parser.ParseTechFiles();

            var visDataMarshaler = new VisDataMarshaler(localisation, null);
            var visResults = visDataMarshaler.CreateVisData(model);

            //save
            visResults.WriteVisDataToOneJSFile(OUTPUT_IN_USE);

            Console.WriteLine("done.  Nodes: " + visResults.nodes.Count() + " Edges: " + visResults.edges.Count());
            Console.ReadLine();
        }
    }
}
