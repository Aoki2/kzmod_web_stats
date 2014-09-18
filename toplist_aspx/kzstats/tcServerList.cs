using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Drawing;
using System.Web;
using System.Net;
using System.IO;
using MySql.Data.MySqlClient;
using System.Collections;

//Class that contains the list of servers that can add entries to the database
namespace classes
{
    public sealed class tcServerList
    {
        private static readonly tcServerList mcSelf = new tcServerList();
        private bool meInitialized = false;
        private List<tcServerInfo> mcServerList = new List<tcServerInfo>();

        private class tcServerInfo
        {
            public string mpanAuthKey;
            public string mpanIp;
            public int mnDynamicIp;

            public tcServerInfo(string apanAuthKey, string apanIp, int anDynamicIp)
            {
                mpanAuthKey = apanAuthKey;
                mpanIp = apanIp;
                mnDynamicIp = anDynamicIp;
            }
        }

        public static tcServerList Instance
        {
            get
            {
                 return mcSelf;
            }
        }

        private tcServerList()
        {
            if (meInitialized == false)
            {
                InitializeServerListFromSqlDatabase();
                meInitialized = true;
            }
        }

        public void InitializeServerListFromSqlDatabase()
        {
            mcServerList.Clear();

            tcMySqlCommand lcMySqlCommand = new tcMySqlCommand(@"SELECT auth_key,ip,dynamic_ip FROM kzs_servers;");

            MySqlDataReader lcMySqlReader = lcMySqlCommand.mcMySqlCommand.ExecuteReader();

            while(lcMySqlReader.HasRows && !lcMySqlReader.IsClosed && lcMySqlReader.Read())
            {
                string lpanAuthKey = lcMySqlReader.GetString(0);
                string lpanIp = lcMySqlReader.GetString(1);
                int lnDynamicIp = lcMySqlReader.GetInt32(2);

                AddServer(lpanAuthKey, lpanIp, lnDynamicIp);
            }

            lcMySqlReader.Close();

        }

        public void AddServer(string apanAuthKey,string apanIp, int anDynamicIp)
        {
            mcServerList.Add(new tcServerInfo(apanAuthKey, apanIp, anDynamicIp));
        }

        public bool IsServerValid(string apanAuthKey, string apanIp)
        {
            bool leReturn = false;

            foreach (tcServerInfo lcServerInfo in mcServerList)
            {
                //Only need to check the auth key if the server is allowed to have a dynamic IP address
                if (lcServerInfo.mnDynamicIp == 1)
                {
                    if (apanAuthKey.Equals(lcServerInfo.mpanAuthKey))
                    {
                        leReturn = true;
                    }
                }
                else
                {
                    if (apanAuthKey.Equals(lcServerInfo.mpanAuthKey) && apanIp.Equals(lcServerInfo.mpanIp))
                    {
                        leReturn = true;
                    }
                }
            }

            return leReturn;
        }
    }
}
