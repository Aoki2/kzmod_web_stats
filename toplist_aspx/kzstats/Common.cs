using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Drawing;
using System.Web;
using System.Net;
using System.IO;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Cryptography;
using System.Reflection;

namespace classes
{
    enum teMedals { eeBronze = 0, eeSilver = 1, eeGold = 2, eePlatinum = 3, eeMax = 4 };

    enum teRewards { eeTimeBomb = 0, eePainKillers = 1, eeSoccerBall = 2, eeStatsReadout = 3, eeMoonBoots = 4, eeStrangeMojo = 5, eeSlowmo = 6, eeBurningFeet = 7, eeCloak = 8, eeFastmo = 9, eeCustomTitle = 10, 
                     eePsychicAntiGravity = 11, eeBootsOfHeight = 12, eeMax = 13 };

    public class Globals
    {
        static public int mnStringLength = 64;
    }

    public static class teColors
    {
        static public Color eeLink = Color.FromName("#DA6D0A");
        static public Color eeText = Color.FromName("#53606F");
        static public Color eeRedText = Color.FromName("#A40000");
        static public Color eeGreenText = Color.FromName("#009933");
        static public Color eeBg = Color.FromName("#637385");
        static public Color eeHeaderBg = Color.FromName("#F4F4F4");
        static public Color eeRowBg = Color.FromName("#F3F1EA");
        static public Color eeAltRowBg = Color.FromName("#F9F9F9");
        static public Color eePanelBg = Color.FromName("#F3F1EA");
    }

    public class StaticMethods
    {
        static public string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        static public string CheckSqlReqValidity(string apnString, string apanDefault)
        {
            if (apnString == null || apnString.Equals("") || apnString.Equals("'") || apnString.Contains(';'))
            {
                return apanDefault;
            }
            else
            {
                return apnString;
            }
        }

        static public string MakeSqlStringSafe(string apanQuery)
        {
            return apanQuery.Replace(";", "").Replace("'", "''");
        }

        static public string GetConnectionString()
        {
		    //TODO: Update for local machine name
		    //If testing with local MySQL server, use dev connection string if on dev machine
            string lcDevHostname = "dev_mysql_server"; 
			
            string lpanConnectionString = null;
            string lpanConStrKey = "csMySQL_ConNet514";

            if (lcDevHostname.Equals(System.Environment.MachineName))
            {
                lpanConStrKey = "csMySQL_ConNet514_dev";
            }

            lpanConnectionString = ConfigurationManager.ConnectionStrings[lpanConStrKey].ConnectionString;

            return lpanConnectionString;
        }
        
        static public string TimeToString(float arTime, bool aeMillisec = true)
        {
            arTime = arTime + (float)0.005; //milliseconds correction
            string lpanReturn = "";
            int lnTime = (int)arTime;

            int lnHours = lnTime / 3600;
            int lnMinutes = lnTime / 60;
            int lnSeconds = lnTime % 60;
            int lnMilliseconds = (int)((arTime % 1.0) * 100.0);

            if (lnHours > 0)
            {
                if (aeMillisec)
                {
                    lpanReturn = String.Format("{0:00}", lnHours) + ":" + String.Format("{0:00}", lnMinutes) + ":" + String.Format("{0:00}", lnSeconds) + "." + String.Format("{0:00}", lnMilliseconds);
                }
                else
                {
                    lpanReturn = String.Format("{0:00}", lnHours) + ":" + String.Format("{0:00}", lnMinutes) + ":" + String.Format("{0:00}", lnSeconds);
                }
            }
            else
            {
                if (aeMillisec)
                {
                    lpanReturn = String.Format("{0:00}", lnMinutes) + ":" + String.Format("{0:00}", lnSeconds) + "." + String.Format("{0:00}", lnMilliseconds);
                }
                else
                {
                    lpanReturn = String.Format("{0:00}", lnMinutes) + ":" + String.Format("{0:00}", lnSeconds);
                }
            }

            return lpanReturn;
        }

        static public string GetSteamProfileLink(string apanSteamId)
        {
            apanSteamId = apanSteamId.Replace("STEAM_0:", "");
            apanSteamId = apanSteamId.Replace(":", "");

            int lnServer = int.Parse(apanSteamId.Substring(0, 1));
            int lnAuth = int.Parse(apanSteamId.Substring(1));

            Int64 lnFriendId = (lnAuth * 2) + 76561197960265728 + lnServer;

            return "http://steamcommunity.com/profiles/" + lnFriendId.ToString();
        }

        static public string GetPlayerImageUrl(string apanSteamId)
        {
            WebClient lcClient = new WebClient();

            string lpanHtml = lcClient.DownloadString(GetSteamProfileLink(apanSteamId));

            lpanHtml = lpanHtml.Substring(lpanHtml.IndexOf("avatarFull"));

            lpanHtml = lpanHtml.Substring(0, lpanHtml.IndexOf("</div>"));

            lpanHtml = lpanHtml.Replace("http://media", "!http://media");
            lpanHtml = lpanHtml.Replace(".jpg\"", ".jpg!");

            lpanHtml = lpanHtml.Substring(lpanHtml.IndexOf("!") + 1, lpanHtml.LastIndexOf("!") - lpanHtml.IndexOf("!") - 1);

            return lpanHtml;
        }
        
        static public void LogMsg(string apanMsg)
        {
            string lpanPath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(StaticMethods)).CodeBase);
            lpanPath = lpanPath.Replace("\\bin", "");
            string lpnFilename = lpanPath + "\\log\\kzstats.log";
            lpnFilename = new Uri(lpnFilename).LocalPath;
            File.SetAttributes(lpnFilename, FileAttributes.Normal);

            if (File.Exists(lpnFilename))
            {
                FileInfo lcFileInfo = new FileInfo(lpnFilename);

                if (lcFileInfo.Length > 100 * 1000 * 1000)
                {
                    File.Delete(lpanPath + "\\log\\kzstats.log");
                }
            }

            FileStream lcLogFile = new FileStream(lpnFilename, FileMode.Append, FileAccess.Write);
            File.SetAttributes(lpnFilename, FileAttributes.Normal);

            StreamWriter lcWriter = new StreamWriter(lcLogFile);

            lcWriter.WriteLine(DateTime.Now.ToString() + " : " + apanMsg);
            lcWriter.Close();
        }
    }
}
