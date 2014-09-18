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
    public partial class playerlist : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
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
            tcGridViewType lcGridViewType = new tcGridViewType(teGridViewType.eePlayerListPage, teSubPageType.eeSubPageNone);
            
            lcGridViewType.meSub = GetGridTypeFromDropDownState();
            string lpanQuery = GetQueryFromDropDownState();

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
            string[] lacText = { "By player", "Tag ranks", "Tag stats", "By country" };
            string[] lacKeyword = { "player", "tag_rank", "tag_stats", "country" };

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
                    lcLink.NavigateUrl = "~/playerranks.aspx?sel=" + lacKeyword[i];
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

        private teSubPageType GetGridTypeFromDropDownState()
        {
            switch (gcDropDown.SelectedValue)
            {
                case "country":
                    return teSubPageType.eePlayerListCountry;
                case "tag_rank":
                    return teSubPageType.eePlayerListTagRank;
                case "tag_stats":
                    return teSubPageType.eePlayerListTagStats;
                default:
                    return teSubPageType.eePlayerListPlayer;
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

        private string GetQueryFromDropDownState()
        {
            switch (gcDropDown.SelectedValue)
            {
                case "tag_rank":
                    //Fall through case
                case "tag_stats":
                    return @"SELECT @rank := @rank + 1 AS Rank, F.* FROM

                            (SELECT P.name AS Player, P.auth AS SteamId, P.country, T.* FROM
                            (SELECT player AS Tplayer, tags, ninja, tagged, round_wins, rounds_played, match_wins, total_untagged_time, round_most_untagged,  round_most_powerup,  avg_powerup_per_round, avg_tags_per_round, avg_tagged_per_round FROM kzs_tag) AS T

                            LEFT JOIN(SELECT id,auth,country,name FROM kzs_players) AS P
                            ON T.Tplayer = P.id) AS F, (SELECT @rank := 0) AS RNK ORDER BY round_wins DESC,tags DESC";
                case "country":
                    return @"SELECT @rank := @rank + 1 AS Rank, F.* FROM

                            (SELECT Pc.country AS Country,IFNULL(players,0) AS Players,IFNULL(nocheck,0) AS Records,IFNULL(cp,0) AS RecordsCp, CNT.full AS CountryFull FROM
                            (SELECT country,COUNT(*) AS Players FROM kzs_players GROUP BY country) AS Pc

                            LEFT JOIN (SELECT * FROM kzs_countries) AS CNT
                            ON CNT.code = Pc.country

                            LEFT JOIN(SELECT P.country, SUM(R.nocheck) AS nocheck FROM
                                                        (SELECT player, COUNT(*) AS nocheck FROM kzs_records GROUP BY player) AS R
                                                        LEFT JOIN (SELECT id,country FROM kzs_players) AS P
                                                            ON R.player = P.id
                                                    GROUP BY country) AS NCHK
                            ON NCHK.country = Pc.country

                            LEFT JOIN(SELECT P.country, SUM(R.cp) AS cp FROM
                                                        (SELECT player, COUNT(*) AS cp FROM kzs_recordscp GROUP BY player) AS R
                                                        LEFT JOIN (SELECT id,country FROM kzs_players) AS P
                                                            ON R.player = P.id
                                                    GROUP BY country) AS CP
                            ON CP.country = Pc.country ORDER BY Records DESC) AS F, (SELECT @rank := 0) AS RNK";
                default:
                    return @"SELECT @rank := @rank + 1 AS rank, F.Player, IFNULL(F.Records,0) AS Records,IFNULL(F.RecordsCp,0) AS RecordsCp, F.auth AS SteamID, F.country AS Country FROM

                            (SELECT P.name AS Player, R.rec AS Records, Rcp.rec AS RecordsCp, P.auth, P.country FROM 

                            (SELECT id,name,auth,country FROM kzs_players) AS P

                            LEFT JOIN (SELECT player, COUNT(DISTINCT(course)) AS rec FROM kzs_records GROUP BY player) AS R
                            ON R.player = P.id

                            LEFT JOIN (SELECT player, COUNT(DISTINCT(course)) AS rec FROM kzs_recordscp GROUP BY player) AS Rcp
                            ON Rcp.player = P.id) AS F, (SELECT @rank := 0) AS RNK ORDER BY Records DESC,RecordsCp DESC";
            }
        }
    }
}
