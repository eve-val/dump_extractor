using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ExtractorLib;

namespace SQL_To_YAML_CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            StreamWriter w = new StreamWriter("C:\\test.yaml");
            SQLToYAML sty = new SQLToYAML("chrAttributes", w);
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
