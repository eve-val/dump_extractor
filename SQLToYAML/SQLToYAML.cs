using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Data;
using System.Data.SqlClient;

namespace ExtractorLib
{
    public delegate void MadeProgressEventHandler(object sender, SQLToYAML.ProgressEventArgs e);
    public delegate void ExtractionFinishedEventHandler(object sender, EventArgs e);

    public class SQLToYAML
    {
        private string[] tablesToExtract;
        private string table;
        private TextWriter outputFile;
        private int notificationPercent;
        private long rows;
        private ISQLAbstraction dataLayer;

        public class ProgressEventArgs : EventArgs
        {
            public ProgressEventArgs(long progress)
            {
                this.progress = progress;
            }
            public long progress;
        }

        public long RowCount
        {
            get
            {
                if (rows < 0)
                {
                    string inExpr = "'" + String.Join("','", tablesToExtract) + "'";
                    string qry = "SELECT SUM(sysindexes.rows) FROM sysobjects " +
                                 "INNER JOIN sysindexes ON sysobjects.id = " +
                                 "sysindexes.id WHERE sysobjects.type = 'U' AND " +
                                 "sysindexes.IndID < 2 AND sysobjects.name IN (" +
                                 inExpr + ");";
                    DataTable t = dataLayer.RunQuery(qry);
                    rows = (int)t.Rows[0][0];
                }
                return rows;
            }
        }

        public event MadeProgressEventHandler MadeProgress;
        public event ExtractionFinishedEventHandler ExtractionFinished;

        public SQLToYAML(string table, TextWriter outputFile, ISQLAbstraction dataLayer, int notificationPercent = 5)
        {
            rows = -1;
            this.outputFile = outputFile;
            this.notificationPercent = notificationPercent;
            this.table = table;
            this.dataLayer = dataLayer;
            if (table == "*")
            {
                tablesToExtract = dataLayer.TableList;
            }
            else
            {
                tablesToExtract = new string[] { table };
            }
        }

        public SQLToYAML(TextWriter outputFile, ISQLAbstraction dataLayer, int notificationPercent = 5)
            : this("*", outputFile, dataLayer, notificationPercent)
        {
        }

        protected virtual void OnMadeProgress(long progress)
        {
            if (MadeProgress != null)
            {
                MadeProgress(this, new ProgressEventArgs(progress));
            }
        }

        protected virtual void OnExtractionFinished(EventArgs e)
        {
            if (ExtractionFinished != null)
            {
                ExtractionFinished(this, e);
            }
        }

        public virtual void ConvertSQLToYAML()
        {
            int progress_mod = (int)Math.Ceiling((RowCount / 100.0) * notificationPercent);
            long numRows = 0;
            outputFile.WriteLine("database:");
            foreach (string t in tablesToExtract)
            {
                DataTable dataTable = dataLayer.RunQuery("SELECT * FROM " + t);

                outputFile.WriteLine("  - table_name: " + t);
                outputFile.WriteLine("    columns:");
                
                List<string> columns = new List<string>();

                foreach (DataColumn column in dataTable.Columns)
                {
                    outputFile.WriteLine("      - " + column.ColumnName);
                    columns.Add(column.ColumnName);
                }

                outputFile.WriteLine("    data:");

                foreach (DataRow row in dataTable.Rows)
                {
                    numRows++;
                    string[] data = new string[columns.Count()];
                    for (int i = 0; i < columns.Count(); i++ )
                    {
                        string c = YAMLConversion.ConvertValue((dynamic)row[columns[i]]);
                        if (c == null) continue;
                        data[i] = columns[i] + ": " + c;
                    }
                    string joinedString = String.Join("\r\n        ", data);
                    joinedString = joinedString.Replace("\r\n        \r\n", "\r\n");
                    outputFile.WriteLine("      - " + joinedString);
                    if ((numRows % 100) == 0)
                    {
                        outputFile.Flush();
                    }
                    if ((numRows % progress_mod) == 0)
                    {
                        OnMadeProgress(numRows);
                    }
                }
            }
            outputFile.Flush();
            OnExtractionFinished(EventArgs.Empty);
        }
    }
}
