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

namespace kzstats
{
    public partial class course : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
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

        private int GetCourseId()
        {
            int lnCourseId;
            string lpanCourseId = Request.QueryString["id"];
            lpanCourseId = StaticMethods.CheckSqlReqValidity(lpanCourseId, "1");

            try
            {
                lnCourseId = Convert.ToInt32(lpanCourseId);

                if (lnCourseId < 1 || lnCourseId > 2048)
                {
                    lnCourseId = 1;
                }
            }
            catch
            {
                lnCourseId = 1;
            }

            return lnCourseId;
        }

        private void DoPageLoad()
        {
            Stopwatch lcStopWatch = new Stopwatch(); lcStopWatch.Reset(); lcStopWatch.Start();
            tcGridViewType lcGridViewType = new tcGridViewType(teGridViewType.eeCoursePage, teSubPageType.eeSubPageNone);
            
            string lpanCheck = Request.QueryString["cp"];

            int lnCourseId = GetCourseId();
            
            //Set map and course labels
            tcMySqlCommand lcMySqlCommand = new tcMySqlCommand(@"SELECT M.name,C.name FROM kzs_courses AS C, kzs_maps AS M WHERE C.id = " + lnCourseId + @" AND M.id = (SELECT map FROM kzs_courses WHERE id =  " + lnCourseId + @" LIMIT 1)");
            MySqlDataReader lcMySqlReader = lcMySqlCommand.mcMySqlCommand.ExecuteReader();
            lcMySqlReader.Read();

            if (lcMySqlReader.HasRows)
            {
                gcMapLink.Text = lcMySqlReader.GetString(0);
                gcMapLink.NavigateUrl = "~/map.aspx?id=" + gcMapLink.Text + "&cp=" + lpanCheck;
                gcMapLink.Style.Value = "text-decoration:none"; //Remove underline

                gcCourseName.Text = lcMySqlReader.GetString(1);
            }

            lcMySqlCommand.Close();

            string lpanQuery = GetQueryFromDropDownState(lnCourseId);
            lcGridViewType.meSub = GetGridTypeFromDropDownState();

            //Set gridview data
            tcMySqlDataSource lcMySqlDataSource = new tcMySqlDataSource(lpanQuery);

            DataTable lcDataTable = lcMySqlDataSource.GetDataTable();

            //If the course has records
            if (lcDataTable != null)
            {
                tcGridView lcGridView = GridViewFactory.Create(lcDataTable, lcGridViewType);

                Panel4.Controls.AddAt(0, lcGridView);
            }

            tcLinkPanel.AddLinkPanel(this);
            Page.DataBind();
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

                string lpanCourseId = Request.QueryString["id"];

                for (int i = 0; i < lacText.Length; i++)
                {
                    HyperLink lcLink = new HyperLink();
                    lcLink.Text = lacText[i];
                    lcLink.NavigateUrl = "~/course.aspx?id=" + lpanCourseId + "&sel=" + lacKeyword[i];
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

                            ORDER BY CASE 1 WHEN C.invert THEN Time ELSE -Time END DESC) AS F, (SELECT @rank := 0) AS RNK";

                case "wr":
                    return @"SELECT @rank := @rank + 1 AS Rank, F.* FROM

                            (SELECT P.name AS Player, P.country AS Country, P.auth AS SteamId, WR.time AS Time, DATE_FORMAT(WR.date,'%Y-%m-%d') AS Date FROM

                            (SELECT player,time,date FROM kzs_wrs WHERE course = " + anCourseId + @") AS WR

                            LEFT JOIN (SELECT id,name,country,auth FROM kzs_players) AS P
                            ON P.id = WR.player

                            LEFT JOIN (SELECT invert FROM kzs_courses WHERE id = " + anCourseId + @") AS C
                            ON C.invert

                            ORDER BY date DESC, CASE 1 WHEN C.invert THEN Time ELSE -Time END DESC) AS F, (SELECT @rank := 0) AS RNK";

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

                            ORDER BY CASE 1 WHEN C.invert THEN Time ELSE -Time END DESC) AS F, (SELECT @rank := 0) AS RNK";
            }
        }
    }
}
