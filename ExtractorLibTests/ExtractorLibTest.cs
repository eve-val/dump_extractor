using System;
using System.Text;
using System.IO;
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExtractorLib;
using YamlDotNet.RepresentationModel;

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
            SQLToYAML sty = new SQLToYAML(sw, dataLayer, 50);
            int progressNum = 0;
            bool finished = false;
            sty.MadeProgress += (s, e) => progressNum++;
            sty.ExtractionFinished += (s, e) => finished = true;
            sty.ConvertSQLToYAML();
            StringReader sr = new StringReader(sw.ToString());

            var yaml = new YamlStream();
            yaml.Load(sr);
            var table1 = (YamlMappingNode)yaml.Documents[0].RootNode;
            var table2 = (YamlMappingNode)yaml.Documents[1].RootNode;

            Assert.IsTrue(finished);
            Assert.AreEqual(2, progressNum);
            Assert.AreEqual(table1.Children[new YamlScalarNode("table_name")].ToString(), "tstTestTable1");
            Assert.AreEqual(table2.Children[new YamlScalarNode("table_name")].ToString(), "tstTestTable2");
            var table1_column = (YamlSequenceNode)table1.Children[new YamlScalarNode("columns")];
            var table2_column = (YamlSequenceNode)table2.Children[new YamlScalarNode("columns")];
            Assert.AreEqual(table1_column.Children[0].ToString(), "col1");
            Assert.AreEqual(table1_column.Children[1].ToString(), "col2");
            Assert.AreEqual(table2_column.Children[0].ToString(), "col1");
            Assert.AreEqual(table2_column.Children[1].ToString(), "col2");
            var table1_data = (YamlSequenceNode)table1.Children[new YamlScalarNode("data")];
            var table2_data = (YamlSequenceNode)table2.Children[new YamlScalarNode("data")];
            Assert.AreEqual(((YamlMappingNode)table1_data.Children[0]).Children[new YamlScalarNode("col1")], new YamlScalarNode("value1"));
            Assert.AreEqual(((YamlMappingNode)table1_data.Children[0]).Children[new YamlScalarNode("col2")], new YamlScalarNode("2"));
            Assert.AreEqual(((YamlMappingNode)table2_data.Children[0]).Children[new YamlScalarNode("col1")], new YamlScalarNode("1"));
            Assert.AreEqual(((YamlMappingNode)table2_data.Children[0]).Children[new YamlScalarNode("col2")], new YamlScalarNode("true"));
        }

        [TestMethod]
        public void enumeratedTest()
        {
            StringWriter sw = new StringWriter();
            TestDataLayer dataLayer = new TestDataLayer();
            SQLToYAML sty = new SQLToYAML("tstTestTable1,tstTestTable2", sw, dataLayer, 50);
            int progressNum = 0;
            bool finished = false;
            sty.MadeProgress += (s, e) => progressNum++;
            sty.ExtractionFinished += (s, e) => finished = true;
            sty.ConvertSQLToYAML();
            StringReader sr = new StringReader(sw.ToString());

            var yaml = new YamlStream();
            yaml.Load(sr);
            var table1 = (YamlMappingNode)yaml.Documents[0].RootNode;
            var table2 = (YamlMappingNode)yaml.Documents[1].RootNode;

            Assert.IsTrue(finished);
            Assert.AreEqual(2, progressNum);
            Assert.AreEqual(table1.Children[new YamlScalarNode("table_name")].ToString(), "tstTestTable1");
            Assert.AreEqual(table2.Children[new YamlScalarNode("table_name")].ToString(), "tstTestTable2");
            var table1_column = (YamlSequenceNode)table1.Children[new YamlScalarNode("columns")];
            var table2_column = (YamlSequenceNode)table2.Children[new YamlScalarNode("columns")];
            Assert.AreEqual(table1_column.Children[0].ToString(), "col1");
            Assert.AreEqual(table1_column.Children[1].ToString(), "col2");
            Assert.AreEqual(table2_column.Children[0].ToString(), "col1");
            Assert.AreEqual(table2_column.Children[1].ToString(), "col2");
            var table1_data = (YamlSequenceNode)table1.Children[new YamlScalarNode("data")];
            var table2_data = (YamlSequenceNode)table2.Children[new YamlScalarNode("data")];
            Assert.AreEqual(((YamlMappingNode)table1_data.Children[0]).Children[new YamlScalarNode("col1")], new YamlScalarNode("value1"));
            Assert.AreEqual(((YamlMappingNode)table1_data.Children[0]).Children[new YamlScalarNode("col2")], new YamlScalarNode("2"));
            Assert.AreEqual(((YamlMappingNode)table2_data.Children[0]).Children[new YamlScalarNode("col1")], new YamlScalarNode("1"));
            Assert.AreEqual(((YamlMappingNode)table2_data.Children[0]).Children[new YamlScalarNode("col2")], new YamlScalarNode("true"));
        }
    }
}
