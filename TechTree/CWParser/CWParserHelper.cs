using CWTools.Parser;
using CWTools.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CWTools.Process;
using TechTree.DTO;
using static CWTools.Localisation.STLLocalisation;
using CWTools.Localisation;
using CWTools.Common;
using TechTree.Extensions;
using System.Linq;
using Newtonsoft.Json;

namespace TechTree.CWParser
{
    /// <summary>
    /// Main Helper class for using the CWTools library to parse general PDX files into a (raw) DTO.
    /// </summary>
    class CWParserHelper
    {
        /// <summary>
        /// Loops over collection of files and parses them using <see cref="ParseParadoxFile(System.String)"/>
        /// </summary>
        /// <param name="filePaths"></param>
        /// <returns></returns>
        public List<CWNode> ParseParadoxFile(IEnumerable<string> filePaths)
        {
            var result = new List<CWNode>();
            foreach (string paradoxFile in filePaths)
            {
                result.Add(ParseParadoxFile(paradoxFile));
            }
            return result;
        }

        /// <summary>
        /// Main method for using the CWTools library to parse an individual file.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>A CWNode representing the contents of the file.</returns>
        public CWNode ParseParadoxFile(string filePath)
        {
            //"Process" result into nicer format
            CK2Process.EventRoot processed = ProcessParadoxFile(filePath);

            // marshall this into a more c# fieldy type using the CWTools example
            CWNode marshaled = CWParsedFileMapper.ToMyNode(processed);

            return marshaled;
        }

        /// <summary>
        /// Get the processed CWTools object for the file, primary used for debugging.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public CK2Process.EventRoot ProcessParadoxFile(string filePath)
        {
            // raw parsing
            var parsed = CKParser.parseEventFile(filePath);

            // this is an extension method in CWTools.CSharp
            var eventFile = parsed.GetResult();

            //"Process" result into nicer format
            CK2Process.EventRoot processed = CK2Process.processEventFile(eventFile);         

            return processed;
        }
    }
}
