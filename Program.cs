using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SimpleCppLinter
{
    internal class SimpleCppLinter
    {
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No input. Please pass either the path to a file or a string.");
                return 1;
            }

            string CppText = args[0];


            string[] SupportedFormats = { ".cpp", ".h", ".c", ".txt" };
            bool bIsFile = false;
            foreach (string SupportedFormat in SupportedFormats)
            {
                if (!CppText.EndsWith(SupportedFormat, StringComparison.InvariantCultureIgnoreCase))
                    continue;

                bIsFile = true;
                break;
            }
            if (bIsFile && File.Exists(CppText))
            {
                CppText = File.ReadAllText(CppText);
            }

            if (CppText == null || CppText.Length == 0)
            {
                Console.WriteLine("Provided input is empty.");
                return 1;
            }

            var MySegmentBuilder = new SegmentBuilder(CppText);
            List<string> Errors = new List<string>();
            List<string> Warnings = new List<string>();
            var SegmentsWithErrors = MySegmentBuilder.Validate(Errors, Warnings);
            int NumErrors = Errors.Count;
            int NumWarnings = Warnings.Count;
            if (NumErrors > 0)
            {
                Console.WriteLine(String.Format("Found {0} error{1}.", NumErrors, NumErrors != 1 ? "s" : ""));
                foreach (string Error in Errors)
                {
                    Console.WriteLine(Error);
                }
            }
            if (NumWarnings > 0)
            {
                Console.WriteLine(String.Format("Found {0} warning{1}.", NumWarnings, NumWarnings != 1 ? "s" : ""));
                foreach (string Warning in Warnings)
                {
                    Console.WriteLine(Warning);
                }
            }
            if (NumErrors > 0 || NumWarnings > 0)
            {
                return 1;
            }

            Console.Write("Parsing complete.");
            return 0;
        }
    }
}