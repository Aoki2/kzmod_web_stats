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
using System.Configuration;
using System.Drawing;

namespace kzstats
{
    public partial class map : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string lpanMapName = Request.QueryString["id"];
            lpanMapName = StaticMethods.CheckSqlReqValidity(lpanMapName, "kz_bhop_ocean");

            Panel4.EnableViewState = false;

            if (Page.IsPostBack == false)
            {
                SetUpDropDown();
            }

            if (HttpContext.Current.Request.UserAgent.Contains("Chrome") == true || HttpContext.Current.Request.UserAgent.Contains("Firefox") == true)
            {
                gcBody.Attributes.Add("style", tcBgStyler.Instance.GetBgImageStyle());
            }

            DoPageLoad();
        }

        private void DoPageLoad()
        {
            Stopwatch lcStopWatch = new Stopwatch(); lcStopWatch.Reset(); lcStopWatch.Start();
            tcGridViewType lcGridViewType = new tcGridViewType(teGridViewType.eeMapPage,teSubPageType.eeSubPageNone);
            HyperLink lcCourseName;

            string lpanMapName = Request.QueryString["id"];
            lpanMapName = StaticMethods.CheckSqlReqValidity(lpanMapName, "kz_bhop_ocean");
            gcLabelMapName.Text = lpanMapName;

            tcMySqlCommand lcMySqlCommand = new tcMySqlCommand(@"SELECT id,name FROM kzs_courses WHERE map = (SELECT id FROM kzs_maps WHERE name = ?map LIMIT 1) ORDER BY id ASC;");
            lcMySqlCommand.mcMySqlCommand.Parameters.Add("?map", lpanMapName);

            MySqlDataReader lcMySqlReader = lcMySqlCommand.mcMySqlCommand.ExecuteReader();
            
            int lnIndex = 0;
            int lnCourseId = 0;

            //Loop over courses to add labels and records
            while (lcMySqlReader.HasRows && !lcMySqlReader.IsClosed && lcMySqlReader.Read())
            {
                Debug.WriteLine(lcMySqlReader.GetString(0) + " " + lcMySqlReader.GetString(1));
                lnCourseId = lcMySqlReader.GetInt32(0);

                if (lnCourseId < 1)
                {
                    lnCourseId = 1;
                }

                //Set properties for the hyperlink to the course page
                lcCourseName = GetCourseHyperLink(lcMySqlReader.GetString(1), "~/course.aspx?id=" + lnCourseId);

                //Indent course hyperlink and add to the panel
                Panel4.Controls.AddAt(lnIndex++, new LiteralControl("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;"));
                Panel4.Controls.AddAt(lnIndex++, lcCourseName);
                
                Panel4.Controls.AddAt(lnIndex++, new LiteralControl("&nbsp;"));
                Label lcWr = GetWrLabel(lnCourseId);
                Panel4.Controls.AddAt(lnIndex++, lcWr);

                string lpanCp = Request.QueryString["cp"];

                //Handle requests for checkpoints that are not postbacks from dropdown state change
                if (Page.IsPostBack == false && lpanCp != null && lpanCp.Equals("1"))
                {
                    gcDropDown.SelectedValue = "cp";
                }

                string lpanQuery = GetQueryFromDropDownState(lnCourseId);
                lcGridViewType.meSub = GetGridTypeFromDropDownState();

                tcMySqlDataSource lcMySqlDataSource = new tcMySqlDataSource(lpanQuery);

                DataTable lcDataTable = lcMySqlDataSource.GetDataTable();

                //If the course has records
                if (lcDataTable != null && lcDataTable.Rows.Count > 0)
                {
                    tcGridView lcGridView = GridViewFactory.Create(lcDataTable, lcGridViewType);
                    
                    Panel4.Controls.AddAt(lnIndex++, new LiteralControl("<br>"));
                    Panel4.Controls.AddAt(lnIndex++, lcGridView);
                    Panel4.Controls.AddAt(lnIndex++, new LiteralControl("<br>"));
                }
                else //Else the course has no records
                {
                    Label lcNoRecords = new Label();
                    lcNoRecords.Text = " - No records!";

                    Panel4.Controls.AddAt(lnIndex++, lcNoRecords);
                    Panel4.Controls.AddAt(lnIndex++, new LiteralControl("<br><br>"));
                }
            }

            lcMySqlCommand.Close();

            tcLinkPanel.AddLinkPanel(this);

            //Refresh all the gridviews with data contents
            Page.DataBind();
            Page.MaintainScrollPositionOnPostBack = true;

            lcStopWatch.Stop(); gcPageLoad.Text = lcStopWatch.ElapsedMilliseconds.ToString();
        }

        private void SetUpDropDown()
        {
            string[] lacText = { "Nocheck records", "Checkpoint records", "World records" };
            string[] lacKeyword = { "nocheck", "cp", "wr" };

            gcDropDown.AutoPostBack = true;
            gcDropDown.SelectedIndexChanged += new EventHandler(cb_DropDownTextChange);

            //Add options to dropdown
            for (int i = 0; i < lacText.Length; i++)
            {
                gcDropDown.Items.Add(new ListItem(lacText[i], lacKeyword[i]));
            }

            string lpanSel = Request.QueryString["sel"];

            if (HttpContext.Current.Request.UserAgent.Contains("Chrome") == false && HttpContext.Current.Request.UserAgent.Contains("Firefox") == false)
            {
                gcDropDown.Visible = false;

                //TODO
                //Panel3.Width = Unit.Percentage(100);
                Panel3.Height = 425;
                Panel3.ScrollBars = ScrollBars.Vertical;

                if (lpanSel == null)
                {
                    lpanSel = lacKeyword[0];
                }
            }

            if (lpanSel != null)
            {
                gcDropDown.SelectedValue = lpanSel;

                Panel3.Controls.Remove(gcDropDown);

                string lpanMapName = Request.QueryString["id"];

                for (int i = 0; i < lacText.Length; i++)
                {
                    HyperLink lcLink = new HyperLink();
                    lcLink.Text = lacText[i];
                    lcLink.NavigateUrl = "~/map.aspx?id=" + lpanMapName + "&sel=" + lacKeyword[i];
                    lcLink.Font.Name = "Arial";
                    lcLink.Font.Size = 11;
                    lcLink.Font.Bold = true;
                    lcLink.ForeColor = teColors.eeLink;
                    gcLinks.Controls.Add(lcLink);

                    if (i + 1 < lacText.Length)
                    {
                        gcLinks.Controls.Add(new LiteralControl("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;"));
                    }
                    else
                    {
                        gcLinks.Controls.Add(new LiteralControl("<br><br>"));
                    }
                }
            }
        }

        private void cb_DropDownTextChange(object acSender, EventArgs e)
        {
            while (Panel4.Controls.Count > 0)
            {
                Panel4.Controls.RemoveAt(0);
            }

            DoPageLoad();
        }

        private HyperLink GetCourseHyperLink(string apanText, string apanUrl)
        {
            HyperLink lcReturn = new HyperLink();

            lcReturn.Text = apanText;
            lcReturn.NavigateUrl = apanUrl;
            lcReturn.ForeColor = Color.FromName("#A40000");
            lcReturn.Style.Value = "text-decoration:none"; //Remove underline
            lcReturn.Font.Bold = true;
            lcReturn.Font.Size = 12;
            lcReturn.Font.Name = "Arial";

            return lcReturn;
        }

        private Label GetWrLabel(int anCourseId)
        {
            Label lcReturn = new Label();

            string lpanPlayer = "";
            string lpanTime = "";
            string lpanDate = "";

            tcMySqlCommand lcMySqlCommand = new tcMySqlCommand(@"SELECT WR.time,P.name,DATE_FORMAT(WR.date,'%Y-%m-%d') AS Date FROM

                                                                (SELECT player,course,time,date FROM kzs_wrs WHERE course = ?courseid) AS WR

                                                                LEFT JOIN (SELECT name,id FROM kzs_players) AS P
                                                                ON P.id = WR.player

                                                                LEFT JOIN (SELECT id,invert FROM kzs_courses) AS C
                                                                ON C.id = WR.course

                                                                ORDER BY Date DESC, CASE 1 WHEN C.invert THEN Time ELSE -Time END  DESC LIMIT 1");

            lcMySqlCommand.mcMySqlCommand.Parameters.Add("?courseid", anCourseId.ToString());

            MySqlDataReader lcMySqlReader = lcMySqlCommand.mcMySqlCommand.ExecuteReader();
            lcMySqlReader.Read();

            if(lcMySqlReader.HasRows)
            {
                lpanTime = StaticMethods.TimeToString(lcMySqlReader.GetFloat(0));
                lpanPlayer = lcMySqlReader.GetString(1);
                lpanDate = lcMySqlReader.GetString(2);
            }

            lcMySqlCommand.Close();

            lcReturn.Font.Name = "Arial";
            lcReturn.Font.Bold = true;
            lcReturn.ForeColor = teColors.eeText;
            lcReturn.Font.Size = 11;

            if (lpanDate != "" && lpanPlayer != "" && lpanTime != "")
            {
                if (lpanPlayer.Length > 18)
                {
                    lcReturn.ToolTip = lpanPlayer;
                    lpanPlayer = lpanPlayer.Substring(0, 17);
                }

                lcReturn.Text = "(World record " + lpanTime + " by " + lpanPlayer + " on " + lpanDate + ")";
            }

            return lcReturn;
        }

        private teSubPageType GetGridTypeFromDropDownState()
        {
            switch (gcDropDown.SelectedValue)
            {
                case "cp":
                    return teSubPageType.eeMapPageCheck;
                case "wr":
                    return teSubPageType.eeMapPageWr;
                default:
                    return teSubPageType.eeMapPageNocheck;
            }
        }

        private string GetQueryFromDropDownState(int anCourseId)
        {
            switch (gcDropDown.SelectedValue)
            {
                case "cp":
                    return @"SELECT @rank := @rank + 1 AS Rank, F.* FROM

                            (SELECT P.name AS Player, P.auth AS SteamID,
                            CASE 1 WHEN C.invert THEN Rtimes.maxtime ELSE Rtimes.mintime END AS Time,
                            CASE 1 WHEN C.invert THEN Rmax.cp ELSE Rmin.cp END AS Checks, 
                            CASE 1 WHEN C.invert THEN Rmax.tele ELSE Rmin.tele END AS Teles, 
                            CASE 1 WHEN C.invert THEN Smax.name ELSE Smin.name END AS Server, 
                            P.country AS Country FROM

                            (SELECT course, min(time) AS mintime, max(time) AS maxtime, player FROM kzs_recordscp WHERE course = " + anCourseId + @" GROUP BY player) AS Rtimes

                            LEFT JOIN (SELECT name,auth,id,country FROM kzs_players) AS P
                            ON P.id = Rtimes.player

                            LEFT JOIN (SELECT cp,tele,time AS ptime,server,player,course FROM kzs_recordscp) AS Rmin
                            ON Rmin.ptime = Rtimes.mintime AND Rmin.player = P.id AND  Rmin.course = Rtimes.course

                            LEFT JOIN (SELECT cp,tele,time AS ptime,server,player,course FROM kzs_recordscp) AS Rmax
                            ON Rmax.ptime = Rtimes.maxtime AND Rmax.player = P.id AND  Rmax.course = Rtimes.course

                            LEFT JOIN (SELECT id,invert FROM kzs_courses) AS C
                            ON C.id = Rtimes.course

                            LEFT JOIN (SELECT id,name FROM kzs_servers) AS Smin
                            ON Smin.id = Rmin.server

                            LEFT JOIN (SELECT id,name FROM kzs_servers) AS Smax
                            ON Smax.id = Rmin.server

                            ORDER BY CASE 1 WHEN C.invert THEN Time ELSE -Time END DESC) AS F, (SELECT @rank := 0) AS RNK LIMIT 10";

                case "wr":
                    return @"SELECT @rank := @rank + 1 AS Rank, F.* FROM

                            (SELECT P.name AS Player, P.country AS Country, P.auth AS SteamId, WR.time AS Time, DATE_FORMAT(WR.date,'%Y-%m-%d') AS Date FROM

                            (SELECT player,time,date FROM kzs_wrs WHERE course = " + anCourseId + @") AS WR

                            LEFT JOIN (SELECT id,name,country,auth FROM kzs_players) AS P
                            ON P.id = WR.player

                            LEFT JOIN (SELECT invert FROM kzs_courses WHERE id = " + anCourseId + @") AS C
                            ON C.invert

                            ORDER BY date DESC, CASE 1 WHEN C.invert THEN Time ELSE -Time END DESC) AS F, (SELECT @rank := 0) AS RNK LIMIT 10";

                default:
                    return @"SELECT @rank := @rank + 1 AS Rank, F.* FROM

                            (SELECT P.name AS Player, P.auth AS SteamID,
                            CASE 1 WHEN C.invert THEN Rtimes.maxtime ELSE Rtimes.mintime END AS Time,
                            CASE 1 WHEN C.invert THEN Smax.name ELSE Smin.name END AS Server, 
                            P.country AS Country FROM

                            (SELECT course, min(time) AS mintime, max(time) AS maxtime, player FROM kzs_records WHERE course = " + anCourseId + @" GROUP BY player) AS Rtimes

                            LEFT JOIN (SELECT name,auth,id,country FROM kzs_players) AS P
                            ON P.id = Rtimes.player

                            LEFT JOIN (SELECT time AS ptime,server,player,course FROM kzs_records) AS Rmin
                            ON Rmin.ptime = Rtimes.mintime AND Rmin.player = P.id AND  Rmin.course = Rtimes.course

                            LEFT JOIN (SELECT time AS ptime,server,player,course FROM kzs_records) AS Rmax
                            ON Rmax.ptime = Rtimes.maxtime AND Rmax.player = P.id AND  Rmax.course = Rtimes.course

                            LEFT JOIN (SELECT id,invert FROM kzs_courses) AS C
                            ON C.id = Rtimes.course

                            LEFT JOIN (SELECT id,name FROM kzs_servers) AS Smin
                            ON Smin.id = Rmin.server

                            LEFT JOIN (SELECT id,name FROM kzs_servers) AS Smax
                            ON Smax.id = Rmin.server

                            ORDER BY CASE 1 WHEN C.invert THEN Time ELSE -Time END DESC) AS F, (SELECT @rank := 0) AS RNK LIMIT 10";
            }
        }
    }
}
