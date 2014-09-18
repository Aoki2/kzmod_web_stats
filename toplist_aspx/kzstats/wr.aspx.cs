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
    public partial class wr : System.Web.UI.Page
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

        private void DoPageLoad()
        {
            Stopwatch lcStopWatch = new Stopwatch(); lcStopWatch.Reset(); lcStopWatch.Start();
            tcGridViewType lcGridViewType = new tcGridViewType(teGridViewType.eeWrPage, teSubPageType.eeSubPageNone);

            string lpanQuery = GetQueryFromDropDownState();
            lcGridViewType.meSub = GetGridTypeFromDropDownState();

            tcMySqlDataSource lcMySqlDataSource = new tcMySqlDataSource(lpanQuery);

            DataTable lcDataTable = lcMySqlDataSource.GetDataTable();

            if (lcDataTable != null && lcDataTable.Rows.Count > 0)
            {
                tcGridView lcGridView = GridViewFactory.Create(lcDataTable, lcGridViewType);
                Panel4.Controls.AddAt(0, lcGridView);
            }

            tcLinkPanel.AddLinkPanel(this);

            //Refresh all the gridviews with data contents
            Page.DataBind();
            Page.MaintainScrollPositionOnPostBack = true;

            lcStopWatch.Stop(); gcPageLoad.Text = lcStopWatch.ElapsedMilliseconds.ToString();
        }

        private void SetUpDropDown()
        {
            string[] lacText = { "World records by course", "World records by player", "World records by country" };
            string[] lacKeyword = { "all", "player", "country" };

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

                for (int i = 0; i < lacText.Length; i++)
                {
                    HyperLink lcLink = new HyperLink();
                    lcLink.Text = lacText[i];
                    lcLink.NavigateUrl = "~/wr.aspx?sel=" + lacKeyword[i];
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
                case "country":
                    return teSubPageType.eeWrPageCountry;
                case "player":
                    return teSubPageType.eeWrPagePlayer;
                default:
                    return teSubPageType.eeWrPageAll;
            }
        }

        private string GetQueryFromDropDownState()
        {
            switch (gcDropDown.SelectedValue)
            {
                case "player":
                    return @"SELECT @rank := @rank + 1 AS Rank, FF.Player, FF.SteamId, FF.Country, FF.CurrentWR, FF.TotalWR, FF.TotalCourses FROM

                            (SELECT  PlayerId,F.Player,SteamId,Country,COUNT(*) AS TotalWR, invert, IFNULL(CURWR.CurrWR,0) AS CurrentWR, NN.TotalCourses FROM

                                (SELECT P.id AS PlayerId, P.name AS Player, P.auth AS SteamId, P.country AS Country,M.id AS MapId,C.id AS CourseId,WR.time,WR.date,C.invert FROM

                                (SELECT player,map,course,time,date FROM kzs_wrs) AS WR

                                LEFT JOIN (SELECT id,invert,name FROM kzs_courses) AS C
                                ON C.id = WR.course

                                LEFT JOIN (SELECT name,id,auth,country FROM kzs_players) AS P
                                ON P.id = WR.player

                                LEFT JOIN (SELECT name,id FROM kzs_maps) AS M
                                ON M.id = WR.map

                                ORDER BY map,course,CASE 1 WHEN C.invert THEN Time ELSE -Time END DESC) AS F

                            LEFT JOIN (SELECT player AS TWRplayer,COUNT(*) AS CurrWR FROM

                                                        (SELECT * FROM

                                                        (SELECT player,course,invert,date,time FROM

                                                        (SELECT player,course,time,date FROM kzs_wrs ORDER BY course) AS WRR

                                                        LEFT JOIN (SELECT invert,id FROM kzs_courses) AS CC
                                                        ON CC.id = WRR.course

                                                        ORDER BY course,date DESC,  CASE 1 WHEN CC.invert THEN Time ELSE -Time END DESC) AS TT

                                                        GROUP BY course) AS PP

                                                        GROUP BY player) AS CURWR
                            ON F.PlayerId = CURWR.TWRplayer

                            LEFT JOIN (SELECT player, COUNT(*) AS TotalCourses FROM (SELECT player, course FROM (SELECT player,course FROM kzs_wrs ORDER BY player,course) AS N GROUP BY player,course ORDER BY player,course) AS A GROUP BY player) AS NN
                            ON NN.player = F.PlayerId

                            GROUP BY PlayerId ORDER BY CurrentWR DESC) AS FF, (SELECT @rank := 0) AS RNK";

                case "country":
                    return @"SELECT FIN.*, CNTT.full AS CountryFull FROM

                            (SELECT @rank := @rank + 1 AS Rank, FF.Country, FF.CurrentWR, FF.TotalWR, FF.TotalCourses FROM

                            (SELECT  PlayerId,F.Country,COUNT(*) AS TotalWR, invert, IFNULL(CURWR.CurrWR,0) AS CurrentWR, NN.TotalCourses FROM

                            (SELECT P.id AS PlayerId, P.country AS Country,M.id AS MapId,C.id AS CourseId,WR.time,
                            WR.date,C.invert FROM

                                (SELECT player,map,course,time,date FROM kzs_wrs) AS WR

                                LEFT JOIN (SELECT id,invert FROM kzs_courses) AS C
                                ON C.id = WR.course

                                LEFT JOIN (SELECT id,country FROM kzs_players) AS P
                                ON P.id = WR.player

                                LEFT JOIN (SELECT id FROM kzs_maps) AS M
                                ON M.id = WR.map

                                ORDER BY map,course,CASE 1 WHEN C.invert THEN Time ELSE -Time END DESC) AS F
    
                            LEFT JOIN (SELECT country AS TWRcountry,COUNT(*) AS CurrWR FROM

                                                        (SELECT * FROM

                                                        (SELECT country,course,invert,date,time FROM

                                                        (SELECT player,course,time,date FROM kzs_wrs ORDER BY course) AS WRR

                                                        LEFT JOIN(SELECT id,country FROM kzs_players) AS PPP
                                                        ON PPP.id = WRR.player

                                                        LEFT JOIN (SELECT invert,id FROM kzs_courses) AS CC
                                                        ON CC.id = WRR.course

                                                        ORDER BY course,date DESC,  CASE 1 WHEN CC.invert THEN Time ELSE -Time END DESC) AS TT

                                                        GROUP BY course) AS PP

                                                        GROUP BY country) AS CURWR
                            ON F.Country = CURWR.TWRcountry

                            LEFT JOIN (SELECT country, COUNT(*) AS TotalCourses FROM (SELECT country, course FROM (SELECT country,course FROM (SELECT player,course FROM kzs_wrs) AS E LEFT JOIN(SELECT id,country FROM kzs_players)AS W ON W.id = E.player ORDER BY country,course) AS N GROUP BY country,course ORDER BY country,course) AS A GROUP BY country) AS NN
                            ON NN.country = F.country

                            GROUP BY Country ORDER BY CurrentWR DESC) AS FF, (SELECT @rank := 0) AS RNK) AS FIN

                            LEFT JOIN (SELECT full,code FROM kzs_countries) AS CNTT
                            ON CNTT.code = FIN.country";

                default:
                    return @"SELECT F.* FROM

                            (SELECT M.name AS Map, C.name AS Course, C.id AS CourseId, P.name AS Player, P.auth AS SteamId, P.country AS Country, WR.time AS Time, DATE_FORMAT(WR.date,'%Y-%m-%d') AS Date FROM

                            (SELECT player,map,course,time,date FROM kzs_wrs) AS WR

                            LEFT JOIN (SELECT name,id,auth,country FROM kzs_players) AS P
                            ON P.id = WR.player

                            LEFT JOIN (SELECT name,id FROM kzs_maps) AS M
                            ON M.id = WR.map

                            LEFT JOIN (SELECT name,id,invert FROM kzs_courses) AS C
                            ON C.id = WR.course

                             ORDER BY Map,Course,Date DESC, CASE 1 WHEN C.invert THEN Time ELSE -Time END DESC) AS F
 
                             GROUP BY Map,Course ORDER BY map,course";
            }
        }
    }
}
