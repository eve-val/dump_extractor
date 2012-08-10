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
            SQLToYAML sty = new SQLToYAML(options.table, w, options.notificationPercent);
            sty.ConnectionString = options.connectionString;
            sty.MadeProgress += new MadeProgressEventHandler(sty_MadeProgress);
            sty.ExtractionFinished += new ExtractionFinishedEventHandler(sty_ExtractionFinished);
            sty.StartProcessing();
        }

        static void sty_ExtractionFinished(object sender, EventArgs e)
        {
            Console.WriteLine("Finished");
        }

        static void sty_MadeProgress(object sender, SQLToYAML.ProgressEventArgs e)
        {
            Console.Write(".");
        }
    }
}
