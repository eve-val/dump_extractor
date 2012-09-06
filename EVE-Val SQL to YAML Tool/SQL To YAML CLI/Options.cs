using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using CommandLine.Text;

namespace SQL_To_YAML_CLI
{
    class Options
    {
        [Option(
            null, "connection_string",
            DefaultValue="Data Source=localhost; Initial Catalog=ebs_DATADUMP; Integrated Security=SSPI;",
            HelpText = "The database connection string to use.")]
        public string connectionString { get; set; }

        [Option(
            "t", "tables", DefaultValue="*",
            HelpText="Comma-delimited list of tables to extract, or '*' to extract them all. Defaults to '*'.")]
        public string tables { get; set; }

        [Option(
            null, "notification_percent", DefaultValue=5,
            HelpText="Controls what percentage of the input processed causes a progress update event.")]
        public int notificationPercent { get; set; }

        [Option("o", "output_file", Required=true, HelpText="The output filename.")]
        public string outputFile { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText
            {
                Heading = new HeadingInfo("CCP Dump Extractor v1.0.1"),
                Copyright = new CopyrightInfo("Silas Snider", 2012),
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true
            };
            help.AddPreOptionsLine("Usage: sql_to_yaml -o FILENAME");
            help.AddOptions(this);
            return help;
        }
    }
}
