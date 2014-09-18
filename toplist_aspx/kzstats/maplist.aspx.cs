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
using System.Net;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using classes;

namespace kzstats
{
    public partial class maplist : System.Web.UI.Page
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
            tcGridViewType lcGridViewType = new tcGridViewType(teGridViewType.eeMapListPage, teSubPageType.eeSubPageNone);

            string lpanQuery = GetQueryFromDropDownState();
            lcGridViewType.meSub = GetGridTypeFromDropDownState();

            tcMySqlDataSource lcMySqlDataSource = new tcMySqlDataSource(lpanQuery);

            DataTable lcDataTable = lcMySqlDataSource.GetDataTable();

            if (lcDataTable != null && lcDataTable.Rows.Count > 0)
            {
                tcGridView lcGridView = GridViewFactory.Create(lcDataTable, lcGridViewType);
                Panel4.Controls.Add(lcGridView);
            }

            tcLinkPanel.AddLinkPanel(this);

            Page.DataBind();
            lcStopWatch.Stop(); gcPageLoad.Text = lcStopWatch.ElapsedMilliseconds.ToString();
        }

        private void SetUpDropDown()
        {
            string[] lacText = { "Maps only", "Expand courses", "Tag maps" };
            string[] lacKeyword = { "maps", "courses", "tag" };

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
                    lcLink.NavigateUrl = "~/maplist.aspx?sel=" + lacKeyword[i];
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
                case "tag":
                    return teSubPageType.eeMapsListTag;
                case "courses":
                    return teSubPageType.eeMapsListCourses;
                default:
                    return teSubPageType.eeMapsListMaps;
            }
        }

        private string GetQueryFromDropDownState()
        {
            switch (gcDropDown.SelectedValue)
            {
                case "tag":
                    return @"SELECT M.name AS Map, M.runs AS ServerRuns, DATE_FORMAT(M.date,'%Y-%m-%d') AS Added FROM kzs_maps AS M
                             WHERE M.name LIKE 'kztag%' ORDER BY Map";

                case "courses":
                    return @"SELECT M.name AS Map, C.name AS Course, C.id AS CourseId, M.runs AS ServerRuns, IFNULL(C.Fin,0) AS CourseCompletions, IFNULL(R.count,0) AS NocheckPlayers, IFNULL(Rcp.count,0) AS CpPlayers, IFNULL(WR.count,0) AS WR, DATE_FORMAT(M.date,'%Y-%m-%d') AS Added FROM kzs_maps AS M
                            LEFT JOIN (SELECT (fin+fincp) AS fin,name,map,id FROM kzs_courses GROUP BY id) AS C
                            ON C.map = M.id

                            LEFT JOIN (SELECT COUNT(DISTINCT(player)) AS count, course FROM kzs_records GROUP BY course) AS R
                            ON R.course = C.id

                            LEFT JOIN (SELECT COUNT(DISTINCT(player)) AS count, course FROM kzs_recordscp GROUP BY course) AS Rcp
                            ON Rcp.course = C.id
                            
                            LEFT JOIN(SELECT COUNT(*) AS count,course FROM kzs_wrs GROUP BY course) AS WR
                            ON WR.course = C.id
                            
                            WHERE M.name NOT LIKE 'kztag%' ORDER BY Map,Course";

                default:
                    return @"SELECT M.name AS Map, IFNULL(C.courses,0) AS Courses, M.runs AS ServerRuns, IFNULL(C.Fin,0) AS CourseCompletions, IFNULL(R.count,0) AS NocheckPlayers, IFNULL(Rcp.count,0) AS CpPlayers, IFNULL(WR.count,0) AS WR, DATE_FORMAT(M.date,'%Y-%m-%d') AS Added FROM kzs_maps AS M
                            LEFT JOIN (SELECT COUNT(*) AS courses, (SUM(fin) + SUM(fincp)) AS Fin, map FROM kzs_courses GROUP BY map) AS C
                            ON C.map = M.id

                            LEFT JOIN (SELECT COUNT(DISTINCT(player)) AS count, map FROM kzs_records GROUP BY map) AS R
                            ON R.map = M.id

                            LEFT JOIN (SELECT COUNT(DISTINCT(player)) AS count, map FROM kzs_recordscp GROUP BY map) AS Rcp
                            ON Rcp.map = M.id
                            
                            LEFT JOIN (SELECT COUNT(*) AS count ,map FROM kzs_wrs GROUP BY map) AS WR
                            ON WR.map = M.id

                            WHERE M.name NOT LIKE 'kztag%' ORDER BY Map";
            }
        }
    }
}
