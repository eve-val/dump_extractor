using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace ExtractorLib
{
    public delegate void MadeProgressEventHandler(object sender, ExtractorLib.ProgressEventArgs e);
    public delegate void ExtractionFinishedEventHandler(object sender, EventArgs e);

    public class ExtractorLib
    {
        private string[] tablesToExtract;
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

        public ExtractorLib(string tables, TextWriter outputFile, ISQLAbstraction dataLayer, int notificationPercent = 5)
        {
            rows = -1;
            this.outputFile = outputFile;
            this.notificationPercent = notificationPercent;
            this.dataLayer = dataLayer;
            if (tables == "*")
            {
                tablesToExtract = dataLayer.TableList;
            }
            else
            {
                tablesToExtract = tables.Split(',');
            }
        }

        public ExtractorLib(TextWriter outputFile, ISQLAbstraction dataLayer, int notificationPercent = 5)
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

        public virtual void Convert()
        {
            int progress_mod = (int)Math.Ceiling((RowCount / 100.0) * notificationPercent);
            long numRows = 0;

            foreach (string t in tablesToExtract)
            {
                DataTable dataTable = dataLayer.RunQuery("SELECT * FROM " + t);
                if (dataTable.Rows.Count == 0) continue;

                Dictionary<String, object> table = new Dictionary<string, object>();
                table["table_name"] = t;
                
                List<string> columns = new List<string>();

                foreach (DataColumn column in dataTable.Columns)
                {
                    columns.Add(column.ColumnName);
                }

                table["columns"] = columns;
                table["data"] = new List<Dictionary<String, object>>();

                foreach (DataRow row in dataTable.Rows)
                {
                    Dictionary<String, object> row_dict = new Dictionary<String, object>();
                    numRows++;

                    for (int i = 0; i < columns.Count(); i++ )
                    {
                        if (row[columns[i]].Equals(System.DBNull.Value))
                        {
                            continue;
                        }
                        row_dict[columns[i]] = row[columns[i]]; 
                    }
                    if ((numRows % progress_mod) == 0)
                    {
                        OnMadeProgress(numRows);
                    }
                    ((List<Dictionary<String, object>>) table["data"]).Add(row_dict);
                }
                outputFile.WriteLine("---");
                outputFile.WriteLine(JsonConvert.SerializeObject(table));
                outputFile.WriteLine("...");
                outputFile.Flush();
            }
            outputFile.Flush();
            OnExtractionFinished(EventArgs.Empty);
        }
    }
}
