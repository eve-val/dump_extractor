using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ExtractorLib;
using CommandLine;

namespace SQL_To_YAML_CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            Options options = new Options();
            if (!CommandLineParser.Default.ParseArguments(args, options))
            {
                return;
            }
            StreamWriter w = new StreamWriter(options.outputFile);
            DataLayer d = new DataLayer(options.connectionString);
            ExtractorLib sty = new ExtractorLib(options.tables, w, d, options.notificationPercent);
            sty.MadeProgress += new MadeProgressEventHandler(sty_MadeProgress);
            sty.ExtractionFinished += new ExtractionFinishedEventHandler(sty_ExtractionFinished);
            sty.ConvertSQLToYAML();
        }

        static void sty_ExtractionFinished(object sender, EventArgs e)
        {
            Console.WriteLine("Finished");
        }

        static void sty_MadeProgress(object sender, ExtractorLib.ProgressEventArgs e)
        {
            Console.Write(".");
        }
    }
}
