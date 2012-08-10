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

    public class SQLToYAML : IDisposable
    {
        private string[] tablesToExtract;
        private string table;
        private TextWriter outputFile;
        private int notificationPercent;
        private SqlConnection conn;
        private string[] tableCache;
        private long rows;

        public class ProgressEventArgs : EventArgs
        {
            public ProgressEventArgs(long progress)
            {
                this.progress = progress;
            }
            public long progress;
        }

        public string ConnectionString
        {
            get
            {
                return conn.ConnectionString;
            }
            set
            {
                if (conn == null)
                {
                    conn = new SqlConnection(value);
                }
                else
                {
                    if (conn.State != ConnectionState.Closed)
                    {
                        conn.Close();
                    }
                    conn.ConnectionString = value;
                }
                conn.Open();
                if (table == "*")
                {
                    tablesToExtract = TableList;
                }
                else
                {
                    tablesToExtract = new string[] { table };
                }
            }
        }

        public string[] TableList
        {
            get
            {
                if (tableCache == null)
                {
                    tableCache = this.ListTables().ToArray();
                }
                return tableCache;
            }
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
                    SqlCommand queryCommand = new SqlCommand(qry, conn);
                    SqlDataReader queryCommandReader = queryCommand.ExecuteReader();
                    DataTable t = new DataTable();
                    t.Load(queryCommandReader);
                    rows = (int)t.Rows[0][0];
                    queryCommandReader.Close();
                }
                return rows;
            }
        }

        public event MadeProgressEventHandler MadeProgress;
        public event ExtractionFinishedEventHandler ExtractionFinished;

        public SQLToYAML(string table, TextWriter outputFile, int notificationPercent = 5)
        {
            rows = -1;
            this.outputFile = outputFile;
            this.notificationPercent = notificationPercent;
            this.table = table;
            this.tableCache = null;
        }

        public SQLToYAML(TextWriter outputFile, int notificationPercent = 5)
            : this("*", outputFile, notificationPercent)
        {
        }

        public void Dispose()
        {
            if (conn.State != ConnectionState.Closed)
            {
                conn.Close();
            }

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

        protected virtual List<string> ListTables()
        {
            DataTable t = conn.GetSchema("Tables");
            List<string> result = new List<string>();
            foreach (DataRow row in t.Rows)
            {
                result.Add(row["TABLE_NAME"].ToString());
            }
            return result;
        }

        protected virtual void ConvertSQLToYAML()
        {
            int progress_mod = (int)Math.Ceiling((RowCount / 100.0) * notificationPercent);
            long numRows = 0;
            outputFile.WriteLine("database:");
            foreach (string t in tablesToExtract)
            {
                SqlCommand queryCommand = new SqlCommand("SELECT * FROM " + t, conn);
                SqlDataReader queryCommandReader = queryCommand.ExecuteReader();
                DataTable dataTable = new DataTable();
                dataTable.Load(queryCommandReader);

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

        public void StartProcessing()
        {
            Thread workerThread = new Thread(this.ConvertSQLToYAML);
            workerThread.Start();
        }
    }
}
