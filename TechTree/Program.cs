﻿using System;
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

namespace TechTree
{
    public class MyValue
    {
        public string Value { get; set; }
    }
    public class MyKeyValue
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
    public class MyNode
    {
        public string Key { get; set; }
        public List<MyKeyValue> KeyValues { get; set; }
        public List<MyNode> Nodes { get; set; }
        public List<MyValue> Values { get; set; }
    }

    public static class MappingSample
    {
        public static MyValue ToMyValue(LeafValue lv)
        {
            return new MyValue { Value = lv.Key };
        }

        public static MyKeyValue ToMyKeyValue(Leaf l)
        {
            return new MyKeyValue { Key = l.Key, Value = l.Value.ToRawString() };
        }

        public static MyNode ToMyNode(Node n)
        {
            var nodes = n.AllChildren.Where(x => x.IsNodeC).Select(x => ToMyNode(x.node)).ToList();
            var leaves = n.AllChildren.Where(x => x.IsLeafC).Select(x => ToMyKeyValue(x.leaf)).ToList();
            var values = n.AllChildren.Where(x => x.IsLeafValueC).Select(x => ToMyValue(x.lefavalue)).ToList();
            return new MyNode { Key = n.Key, Nodes = nodes, Values = values, KeyValues = leaves };
        }
        public static MyNode MapToMyNode()
        {
            //Support UTF-8
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            //Parse event file
            var parsed = CKParser.parseEventFile("./testevent.txt");

            var eventFile = parsed.GetResult();

            //"Process" result into nicer format
            var processed = CK2Process.processEventFile(eventFile);

            return ToMyNode(processed);
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            //Support UTF-8
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            //Parse event file
            var parsed = CKParser.parseEventFile("C:/Games/SteamLibrary/steamapps/common/Stellaris/common/technology/00_eng_tech.txt");

            var eventFile = parsed.GetResult();

            //"Process" result into nicer format
            var processed = CK2Process.processEventFile(eventFile);

            //Find interesting event
            var myEvent = processed.Children.FirstOrDefault(x => x.Key == "tech_luxuries_2");
        
            //Output
            var output = processed.ToRaw;
            Console.WriteLine(CKPrinter.printKeyValueList(output, 0));
            PrintfModule
                .PrintFormatLine(
                    new PrintfFormat<FSharpFunc<FSharpList<Statement>, Unit>, TextWriter, Unit, Unit, FSharpList<Statement>>("%A"))
                .Invoke(output);
            Console.ReadLine();
        }
    }
}
