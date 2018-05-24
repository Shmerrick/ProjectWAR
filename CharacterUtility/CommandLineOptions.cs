using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace CharacterUtility
{
    public class CommandLineOptions
    {
        [Verb("import", HelpText = "Import Character from Character-Export file.")]
        public class ImportOptions
        {
            [Option('f', "output-file", Required = false, HelpText = "Character-Export file to be processed.")]
            public string ImportFileName { get; set; }
        }

        [Verb("export", HelpText = "Export Character to Character-Export file.")]
        public class ExportOptions
        {
            [Option('f', "output-file", Required = false, HelpText = "Character-Export file to be processed.")]
            public string ExportFileName { get; set; }

        }

        [Verb("create", HelpText = "Create Character directly into DB.")]
        public class CreateOptions
        {
            [Option('f', "template-file", Required = true, HelpText = "Template file to be processed.")]
            public string TemplateFileName { get; set; }

        }

        [Verb("itemset", HelpText = "Extend ItemSet data in DB.")]
        public class ItemSetOptions
        {
            [Option('r', "rebuild", Required = false, HelpText = "Rebuild the values of the item set table")]
            public bool RebuildItemsetFlag { get; set; }

            [Option('v', "view", Required = false, HelpText = "Extract and view the values of the item set table")]
            public bool ViewItemSetFlag { get; set; }

            [Option('f', "output-file", Required = false, HelpText = "File to extract itemset to")]
            public string ItemsetOutputFile { get; set; }

        }


    }
}
