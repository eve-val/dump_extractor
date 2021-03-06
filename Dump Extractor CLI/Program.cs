﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ExtractorLib;
using CommandLine;

namespace Dump_Extractor_CLI
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
            DataLayer d;
            try
            {
                d = new DataLayer(options.connectionString);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return;
            }
            ExtractorLib.ExtractorLib sty = new ExtractorLib.ExtractorLib(options.tables, w, d, options.notificationPercent);
            sty.MadeProgress += new MadeProgressEventHandler(sty_MadeProgress);
            sty.ExtractionFinished += new ExtractionFinishedEventHandler(sty_ExtractionFinished);
            sty.Convert();
        }

        static void sty_ExtractionFinished(object sender, EventArgs e)
        {
            Console.WriteLine("Finished");
        }

        static void sty_MadeProgress(object sender, ExtractorLib.ExtractorLib.ProgressEventArgs e)
        {
            Console.Write(".");
        }
    }
}
