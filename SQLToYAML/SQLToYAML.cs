using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Data;
using System.Data.SqlClient;

namespace SQLToYAML
{
    public delegate void MadeProgressEventHandler(object sender, EventArgs e);
    public delegate void ExtractionFinishedEventHandler(object sender, EventArgs e);

    public class SQLToYAML
    {
        private string table;
        private TextWriter outputFile;
        private int notificationPercent;
        private SqlConnection conn;

        public event MadeProgressEventHandler MadeProgress;
        public event ExtractionFinishedEventHandler ExtractionFinished;

        public SQLToYAML(string table, TextWriter outputFile, int notificationPercent = 5)
        {
            this.table = table;
            this.outputFile = outputFile;
            this.notificationPercent = notificationPercent;
            this.conn = new SqlConnection("Data Source=localhost; Initial Catalog=ebs_DATADUMP; Integrated Security=SSPI;");
        }

        public SQLToYAML(TextWriter outputFile, int notificationPercent = 5) : this("*", outputFile, notificationPercent)
        {
        }

        public ~SQLToYAML()
        {
            if (conn != null)
            {
                conn.Close();
            }
                
        }

        protected virtual void OnMadeProgress(EventArgs e)
        {
            if (MadeProgress != null)
            {
                MadeProgress(this, e);
            }
        }

        protected virtual void OnExtractionFinished(EventArgs e)
        {
            if (ExtractionFinished != null)
            {
                ExtractionFinished(this, e);
            }
        }

        public List<string> ListTables()
        {
            DataTable t = conn.GetSchema("Tables");
            //TODO(swsnider): Extract table names from the DataTable.
            return null;
        }

        protected virtual void ConvertSQLToYAML()
        {
            //TODO(swsnider): Do stuff
        }

        public void StartProcessing()
        {
            Thread workerThread = new Thread(this.ConvertSQLToYAML);
        }
    }
}
