using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Reflection;
using System.IO;
using System.Data;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using classes;
using System.Drawing;
using System.Globalization;

namespace kzstats
{
    public partial class submit : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                string lpanKey = GetRequestString("key");
                string lpanSel = GetRequestString("sel");

                if(lpanSel.Equals("create_tables"))
                {
                    SqlCreateTables();
                }
                else if (lpanSel.Equals("add_server"))
                {
                    if (IsAuthKeyValid(lpanKey))
                    {
                        SqlAddServer(lpanKey);
                    }
                }
                //Else if the server is registered and valid
                else if (tcServerList.Instance.IsServerValid(lpanKey, HttpContext.Current.Request.UserHostAddress))
                {
                    switch (lpanSel)
                    {
                        case "add_map":
                            SqlAddMap();
                            break;
                        case "add_player":
                            SqlAddPlayer();
                            break;
                        case "update_medals":
                            SqlUpdateMedals();
                            break;
                        case "update_player_info":
                            SqlUpdatePlayerInfo();
                            break;
                        case "add_record":
                            SqlAddRecord(lpanKey);
                            break;
                        case "medal_spend":
                            SqlUpdateRewardPurchases();
                            break;
                        case "add_tag":
                            SqlAddTag();
                            break;
                        case "tag_round_end":
                            SqlTagRoundEnd();
                            break;
                        case "tag_map_end":
                            SqlTagMatchEnd();
                            break;
                        default:
                            Debug.WriteLine("Invalid selection: " + lpanSel);
                            break;
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Page load: " + ex.Message);
            }
        }

        private void SqlTagMatchEnd()
        {
            string lpanSteamId = GetRequestString("auth");
            int lnPlayerId = GetPlayerIdFromAuth(lpanSteamId);

            if (lnPlayerId > 0)
            {
                MySqlRunNonQuery("UPDATE kzs_tag SET match_wins = match_wins + 1 WHERE player = " + lnPlayerId + ";");
            }
        }

        private void SqlTagRoundEnd()
        {
            string lpanSteamId = GetRequestString("auth");
            int lnWinner = GetRequestInt("winner");
            int lnRoundUntaggedTime = GetRequestInt("round_untagged_time");
            int lnRoundPowerups = GetRequestInt("round_powerups");
            int lnRoundTags = GetRequestInt("round_tags");
            int lnRoundTagged = GetRequestInt("round_tagged");

            int lnPlayerId = GetPlayerIdFromAuth(lpanSteamId);

            if (lnPlayerId > 0)
            {
                MySqlRunNonQuery("UPDATE kzs_tag SET rounds_played = rounds_played + 1, round_wins = round_wins + " + lnWinner + ",total_untagged_time = total_untagged_time + " +
                    lnRoundUntaggedTime + ", round_most_untagged = GREATEST(round_most_untagged," + lnRoundUntaggedTime + "), round_least_untagged = LEAST(round_least_untagged," +
                    lnRoundUntaggedTime + "), round_most_powerup = GREATEST(round_most_powerup," + lnRoundPowerups + "), round_least_powerup = LEAST(round_least_powerup" + 
                    lnRoundPowerups + ") WHERE player = " + lnPlayerId + ";");

                MySqlRunNonQuery("UPDATE kzs_tag SET avg_tags_per_round = ((avg_tags_per_round*(rounds_played-1))+" + 
                    lnRoundTags + ")/rounds_played, avg_tagged_per_round = ((avg_tagged_per_round*(rounds_played-1))+"+
                    lnRoundTagged+")/rounds_played, avg_powerup_per_round = ((avg_powerup_per_round*(rounds_played-1))+"+
                    lnRoundPowerups+")/rounds_played WHERE player = "+lnPlayerId+";");
            }
        }

        private void SqlAddTag()
        {
            string lpanTaggerSteamId = GetRequestString("tagger_auth");
            string lpanTaggedSteamId = GetRequestString("tagged_auth");
            int lnNinjaTag = GetRequestInt("ninja");

            int lnTaggerId = GetPlayerIdFromAuth(lpanTaggerSteamId);
            int lnTaggedId = GetPlayerIdFromAuth(lpanTaggedSteamId);

            if (lnTaggerId > 0)
            {
                MySqlRunNonQuery("UPDATE kzs_tag SET tags = tags + 1, ninja = ninja + " + lnNinjaTag + " WHERE player = " + lnTaggerId + ";");
            }

            if (lnTaggedId > 0)
            {
                MySqlRunNonQuery("UPDATE kzs_tag SET tagged = tagged + 1 WHERE player = " + lnTaggedId + ";");
            }
        }

        private void SqlUpdateRewardPurchases()
        {
            string lpanSteamId = GetRequestString("auth");
            string lpanRewardName = GetRequestString("item");
            int lnPlayerId = GetPlayerIdFromAuth(lpanSteamId);

            MySqlRunNonQuery("UPDATE kzs_medals SET " + lpanRewardName + " = " + lpanRewardName + " + 1 WHERE player = " + lnPlayerId + ";");
        }

        private void SqlAddRecord(string apanKey)
        {
            string lpanSteamId = GetRequestString("auth");
            string lpanMapName = GetRequestString("map");
            string lpanCourseName = GetRequestString("course");
            int lnTeles = GetRequestInt("tele");
            int lnCheckpoints = GetRequestInt("cp");
            int lnJumps = GetRequestInt("jumps");
            int lnFlags = GetRequestInt("flags");
            double lrTime = GetRequestDouble("time");
            int lnTeamCheck = GetRequestInt("teamcheck");
                        
            //Get the course ID and invert state
            int lnPlayerId = GetPlayerIdFromAuth(lpanSteamId);
            int lnMapId = GetMapIdFromName(lpanMapName);
            int lnServerId = GetServerIdFromKey(apanKey);

            //Ensure that the course exists in the database
            MySqlRunNonQuery("INSERT IGNORE INTO kzs_courses (map,name) VALUES(" + lnMapId + ",'" + lpanCourseName + "');");

            //Get course information and then add record and update run counts
            tcMySqlCommand lcMySqlCommand = new tcMySqlCommand(@"SELECT id, invert FROM kzs_courses WHERE name = '"+lpanCourseName+"' AND map = "+lnMapId+";");

            MySqlDataReader lcMySqlReader = lcMySqlCommand.mcMySqlCommand.ExecuteReader();
            lcMySqlReader.Read();

            if (lcMySqlReader.HasRows && lnPlayerId != -1 && lrTime > 0.0)
            {
                int lnCourseId = lcMySqlReader.GetInt32(0);
                int lnInvert = lcMySqlReader.GetInt32(1);

                //Add record if it does not exist, increment run counts
                if (lnTeles == 0 && lnTeamCheck == 0)
                {
                    MySqlRunNonQuery("INSERT IGNORE INTO kzs_records (player,map,course,time,jumps,flags,server,date) VALUES(" + lnPlayerId + "," + lnMapId + "," + lnCourseId + "," + lrTime + "," + lnJumps + "," + lnFlags + "," + lnServerId + ",CURDATE());");

                    MySqlRunNonQuery("UPDATE kzs_courses SET fin = fin + 1 WHERE id = " + lnCourseId + ";");

                    MySqlRunNonQuery("INSERT INTO kzs_runcounts (player,map,course,nocheck,cp) VALUES(" + lnPlayerId + "," + lnMapId + "," + lnCourseId + ",1,0) ON DUPLICATE KEY UPDATE nocheck = nocheck + 1;");
                }
                else
                {
                    MySqlRunNonQuery("INSERT IGNORE INTO kzs_recordscp (player,map,course,time,jumps,cp,tele,flags,server,date) VALUES(" + lnPlayerId + "," + lnMapId + "," + lnCourseId + "," + lrTime + "," + lnJumps + ","+lnCheckpoints+","+lnTeles+"," + lnFlags + "," + lnServerId + ",CURDATE());");

                    MySqlRunNonQuery("UPDATE kzs_courses SET fincp = fincp + 1 WHERE id = " + lnCourseId + ";");

                    MySqlRunNonQuery("INSERT INTO kzs_runcounts (player,map,course,nocheck,cp) VALUES(" + lnPlayerId + "," + lnMapId + "," + lnCourseId + ",0,1) ON DUPLICATE KEY UPDATE cp = cp + 1;");
                }

                //Update record if record already existed
                string lpanQuery = null;
                
                if(lnTeles == 0)
                {
                    lpanQuery = @"INSERT INTO kzs_records (SELECT * FROM kzs_records WHERE player = " + lnPlayerId + " AND course = " + lnCourseId + " AND server = " + lnServerId +
                        " AND time > " + lrTime + " AND flags = " + lnFlags + ") ON DUPLICATE KEY UPDATE time = " + lrTime + ", jumps = " + lnJumps + ", date = CURDATE();";
                }
                else
                {
                    lpanQuery = @"INSERT INTO kzs_recordscp (SELECT * FROM kzs_recordscp WHERE player = " + lnPlayerId + " AND course = " + lnCourseId +
                        " AND server = " + lnServerId + " AND time > " + lrTime + " AND flags = " + lnFlags + ") ON DUPLICATE KEY UPDATE time = " + lrTime + ", jumps = " + lnJumps + ", cp = " + lnCheckpoints + ", tele = " + lnTeles + ", date = CURDATE();";
                }

                if (lnInvert == 1)
                {
                    lpanQuery = lpanQuery.Replace("time >", "time <");
                }

                MySqlRunNonQuery(lpanQuery);
            }

            lcMySqlReader.Close();
        }

        private void SqlUpdatePlayerInfo()
        {
            string lpanSteamId = GetRequestString("auth");
            string lpanPlayerName = GetRequestString("name");
            string lpanIpAddress = GetRequestString("ip");
            string lpanCountryCode = GetRequestString("country");

            int lnPlayerId = GetPlayerIdFromAuth(lpanSteamId);

            if (lnPlayerId != -1)
            {
                MySqlRunNonQuery("INSERT INTO kzs_ips (player,ip,country,count) VALUES (" + lnPlayerId + ", '" + lpanIpAddress + "','"+lpanCountryCode+"',1) ON DUPLICATE KEY UPDATE count = count + 1;");
                MySqlRunNonQuery("INSERT INTO kzs_names (player,name,count) VALUES (" + lnPlayerId + ", '" + lpanPlayerName + "',1) ON DUPLICATE KEY UPDATE count = count + 1;");

                EvaluatePlayerNameFrequency(lnPlayerId);
            }
        }

        //Evaluate player's name usage and see if main name in players table should be updated
        private void EvaluatePlayerNameFrequency(int anPlayerId)
        {
            tcMySqlCommand lcMySqlCommand = new tcMySqlCommand(@"SELECT name FROM kzs_names WHERE player = " + anPlayerId + " ORDER BY count DESC LIMIT 1;");

            MySqlDataReader lcMySqlReader = lcMySqlCommand.mcMySqlCommand.ExecuteReader();
            lcMySqlReader.Read();

            if (lcMySqlReader.HasRows)
            {
                string lpanMostFrequentName = lcMySqlReader.GetString(0);
                MySqlRunNonQuery("UPDATE kzs_players SET name='" + lpanMostFrequentName + "' WHERE id=" + anPlayerId + ";");
            }

            lcMySqlReader.Close();
        }

        private int ValidateMedalCount(int anMedalCount)
        {
            if (anMedalCount >= 1)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        private void SqlUpdateMedals()
        {
            string lpanSteamId = GetRequestString("auth");
            int lnBronze = ValidateMedalCount(GetRequestInt("bronze"));
            int lnSilver = ValidateMedalCount(GetRequestInt("silver"));
            int lnGold = ValidateMedalCount(GetRequestInt("gold"));
            int lnPlatinum = ValidateMedalCount(GetRequestInt("platinum"));

            int lnPlayerId = GetPlayerIdFromAuth(lpanSteamId);

            if (lnPlayerId != -1)
            {
                MySqlRunNonQuery(@"UPDATE kzs_medals SET bronze = bronze + "+lnBronze+", silver = silver + "+lnSilver+", gold = gold + "+lnGold+", platinum = platinum + "+lnPlatinum+" WHERE player = " + lnPlayerId + ";");
            }
        }

        private void SqlAddPlayer()
        {
            string lpanPlayerName = GetRequestString("name");
            string lpanSteamId = GetRequestString("auth");
            string lpanIpAddress = GetRequestString("ip");
            string lpanCountryCode = GetRequestString("country");
            MySqlRunNonQuery(@"INSERT INTO kzs_players (name,auth,date,ip,country) VALUES ('" + lpanPlayerName + "','" + lpanSteamId + "',CURDATE(),'" + lpanIpAddress + "','" + lpanCountryCode + "') ON DUPLICATE KEY UPDATE country = '" + lpanCountryCode + "';");

            int lnPlayerId = GetPlayerIdFromAuth(lpanSteamId);

            if (lnPlayerId != -1)
            {
                MySqlRunNonQuery("INSERT IGNORE INTO kzs_medals (player) VALUES (" + lnPlayerId + ");");
                MySqlRunNonQuery("INSERT IGNORE INTO kzs_tag (player) VALUES (" + lnPlayerId + ");");
            }
            else
            {
                Debug.WriteLine("Failed to get played ID from auth " + lpanSteamId);
            }
        }
        
        private void SqlAddMap()
        {
            string lpanMapName = GetRequestString("map");
            MySqlRunNonQuery("INSERT INTO kzs_maps (name,runs,date) VALUES('" + lpanMapName + "',1,CURDATE()) ON DUPLICATE KEY UPDATE runs = runs + 1;");
        }

        private void SqlAddServer(string apanKey)
        {
            string lpanIp =  HttpContext.Current.Request.UserHostAddress;
            string lpanHostname = GetRequestString("hostname");
            string lnPort = GetRequestString("port");

            if (tcServerList.Instance.IsServerValid(apanKey, lpanIp) == false)
            {
                tcServerList.Instance.AddServer(apanKey, lpanIp, 0);
            }

            //TODO uncomment to allow new servers to be added if their keys are valid
            //MySqlRunNonQuery("INSERT IGNORE INTO kzs_servers (name,ip,port,date,auth_key) VALUES('" + lpanHostname + "','" + lpanIp + "'," + lnPort + ",CURDATE(),'" + apanKey + "');");

            MySqlRunNonQuery("UPDATE kzs_servers SET name = '" + lpanHostname + "' WHERE auth_key = '" + apanKey + "';");
        }

        private bool IsAuthKeyValid(string apanKey)
        {
            bool lbEqual = false;

            string lpanRand = apanKey.Substring(0,apanKey.IndexOf('-'));
            string lpanRandHash = StaticMethods.CalculateMD5Hash(lpanRand).Substring(0, 8);
            
            string lpanHash = apanKey.Substring(apanKey.IndexOf('-') + 1);

            if (lpanHash.Equals(lpanRandHash))
            {
                lbEqual = true;
            }

            return lbEqual;
        }

        private void SqlCreateTables()
        {
            MySqlRunNonQuery("SET NAMES \"UTF8\"");
            MySqlRunNonQuery("SET SQL_SAFE_UPDATES=0;");
            MySqlRunNonQuery("CREATE TABLE IF NOT EXISTS kzs_maps (id INT NOT NULL AUTO_INCREMENT, name VARCHAR(64) NOT NULL UNIQUE, runs INT NOT NULL, allow INT DEFAULT 1, date DATE NOT NULL,PRIMARY KEY (id));");
            MySqlRunNonQuery("CREATE TABLE IF NOT EXISTS kzs_courses (map INT NOT NULL, id INT NOT NULL AUTO_INCREMENT,name VARCHAR(64) NOT NULL,base_points INT DEFAULT 0, dyn_points INT DEFAULT 0, invert INT NOT NULL DEFAULT 0,fin INT NOT NULL DEFAULT 0,fincp INT NOT NULL DEFAULT 0,PRIMARY KEY (id),FOREIGN KEY (map) REFERENCES kzs_maps(id),CONSTRAINT mapcourse UNIQUE (map,name));");
            MySqlRunNonQuery("CREATE TABLE IF NOT EXISTS kzs_players (id INT NOT NULL AUTO_INCREMENT, name VARCHAR(65) NOT NULL,auth VARCHAR(23) NOT NULL UNIQUE, date DATE NOT NULL,ip VARCHAR(16) NOT NULL DEFAULT '',country VARCHAR(3) NOT NULL,PRIMARY KEY (id));");
            MySqlRunNonQuery("CREATE TABLE IF NOT EXISTS kzs_servers (id INT NOT NULL AUTO_INCREMENT, name VARCHAR(128) NOT NULL, auth_key VARCHAR(64) NOT NULL, allow INT DEFAULT 0, ip VARCHAR(32) NOT NULL, port INT NOT NULL, date DATE NOT NULL, dynamic_ip INT DEFAULT 0, PRIMARY KEY(id),CONSTRAINT UNIQUE(ip,port),CONSTRAINT UNIQUE(auth_key));");
            MySqlRunNonQuery("CREATE TABLE IF NOT EXISTS kzs_records (id INT NOT NULL AUTO_INCREMENT,player INT NOT NULL, map INT NOT NULL, course INT NOT NULL, time FLOAT NOT NULL, jumps INT NOT NULL, flags INT NOT NULL DEFAULT 0, server INT NOT NULL, date DATE NOT NULL,PRIMARY KEY(id),FOREIGN KEY (player) REFERENCES kzs_players(id),FOREIGN KEY (map) REFERENCES kzs_maps(id),FOREIGN KEY (course) REFERENCES kzs_courses(id),FOREIGN KEY (server) REFERENCES kzs_servers(id), CONSTRAINT UNIQUE INDEX(player,course,server));");
            MySqlRunNonQuery("CREATE TABLE IF NOT EXISTS kzs_recordscp (id INT NOT NULL AUTO_INCREMENT,player INT NOT NULL, map INT NOT NULL, course INT NOT NULL, time FLOAT NOT NULL, jumps INT NOT NULL, cp INT NOT NULL, tele INT NOT NULL, flags INT NOT NULL DEFAULT 0, server INT NOT NULL, date DATE NOT NULL,PRIMARY KEY(id),FOREIGN KEY (player) REFERENCES kzs_players(id),FOREIGN KEY (map) REFERENCES kzs_maps(id),FOREIGN KEY (course) REFERENCES kzs_courses(id),FOREIGN KEY (server) REFERENCES kzs_servers(id),CONSTRAINT UNIQUE INDEX(player,course,server));");
            MySqlRunNonQuery("CREATE TABLE IF NOT EXISTS kzs_ratings (player INT NOT NULL, map INT NOT NULL, rating INT NOT NULL, diff INT NOT NULL, date DATE NOT NULL,FOREIGN KEY (player) REFERENCES kzs_players(id),FOREIGN KEY (map) REFERENCES kzs_maps(id));");
            MySqlRunNonQuery("CREATE TABLE IF NOT EXISTS kzs_wrs (id INT NOT NULL AUTO_INCREMENT,player INT NOT NULL,map INT NOT NULL,course INT NOT NULL,time FLOAT NOT NULL,date DATE NOT NULL,PRIMARY KEY (id),FOREIGN KEY (player) REFERENCES kzs_players(id),FOREIGN KEY (map) REFERENCES kzs_maps(id),FOREIGN KEY (course) REFERENCES kzs_courses(id));");
            MySqlRunNonQuery("CREATE TABLE IF NOT EXISTS kzs_ips (id int NOT NULL AUTO_INCREMENT,player int NOT NULL,ip VARCHAR(16) NOT NULL,country VARCHAR(3) NOT NULL,count int NOT NULL,PRIMARY KEY(id),FOREIGN KEY (player) REFERENCES kzs_players(id),CONSTRAINT UNIQUE(player,ip));");
            MySqlRunNonQuery("CREATE TABLE IF NOT EXISTS kzs_names (id int NOT NULL AUTO_INCREMENT,player int NOT NULL,name VARCHAR(65) NOT NULL,count int NOT NULL,PRIMARY KEY(id),FOREIGN KEY (player) REFERENCES kzs_players(id),CONSTRAINT UNIQUE(player,name));");
            MySqlRunNonQuery("CREATE TABLE IF NOT EXISTS kzs_runcounts(id INT NOT NULL AUTO_INCREMENT,player INT NOT NULL,map INT NOT NULL,course INT NOT NULL, nocheck INT NOT NULL DEFAULT 1,cp INT NOT NULL DEFAULT 1,PRIMARY KEY(id),FOREIGN KEY (player) REFERENCES kzs_players(id),FOREIGN KEY (map) REFERENCES kzs_maps(id),FOREIGN KEY (course) REFERENCES kzs_courses(id),CONSTRAINT UNIQUE INDEX(player,map,course));");
            MySqlRunNonQuery("CREATE TABLE IF NOT EXISTS kzs_medals(id int NOT NULL AUTO_INCREMENT,player int NOT NULL,bronze INT NOT NULL DEFAULT 0, silver INT NOT NULL DEFAULT 0, gold INT NOT NULL DEFAULT 0, platinum INT NOT NULL DEFAULT 0, timebomb INT NOT NULL DEFAULT 0,painkillers INT NOT NULL DEFAULT 0,soccerball INT NOT NULL DEFAULT 0,stats_readout INT NOT NULL DEFAULT 0,moonboots INT NOT NULL DEFAULT 0,strange_mojo INT NOT NULL DEFAULT 0,slowmo INT NOT NULL DEFAULT 0,burningfeet INT NOT NULL DEFAULT 0,cloak INT NOT NULL DEFAULT 0,fastmo INT NOT NULL DEFAULT 0, custom_title INT NOT NULL DEFAULT 0,psychic_anti_gravity INT NOT NULL DEFAULT 0,boots_of_height INT NOT NULL DEFAULT 0,PRIMARY KEY(id),FOREIGN KEY(player) REFERENCES kzs_players(id),CONSTRAINT UNIQUE INDEX(player));");
            MySqlRunNonQuery("CREATE TABLE IF NOT EXISTS kzs_tag (id INT NOT NULL AUTO_INCREMENT, player int NOT NULL, tags int NOT NULL DEFAULT 0, ninja int NOT NULL DEFAULT 0, tagged int NOT NULL DEFAULT 0, round_wins int NOT NULL DEFAULT 0, rounds_played int NOT NULL DEFAULT 0, match_wins int NOT NULL DEFAULT 0, total_untagged_time int NOT NULL DEFAULT 0, round_most_untagged int NOT NULL DEFAULT 0, round_least_untagged int NOT NULL DEFAULT 99999, round_most_powerup int NOT NULL DEFAULT 0, round_least_powerup int NOT NULL DEFAULT 99999, avg_powerup_per_round float NOT NULL DEFAULT 0.0, avg_tags_per_round float NOT NULL DEFAULT 0.0, avg_tagged_per_round float NOT NULL DEFAULT 0.0, PRIMARY KEY(id), FOREIGN KEY (player) REFERENCES kzs_players(id), CONSTRAINT UNIQUE INDEX(player));");
        }

        //TODO: This function should probably be deprecated in favor of prepared statements
        private void MySqlRunNonQuery(string apanQuery)
        {
            try
            {
                tcMySqlCommand lcMySqlCommand = new tcMySqlCommand(apanQuery);
                lcMySqlCommand.mcMySqlCommand.ExecuteNonQuery();
                lcMySqlCommand.Close();
            }
            catch (Exception e)
            {
                Debug.WriteLine("MySqlRunNonQuery error: " + e.Message + ", Query: " + apanQuery);
            }
        }
        
        private string GetRequestString(string apanString)
        {
            string lpanReturn = Request.QueryString[apanString];

            if (lpanReturn == null)
            {
                lpanReturn = "";
            }

            return StaticMethods.MakeSqlStringSafe(lpanReturn);
        }

        private double GetRequestDouble(string apanString)
        {
            string lpanReturn = Request.QueryString[apanString];
            double lrReturn = -1.0;

            if (lpanReturn != null)
            {
                try
                {
                    lrReturn = double.Parse(lpanReturn);
                }
                catch
                {
                    lrReturn = -1.0;
                }
            }

            return lrReturn;
        }

        private int GetRequestInt(string apanString)
        {
            string lpanReturn = Request.QueryString[apanString];
            int lnReturn = -1;

            if (lpanReturn != null)
            {
                try
                {
                    lnReturn = int.Parse(lpanReturn);
                }
                catch
                {
                    lnReturn = -1;
                }
            }

            return lnReturn;
        }

        private int GetMapIdFromName(string apanMapName)
        {
            int lnMapId = -1;

            tcMySqlCommand lcMySqlCommand = new tcMySqlCommand(@"SELECT id FROM kzs_maps WHERE name = ?map LIMIT 1;");
            lcMySqlCommand.mcMySqlCommand.Parameters.Add("?map", apanMapName);

            MySqlDataReader lcMySqlReader = lcMySqlCommand.mcMySqlCommand.ExecuteReader();
            lcMySqlReader.Read();

            if (lcMySqlReader.HasRows)
            {
                lnMapId = lcMySqlReader.GetInt32(0);
            }

            lcMySqlReader.Close();

            return lnMapId;
        }

        private int GetPlayerIdFromAuth(string apanSteamId)
        {
            int lnPlayerId = -1;

            tcMySqlCommand lcMySqlCommand = new tcMySqlCommand(@"SELECT id FROM kzs_players WHERE auth = ?auth LIMIT 1;");
            lcMySqlCommand.mcMySqlCommand.Parameters.Add("?auth", apanSteamId);

            MySqlDataReader lcMySqlReader = lcMySqlCommand.mcMySqlCommand.ExecuteReader();
            lcMySqlReader.Read();

            if (lcMySqlReader.HasRows)
            {
                lnPlayerId = lcMySqlReader.GetInt32(0);
            }

            lcMySqlReader.Close();

            return lnPlayerId;
        }

        private int GetServerIdFromKey(string apanKey)
        {
            int lnServerId = -1;

            tcMySqlCommand lcMySqlCommand = new tcMySqlCommand(@"SELECT id FROM kzs_servers WHERE auth_key = ?key LIMIT 1;");
            lcMySqlCommand.mcMySqlCommand.Parameters.Add("?key", apanKey);

            MySqlDataReader lcMySqlReader = lcMySqlCommand.mcMySqlCommand.ExecuteReader();
            lcMySqlReader.Read();

            if (lcMySqlReader.HasRows)
            {
                lnServerId = lcMySqlReader.GetInt32(0);
            }

            lcMySqlReader.Close();

            return lnServerId;
        }
    }
}
