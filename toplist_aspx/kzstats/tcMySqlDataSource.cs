using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.Configuration;
using System.Data;
using System.Data.Odbc;
using System.Web.Configuration;

namespace classes
{
    public class tcMySqlDataSource
    {
        private MySqlConnection mcMySqlConnection;
        private string mpanQuery;
        
        public tcMySqlDataSource(string apanSelect)
        {
            string lpanConnectionString = StaticMethods.GetConnectionString();
            mcMySqlConnection = new MySqlConnection(lpanConnectionString);

            mpanQuery = apanSelect;
        }

        public DataTable GetDataTable()
        {
            DataTable lcReturn = null;
            
            try
            {
                MySqlDataAdapter lcSqlDataAdapter = new MySqlDataAdapter(mpanQuery, mcMySqlConnection);

                if (mcMySqlConnection.State != ConnectionState.Open)
                {
                    mcMySqlConnection.Open();
                }

                if (mcMySqlConnection.State == ConnectionState.Open)
                {
                    lcReturn = new DataTable();

                    lcSqlDataAdapter.Fill(lcReturn);

                    mcMySqlConnection.Close();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("tcMySqlDataSource error: " + e.Message);
            }

            return lcReturn;
        }
    }
    
    public class tcMySqlCommand
    {
        private MySqlConnection mcMySqlConnection;

        public MySqlCommand mcMySqlCommand;

        public tcMySqlCommand(string apanSelect)
        {
            string lpanConnectionString = StaticMethods.GetConnectionString();

            try
            {
                mcMySqlConnection = new MySqlConnection(lpanConnectionString);

                mcMySqlCommand = mcMySqlConnection.CreateCommand();
                mcMySqlCommand.CommandText = apanSelect;

                mcMySqlConnection.Open();
            }
            catch (Exception e)
            {
                Debug.WriteLine("tcMySqlCommand error : " + e.Message);
            }
        }

        public void Close()
        {
            if (mcMySqlConnection.State != ConnectionState.Closed)
            {
                mcMySqlConnection.Close();
            }
        }
    }
}
