using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ExtractorLib
{
    public interface ISQLAbstraction
    {
        string[] TableList
        {
            get;
        }

        DataTable RunQuery(string qry);
    }

    public class DataLayer : IDisposable, ISQLAbstraction
    {
        private SqlConnection conn;
        private string[] tableCache;

        public DataLayer(string connectionString)
        {
            conn = new SqlConnection(connectionString);
            conn.Open();
        }

        public string[] TableList
        {
            get
            {
                if (tableCache == null)
                {
                    DataTable t = conn.GetSchema("Tables");
                    List<string> result = new List<string>();
                    foreach (DataRow row in t.Rows)
                    {
                        result.Add(row["TABLE_NAME"].ToString());
                    }
                    tableCache = result.ToArray();
                }
                return tableCache;
            }
        }

        public string ConnectionString
        {
            get
            {
                return conn.ConnectionString;
            }
            set
            {
               
            }
        }

        public void Dispose()
        {
            if (conn.State != ConnectionState.Closed)
            {
                conn.Close();
            }

        }

        public DataTable RunQuery(string qry)
        {
            SqlCommand queryCommand = new SqlCommand(qry, conn);
            SqlDataReader queryCommandReader = queryCommand.ExecuteReader();
            DataTable t = new DataTable();
            t.Load(queryCommandReader);
            queryCommandReader.Close();
            return t;
        }
    }
}
