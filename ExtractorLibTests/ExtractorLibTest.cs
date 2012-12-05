using System;
using System.Text;
using System.IO;
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExtractorLib;

namespace ExtractorLibTests
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

    [TestClass]
    public class ExtractorLibTest
    {
        [TestMethod]
        public void testExtraction()
        {
            StringWriter sw = new StringWriter();
            TestDataLayer dataLayer = new TestDataLayer();
            ExtractorLib.ExtractorLib sty = new ExtractorLib.ExtractorLib(sw, dataLayer, 50);
            int progressNum = 0;
            bool finished = false;
            sty.MadeProgress += (s, e) => progressNum++;
            sty.ExtractionFinished += (s, e) => finished = true;
            sty.Convert();
            Console.WriteLine(sw.ToString());
        }

        [TestMethod]
        public void enumeratedTest()
        {
            StringWriter sw = new StringWriter();
            TestDataLayer dataLayer = new TestDataLayer();
            ExtractorLib.ExtractorLib sty = new ExtractorLib.ExtractorLib("tstTestTable1,tstTestTable2", sw, dataLayer, 50);
            int progressNum = 0;
            bool finished = false;
            sty.MadeProgress += (s, e) => progressNum++;
            sty.ExtractionFinished += (s, e) => finished = true;
            sty.Convert();
            StringReader sr = new StringReader(sw.ToString());
        }
    }
}
