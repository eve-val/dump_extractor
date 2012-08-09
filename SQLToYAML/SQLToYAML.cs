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
            ConnectionString = "Data Source=localhost; Initial Catalog=ebs_DATADUMP; Integrated Security=SSPI;";
            if (table == "*")
            {
                tablesToExtract = TableList;
            }
            else
            {
                tablesToExtract = new string[] { table };
            }
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
                //Step 1: SELECT * FROM t;
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

                //Step 2: loop over results, streaming each one to the writer and flushing.
                foreach (DataRow row in dataTable.Rows)
                {
                    //TODO(swsnider): compute progress/fire events.
                    numRows++;
                    string[] data = new string[columns.Count()];
                    for (int i = 0; i < columns.Count(); i++ )
                    {
                        data[i] = columns[i] + ": " + row[columns[i]];
                        data[i] = data[i].Replace("\n", "\\n").Replace("\r", "\\r");
                    }
                    outputFile.WriteLine("      - " + String.Join("\r\n        ", data));
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
