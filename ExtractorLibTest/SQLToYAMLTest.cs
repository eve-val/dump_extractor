using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using ExtractorLib;

namespace ExtractorLibTest
{
    class TestDataLayer : ISQLAbstraction
    {
        public string[] TableList
        {
            get
            {
                return new string[] { "tstTestTable1", "tstTestTable2" };
            }
        }

        public DataTable RunQuery(string qry)
        {
            DataTable result = new DataTable();
            if (qry.IndexOf("sysindexes") != -1)
            {
                result.Columns.Add("unamed", typeof(int));
                DataRow row = result.NewRow();
                row["unamed"] = (int)2;
                result.Rows.Add(row);
            }
            else if (qry.IndexOf("tstTestTable1") != -1)
            {
                result.Columns.Add("col1", typeof(string));
                result.Columns.Add("col2", typeof(int));
                DataRow row = result.NewRow();
                row["col1"] = "value1";
                row["col2"] = 2;
                result.Rows.Add(row);
            }
            else
            {
                result.Columns.Add("col1", typeof(float));
                result.Columns.Add("col2", typeof(bool));
                DataRow row = result.NewRow();
                row["col1"] = 1.0;
                row["col2"] = true;
                result.Rows.Add(row);
            }
            result.AcceptChanges();
            return result;
        }
    }

    [TestFixture]
    class SQLToYAMLTest
    {
        [Test]
        public void simpleTest()
        {
            StringWriter sw = new StringWriter();
            TestDataLayer dataLayer = new TestDataLayer();
            SQLToYAML sty = new SQLToYAML(sw, dataLayer, 50);
            int progressNum = 0;
            bool finished = false;
            sty.MadeProgress += (s, e) => progressNum++;
            sty.ExtractionFinished += (s, e) => finished = true;
            sty.ConvertSQLToYAML();

            StringBuilder o = new StringBuilder();
            o.AppendLine("database:");
            o.AppendLine("  - table_name: tstTestTable1");
            o.AppendLine("    columns:");
            o.AppendLine("      - col1");
            o.AppendLine("      - col2");
            o.AppendLine("    data:");
            o.AppendLine("      - col1: \"value1\"");
            o.AppendLine("        col2: 2");
            o.AppendLine("  - table_name: tstTestTable2");
            o.AppendLine("    columns:");
            o.AppendLine("      - col1");
            o.AppendLine("      - col2");
            o.AppendLine("    data:");
            o.AppendLine("      - col1: !!float 1");
            o.AppendLine("        col2: Yes");

            Assert.IsTrue(finished);
            Assert.AreEqual(2, progressNum);
            Assert.AreEqual(o.ToString(), sw.ToString());
        }

    }
}
