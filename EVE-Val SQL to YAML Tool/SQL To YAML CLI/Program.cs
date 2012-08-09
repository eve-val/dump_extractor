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
            StringWriter w = new StringWriter();
            SQLToYAML sty = new SQLToYAML(w);
            foreach (string table_name in sty.ListTables())
            {
                Console.WriteLine(table_name);
            }
        }
    }
}
