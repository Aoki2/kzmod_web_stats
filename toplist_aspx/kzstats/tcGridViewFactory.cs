using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Data;
using System.Diagnostics;
using System.Globalization;

namespace classes
{
    public class tcGridViewType
    {
        public teGridViewType meType;
        public teSubPageType meSub;

        public tcGridViewType(teGridViewType aeType, teSubPageType aeSub)
        {
            meType = aeType;
            meSub = aeSub;
        }
    }

    public enum teSubPageType
    {
        eeSubPageNone,
        eePlayerPageNocheck,
        eePlayerPageCp,
        eePlayerPageWr,
        eePlayerPageTag,
        eePlayerPageStats,
        eeMapPageNocheck,
        eeMapPageCheck,
        eeMapPageWr,
        eeWrPageAll,
        eeWrPagePlayer,
        eeWrPageCountry,
        eeMapsListMaps,
        eeMapsListCourses,
        eeMapsListTag,
        eePlayerListPlayer,
        eePlayerListCountry,
        eePlayerListTagRank,
        eePlayerListTagStats,
        eePlayerPageIncomplete
    }

    public enum teGridViewType
    {
        eeNone,
        eeMapPage,
        eeCoursePage,
        eeMapListPage,
        eePlayerPage,
        eePlayerListPage,
        eeWrPage
    }

    public class GridViewFactory
    {
        static private bool meInitialized = false;
        static private TableItemStyle mcStyle;

        static private void DoInitialization()
        {
            mcStyle = new TableItemStyle();
            mcStyle.HorizontalAlign = HorizontalAlign.Center;
            mcStyle.BackColor = teColors.eeRowBg;
            mcStyle.BorderColor = Color.Black;
            mcStyle.BorderStyle = BorderStyle.None;
            mcStyle.BorderWidth = 0;
            mcStyle.ForeColor = teColors.eeText;

            mcStyle.Font.Name = "Arial";
            mcStyle.Font.Size = 10;
            mcStyle.HorizontalAlign = HorizontalAlign.Left;
        }

        static public tcGridView Create(DataTable acDataTable, tcGridViewType aeType)
        {
            if (meInitialized == false)
            {
                DoInitialization();
                meInitialized = true;
            }

            tcGridView lcReturn = new tcGridView(aeType);

            lcReturn.BorderStyle = BorderStyle.None;
            lcReturn.GridLines = GridLines.None;

            lcReturn.AllowSorting = false;
            lcReturn.AllowPaging = false;

            switch(aeType.meType)
            {
                case teGridViewType.eeCoursePage:
                    lcReturn = SetMapPageColumns(lcReturn, true, aeType.meSub);
                    break;
                case teGridViewType.eeMapPage:
                    lcReturn = SetMapPageColumns(lcReturn, false, aeType.meSub);
                    break;
                case teGridViewType.eeMapListPage:
                    lcReturn = SetMapListPageColumns(lcReturn, aeType.meSub);
                    break;
                case teGridViewType.eePlayerPage:
                    lcReturn = SetPlayerPageColumns(lcReturn, aeType.meSub);
                    break;
                case teGridViewType.eePlayerListPage:
                    lcReturn = SetPlayerListPageColumns(lcReturn, aeType.meSub);
                    break;
                case teGridViewType.eeWrPage:
                    lcReturn = SetWrPageColumns(lcReturn, aeType.meSub);
                    break;
                default:
                    Debug.WriteLine("Unexpected switch case");
                    break;
            }

            lcReturn.DataSource = acDataTable;

            lcReturn.ShowHeader = true;

            lcReturn.HorizontalAlign = HorizontalAlign.Center;

            lcReturn.CellPadding = 1;
            lcReturn.CellSpacing = 1;
            lcReturn.Width = 673;

            mcStyle.BackColor = teColors.eeRowBg;
            lcReturn.EditRowStyle.CopyFrom(mcStyle);
            lcReturn.AlternatingRowStyle.CopyFrom(mcStyle);

            mcStyle.BackColor = teColors.eeHeaderBg;
            lcReturn.HeaderStyle.CopyFrom(mcStyle);
            lcReturn.FooterStyle.CopyFrom(mcStyle);
            lcReturn.PagerStyle.CopyFrom(mcStyle);
            lcReturn.PagerStyle.HorizontalAlign = HorizontalAlign.Center;
            lcReturn.PagerStyle.Font.Name = "Arial";
            lcReturn.PagerStyle.ForeColor = teColors.eeText;

            mcStyle.BackColor = teColors.eeAltRowBg;
            lcReturn.RowStyle.CopyFrom(mcStyle);
            
            return lcReturn;
        }

        static private tcGridView SetMapPageColumns(tcGridView acGridView, bool aePaging, teSubPageType aeType)
        {
            int lnCol = 0;

            acGridView.AutoGenerateColumns = false;
            acGridView.PageSize = 30;

            acGridView.AllowSorting = true;
            acGridView.AllowPaging = aePaging;

            acGridView.Columns.Add(CreateBoundField("Rank", "Rank", true,true,false,50));
            acGridView.Columns[lnCol].HeaderText = "Rank";
            acGridView.Columns[lnCol++].SortExpression = "Rank";

            acGridView.Columns.Add(CreatePlayerNameField());
            acGridView.Columns[lnCol].HeaderText = "Player";
            acGridView.Columns[lnCol].HeaderStyle.Width = 256;
            acGridView.Columns[lnCol].ItemStyle.Width = 256;
            acGridView.Columns[lnCol++].SortExpression = "Player";

            acGridView.Columns.Add(CreateBoundField("Player", "Player", false));
            acGridView.Columns[lnCol].HeaderText = "Player";
            acGridView.Columns[lnCol++].SortExpression = "Player";

            acGridView.Columns.Add(CreateBoundField("SteamID", "SteamID", false));
            acGridView.Columns[lnCol].HeaderText = "SteamID";
            acGridView.Columns[lnCol++].SortExpression = "SteamID";

            acGridView.Columns.Add(CreateBoundField("Time", "Time", true));
            acGridView.Columns[lnCol].HeaderText = "Time";
            acGridView.Columns[lnCol++].SortExpression = "Time";

            if (aeType == teSubPageType.eeMapPageCheck)
            {
                acGridView.Columns.Add(CreateBoundField("Checks", "Checks", true,true));
                acGridView.Columns[lnCol].HeaderText = "Checks";
                acGridView.Columns[lnCol].HeaderStyle.Width = 64;
                acGridView.Columns[lnCol++].SortExpression = "Checks";

                acGridView.Columns.Add(CreateBoundField("Teles", "Teles", true,true));
                acGridView.Columns[lnCol].HeaderText = "Teles";
                acGridView.Columns[lnCol].HeaderStyle.Width = 64;
                acGridView.Columns[lnCol++].SortExpression = "Teles";
            }

            if (aeType != teSubPageType.eeMapPageWr)
            {
                acGridView.Columns.Add(CreateBoundField("Server", "Server", true));
                acGridView.Columns[lnCol].HeaderText = "Server";
                acGridView.Columns[lnCol].HeaderStyle.Width = 256;
                acGridView.Columns[lnCol].ItemStyle.Width = 256;
                acGridView.Columns[lnCol++].SortExpression = "Server";
            }
            else
            {
                acGridView.Columns.Add(CreateBoundField("Date", "Date", true));
                acGridView.Columns[lnCol].HeaderText = "Date";
                acGridView.Columns[lnCol++].SortExpression = "Date";
            }
            
            return acGridView;
        }

        static private tcGridView SetMapListPageColumns(tcGridView acGridView, teSubPageType aeType)
        {
            int lnCol = 0;

            acGridView.AutoGenerateColumns = false;

            acGridView.Columns.Add(CreateHyperLinkField(new string[] {"Map"}, "Map", "~/map.aspx?id={0}"));
            acGridView.Columns[lnCol].SortExpression = "Map";
            acGridView.Columns[lnCol++].HeaderText = "Map";

            if (aeType == teSubPageType.eeMapsListMaps)
            {
                acGridView.Columns.Add(CreateBoundField("Courses", "Courses", true, true));
                acGridView.Columns[lnCol].SortExpression = "Courses";
                acGridView.Columns[lnCol++].HeaderText = "Courses";

                acGridView.Columns.Add(CreateBoundField("ServerRuns", "ServerRuns", true, true));
                acGridView.Columns[lnCol].SortExpression = "ServerRuns";
                acGridView.Columns[lnCol++].HeaderText = "Server<br/>Runs";
            }
            else if (aeType == teSubPageType.eeMapsListCourses)
            {
                acGridView.Columns.Add(CreateHyperLinkField(new string[] { "CourseID" }, "Course", "~/course.aspx?id={0}"));
                acGridView.Columns[lnCol].HeaderText = "Course";
                acGridView.Columns[lnCol++].SortExpression = "Course";
            }
            else if (aeType == teSubPageType.eeMapsListTag)
            {
                acGridView.Columns.Add(CreateBoundField("ServerRuns", "ServerRuns", true, true));
                acGridView.Columns[lnCol].SortExpression = "ServerRuns";
                acGridView.Columns[lnCol++].HeaderText = "Server Runs";

                acGridView.Columns.Add(CreateBoundField("Added", "Added", true, true));
                acGridView.Columns[lnCol].SortExpression = "Added";
                acGridView.Columns[lnCol++].HeaderText = "Date Added";
            }

            if (aeType != teSubPageType.eeMapsListTag)
            {
                acGridView.Columns.Add(CreateBoundField("CourseCompletions", "CourseCompletions", true, true));
                acGridView.Columns[lnCol].SortExpression = "CourseCompletions";
                acGridView.Columns[lnCol++].HeaderText = "Course<br/>Completions";

                acGridView.Columns.Add(CreateBoundField("NocheckPlayers", "NocheckPlayers", true, true));
                acGridView.Columns[lnCol].SortExpression = "NocheckPlayers";
                acGridView.Columns[lnCol++].HeaderText = "Nocheck<br/>Players";

                acGridView.Columns.Add(CreateBoundField("CpPlayers", "CpPlayers", true, true));
                acGridView.Columns[lnCol].SortExpression = "CpPlayers";
                acGridView.Columns[lnCol++].HeaderText = "Checkpoint<br/>Players";

                acGridView.Columns.Add(CreateBoundField("WR", "WR", true, true));
                acGridView.Columns[lnCol].SortExpression = "WR";
                acGridView.Columns[lnCol++].HeaderText = "World<br/>Records";

                acGridView.Columns.Add(CreateBoundField("Added", "Added", true, true));
                acGridView.Columns[lnCol].SortExpression = "Added";
                acGridView.Columns[lnCol++].HeaderText = "Date<br/>Added";
            }

            acGridView.AllowSorting = true;
            acGridView.AllowPaging = false;

            return acGridView;
        }

        static private tcGridView SetWrPageColumns(tcGridView acGridView, teSubPageType aeType)
        {
            int lnCol = 0;
            acGridView.AutoGenerateColumns = false;
            acGridView.AllowSorting = true;
            acGridView.AllowPaging = false;

            if (aeType != teSubPageType.eeWrPageAll)
            {
                acGridView.Columns.Add(CreateBoundField("Rank", "Rank", true,true));
                acGridView.Columns[lnCol].HeaderText = "Current<br/>Rank";
                acGridView.Columns[lnCol++].SortExpression = "Rank";
            }

            if (aeType == teSubPageType.eeWrPageAll)
            {
                acGridView.Columns.Add(CreateHyperLinkField(new string[] { "Map" }, "Map", "~/map.aspx?id={0}"));
                acGridView.Columns[lnCol].HeaderText = "Map";
                acGridView.Columns[lnCol++].SortExpression = "Map";

                acGridView.Columns.Add(CreateHyperLinkField(new string[] { "CourseID" }, "Course", "~/course.aspx?id={0}"));
                acGridView.Columns[lnCol].HeaderText = "Course";
                acGridView.Columns[lnCol++].SortExpression = "Course";
            }

            if (aeType != teSubPageType.eeWrPageCountry)
            {
                acGridView.Columns.Add(CreatePlayerNameField());
                acGridView.Columns[lnCol].SortExpression = "Player";
                acGridView.Columns[lnCol++].HeaderText = "Player";
            }
            else
            {
                acGridView.Columns.Add(CreateCountryNameField());
                acGridView.Columns[lnCol].HeaderText = "Country";
                acGridView.Columns[lnCol++].SortExpression = "Country";
            }

            if (aeType == teSubPageType.eeWrPageAll)
            {
                acGridView.Columns.Add(CreateBoundField("Time", "Time", true));
                acGridView.Columns[lnCol].HeaderText = "Time";
                acGridView.Columns[lnCol++].SortExpression = "Time";

                acGridView.Columns.Add(CreateBoundField("Date", "Date", true));
                acGridView.Columns[lnCol].HeaderText = "Date";
                acGridView.Columns[lnCol++].SortExpression = "Date";
            }

            if (aeType != teSubPageType.eeWrPageAll)
            {
                acGridView.Columns.Add(CreateBoundField("CurrentWR", "CurrentWR", true,true));
                acGridView.Columns[lnCol].HeaderText = "Current<br/>World Records";
                acGridView.Columns[lnCol++].SortExpression = "CurrentWR";

                acGridView.Columns.Add(CreateBoundField("TotalWR", "TotalWR", true,true));
                acGridView.Columns[lnCol].HeaderText = "Lifetime<br/>World Records";
                acGridView.Columns[lnCol++].SortExpression = "TotalWR";

                acGridView.Columns.Add(CreateBoundField("TotalCourses", "TotalCourses", true,true));
                acGridView.Columns[lnCol].HeaderText = "Unique<br/>Courses";
                acGridView.Columns[lnCol++].SortExpression = "TotalCourses";
            }

            return acGridView;
        }

        static private tcGridView SetPlayerPageColumns(tcGridView acGridView, teSubPageType aeType)
        {
            int lnCol = 0;
            acGridView.AutoGenerateColumns = false;
            acGridView.AllowSorting = true;
            acGridView.AllowPaging = false;

            acGridView.Columns.Add(CreateHyperLinkField(new string[] { "Map" }, "Map", "~/map.aspx?id={0}"));
            acGridView.Columns[lnCol].HeaderText = "Map";
            acGridView.Columns[lnCol++].SortExpression = "Map";

            acGridView.Columns.Add(CreateHyperLinkField(new string[] { "CourseID" }, "Course", "~/course.aspx?id={0}"));
            acGridView.Columns[lnCol].HeaderText = "Course";
            acGridView.Columns[lnCol++].SortExpression = "Course";

            if (aeType != teSubPageType.eePlayerPageIncomplete)
            {
                acGridView.Columns.Add(CreateBoundField("Time", "Time", true));
                acGridView.Columns[lnCol].HeaderText = "Time";
                acGridView.Columns[lnCol++].SortExpression = "Time";

                if (aeType == teSubPageType.eePlayerPageCp)
                {
                    acGridView.Columns.Add(CreateBoundField("Cp", "Cp", true));
                    acGridView.Columns[lnCol].HeaderText = "Checks";
                    acGridView.Columns[lnCol++].SortExpression = "Cp";

                    acGridView.Columns.Add(CreateBoundField("Teles", "Teles", true));
                    acGridView.Columns[lnCol].HeaderText = "Teles";
                    acGridView.Columns[lnCol++].SortExpression = "Teles";
                }

                if (aeType != teSubPageType.eePlayerPageWr)
                {
                    acGridView.Columns.Add(CreateRankField());
                    acGridView.Columns[lnCol].HeaderText = "Rank";
                    acGridView.Columns[lnCol].HeaderStyle.Width = 64;
                    acGridView.Columns[lnCol].HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
                    acGridView.Columns[lnCol].ItemStyle.HorizontalAlign = HorizontalAlign.Center;
                    acGridView.Columns[lnCol++].SortExpression = "rank";

                    acGridView.Columns.Add(CreateBoundField("Server", "Server", true));
                    acGridView.Columns[lnCol].HeaderText = "Server";
                    acGridView.Columns[lnCol].HeaderStyle.Width = 256;
                    acGridView.Columns[lnCol].ItemStyle.Width = 256;
                    acGridView.Columns[lnCol++].SortExpression = "Server";
                }

                if (aeType == teSubPageType.eePlayerPageWr)
                {
                    acGridView.Columns.Add(CreateBoundField("Date", "Date", true));
                    acGridView.Columns[lnCol].HeaderText = "Date";
                    acGridView.Columns[lnCol++].SortExpression = "Date";

                    acGridView.Columns.Add(CreateBoundField("Status", "Status", true));
                    acGridView.Columns[lnCol].HeaderText = "Status";
                    acGridView.Columns[lnCol++].SortExpression = "Status";
                }
            }
            else
            {
                acGridView.AllowPaging = true;
                acGridView.PageSize = 30;
            }

            return acGridView;
        }

        static private tcGridView SetPlayerListPageColumns(tcGridView acGridView, teSubPageType aeType)
        {
            int lnCol = 0;
            acGridView.AutoGenerateColumns = false;
            acGridView.PageSize = 30;

            acGridView.Columns.Add(CreateBoundField("Rank", "Rank", true));
            acGridView.Columns[lnCol].HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
            acGridView.Columns[lnCol].ItemStyle.HorizontalAlign = HorizontalAlign.Center;
            acGridView.Columns[lnCol].SortExpression = "Rank";
            acGridView.Columns[lnCol++].HeaderText = "Rank";

            if (aeType != teSubPageType.eePlayerListCountry)
            {
                acGridView.Columns.Add(CreatePlayerNameField());
                acGridView.Columns[lnCol].SortExpression = "Player";
                acGridView.Columns[lnCol++].HeaderText = "Player";
            }
            else if (aeType == teSubPageType.eePlayerListCountry)
            {
                acGridView.Columns.Add(CreateCountryNameField());
                acGridView.Columns[lnCol].HeaderText = "Country";
                acGridView.Columns[lnCol++].SortExpression = "Country";

                acGridView.Columns.Add(CreateBoundField("Players", "Players", true));
                acGridView.Columns[lnCol].HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
                acGridView.Columns[lnCol].ItemStyle.HorizontalAlign = HorizontalAlign.Center;
                acGridView.Columns[lnCol].SortExpression = "Players";
                acGridView.Columns[lnCol++].HeaderText = "Players";
            }

            if (aeType == teSubPageType.eePlayerListCountry || aeType == teSubPageType.eePlayerListPlayer)
            {
                acGridView.Columns.Add(CreateBoundField("Records", "Records", true, true));
                acGridView.Columns[lnCol].SortExpression = "Records";
                acGridView.Columns[lnCol++].HeaderText = "Nocheck<br/>Records";

                acGridView.Columns.Add(CreateBoundField("RecordsCp", "RecordsCp", true, true));
                acGridView.Columns[lnCol].SortExpression = "RecordsCp";
                acGridView.Columns[lnCol++].HeaderText = "Checkpoint<br/>Records";
            }

            if (aeType == teSubPageType.eePlayerListTagRank)
            {
                acGridView.Columns.Add(CreateBoundField("round_wins", "round_wins", true, true));
                acGridView.Columns[lnCol].SortExpression = "round_wins";
                acGridView.Columns[lnCol++].HeaderText = "Round<br/>Wins";

                acGridView.Columns.Add(CreateBoundField("rounds_played", "rounds_played", true, true));
                acGridView.Columns[lnCol].SortExpression = "rounds_played";
                acGridView.Columns[lnCol++].HeaderText = "Rounds<br/>Played";

                acGridView.Columns.Add(CreateTagRoundWinPercentField());
                acGridView.Columns[lnCol].HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
                acGridView.Columns[lnCol].ItemStyle.HorizontalAlign = HorizontalAlign.Center;
                acGridView.Columns[lnCol].HeaderText = "Win<br/>Rate";
                acGridView.Columns[lnCol++].SortExpression = "round_wins";
                
                acGridView.Columns.Add(CreateBoundField("match_wins", "match_wins", true, true));
                acGridView.Columns[lnCol].SortExpression = "match_wins";
                acGridView.Columns[lnCol++].HeaderText = "Match<br/>Wins";

                acGridView.Columns.Add(CreateBoundField("tags", "tags", true, true));
                acGridView.Columns[lnCol].SortExpression = "tags";
                acGridView.Columns[lnCol++].HeaderText = "Tags";

                acGridView.Columns.Add(CreateBoundField("ninja", "ninja", true, true));
                acGridView.Columns[lnCol].SortExpression = "ninja";
                acGridView.Columns[lnCol++].HeaderText = "Ninja<br/>Tags";

                acGridView.Columns.Add(CreateBoundField("tagged", "tagged", true, true));
                acGridView.Columns[lnCol].SortExpression = "tagged";
                acGridView.Columns[lnCol++].HeaderText = "Tagged";
            }

            if (aeType == teSubPageType.eePlayerListTagStats)
            {
                acGridView.Columns.Add(CreateBoundField("avg_tags_per_round", "avg_tags_per_round", true, true));
                acGridView.Columns[lnCol].SortExpression = "avg_tags_per_round";
                acGridView.Columns[lnCol++].HeaderText = "Avg tags<br/>per round";

                acGridView.Columns.Add(CreateBoundField("avg_tagged_per_round", "avg_tagged_per_round", true, true));
                acGridView.Columns[lnCol].SortExpression = "avg_tagged_per_round";
                acGridView.Columns[lnCol++].HeaderText = "Avg tagged<br/>per round";

                acGridView.Columns.Add(CreateBoundField("avg_powerup_per_round", "avg_powerup_per_round", true, true));
                acGridView.Columns[lnCol].SortExpression = "avg_powerup_per_round";
                acGridView.Columns[lnCol++].HeaderText = "Avg powerups<br/>per round";

                //acGridView.Columns.Add(CreateBoundField("total_untagged_time", "total_untagged_time", true, true));
                //acGridView.Columns[lnCol].SortExpression = "total_untagged_time";
                //acGridView.Columns[lnCol++].HeaderText = "Total<br/>untagged<br/>time";

                acGridView.Columns.Add(CreateBoundField("round_most_untagged", "round_most_untagged", true, true));
                acGridView.Columns[lnCol].SortExpression = "round_most_untagged";
                acGridView.Columns[lnCol++].HeaderText = "Most untagged<br/> time in a round";

                acGridView.Columns.Add(CreateBoundField("round_most_powerup", "round_most_powerup", true, true));
                acGridView.Columns[lnCol].SortExpression = "round_most_powerup";
                acGridView.Columns[lnCol++].HeaderText = "Most<br/>powerups<br/>per round";
            }
                        
            acGridView.AllowSorting = true;
            acGridView.AllowPaging = true;

            return acGridView;
        }

        static private HyperLinkField CreateHyperLinkField(string[] aapanUrlNavigateVar, string apanUrlText, string apanUrl)
        {
            HyperLinkField lcHyperLinkField = new HyperLinkField();

            lcHyperLinkField.DataNavigateUrlFields = aapanUrlNavigateVar;
            lcHyperLinkField.DataTextField = apanUrlText;
            lcHyperLinkField.DataNavigateUrlFormatString = apanUrl;

            return lcHyperLinkField;
        }

        static private BoundField CreateBoundField(string apanHeaderText, string apanDataField, bool aeVisible=true,bool aeCenter=false, bool aeHtmlEncode=false, int anWidth=0)
        {
            BoundField lcReturn = new BoundField();

            if (aeCenter)
            {
                lcReturn.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
                lcReturn.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
            }

            if (anWidth != 0)
            {
                lcReturn.ItemStyle.Width = anWidth;
            }

            lcReturn.HtmlEncode = aeHtmlEncode;
            lcReturn.DataField = apanDataField;
            lcReturn.HeaderText = apanHeaderText;
            lcReturn.Visible = aeVisible;

            return lcReturn;
        }

        static private ImageField CreateImageField(string apanUrlNavigateVar, string apanUrl)
        {
            ImageField lcImageField = new ImageField();

            lcImageField.DataImageUrlField = apanUrlNavigateVar;
            lcImageField.DataImageUrlFormatString = apanUrl;

            return lcImageField;
        }

        //Custom field for player with country's flag
        static private TemplateField CreatePlayerNameField()
        {
            TemplateField lcReturn = new TemplateField();

            lcReturn.ItemTemplate = new PlayerField();

            return lcReturn;
        }

        class PlayerField : ITemplate
        {
            public void InstantiateIn(Control acContainer)
            {
                System.Web.UI.WebControls.Image lcImage = new System.Web.UI.WebControls.Image();
                acContainer.Controls.Add(lcImage);
                lcImage.DataBinding += new EventHandler(mpfFlagImageBinding);

                HyperLink lcPlayerNameLabel = new HyperLink();
                acContainer.Controls.Add(lcPlayerNameLabel);
                lcPlayerNameLabel.DataBinding += new EventHandler(mpfHyperLinkBinding);
            }

            private void mpfFlagImageBinding(object acSender, EventArgs e)
            {
                System.Web.UI.WebControls.Image lcImage = (System.Web.UI.WebControls.Image)acSender;
                GridViewRow lcContainer = (GridViewRow)lcImage.NamingContainer;

                object lcCountryValue = DataBinder.Eval(lcContainer.DataItem, "Country");

                if (lcCountryValue != DBNull.Value)
                {
                    lcImage.ImageUrl = "~/img/flags/" + lcCountryValue.ToString() + ".gif";
                    lcImage.ToolTip = tcCountryTable.CodeToFull(lcCountryValue.ToString());
                }
            }

            private void mpfHyperLinkBinding(object acSender, EventArgs e)
            {
                HyperLink lcHyperLink = (HyperLink)acSender;
                GridViewRow lcContainer = (GridViewRow)lcHyperLink.NamingContainer;

                object lcPlayerValue = DataBinder.Eval(lcContainer.DataItem, "Player");
                object lcSteamIdValue = DataBinder.Eval(lcContainer.DataItem, "SteamID");

                if (lcPlayerValue != DBNull.Value)
                {
                    lcHyperLink.Text = " " + lcPlayerValue.ToString();
                    lcHyperLink.NavigateUrl = "~/player.aspx?id=" + lcSteamIdValue.ToString();
                    lcHyperLink.ForeColor = teColors.eeLink;
                    lcHyperLink.Font.Bold = true;
                    lcHyperLink.Font.Underline = false;

                    if (lcHyperLink.Text.Length > 21)
                    {
                        lcHyperLink.ToolTip = lcHyperLink.Text;
                        lcHyperLink.Text = lcHyperLink.Text.Substring(0, 20);
                    }
                }
            }
        }

        static private TemplateField CreateCountryNameField()
        {
            TemplateField lcReturn = new TemplateField();

            lcReturn.ItemTemplate = new CountryField();

            return lcReturn;
        }

        //Custom field for country with flag
        class CountryField : ITemplate
        {
            public void InstantiateIn(Control acContainer)
            {
                System.Web.UI.WebControls.Image lcImage = new System.Web.UI.WebControls.Image();
                acContainer.Controls.Add(lcImage);
                lcImage.DataBinding += new EventHandler(mpfFlagImageBindingCountry);

                Label lcCountryNameLabel = new Label();
                acContainer.Controls.Add(lcCountryNameLabel);
                lcCountryNameLabel.DataBinding += new EventHandler(mpfLabelBinding);
            }

            private void mpfFlagImageBindingCountry(object acSender, EventArgs e)
            {
                System.Web.UI.WebControls.Image lcImage = (System.Web.UI.WebControls.Image)acSender;
                GridViewRow lcContainer = (GridViewRow)lcImage.NamingContainer;

                object lcCountryValue = DataBinder.Eval(lcContainer.DataItem, "Country");

                if (lcCountryValue != DBNull.Value)
                {
                    lcImage.ImageUrl = "~/img/flags/" + lcCountryValue.ToString() + ".gif";
                }
            }

            private void mpfLabelBinding(object acSender, EventArgs e)
            {
                Label lcLabel = (Label)acSender;
                GridViewRow lcContainer = (GridViewRow)lcLabel.NamingContainer;

                object lcCountryValue = DataBinder.Eval(lcContainer.DataItem, "CountryFull");

                if (lcCountryValue != DBNull.Value)
                {
                    lcLabel.Text = " " + lcCountryValue.ToString();
                    lcLabel.ForeColor = teColors.eeText;
                    lcLabel.Font.Bold = true;
                }
            }
        }

        static private TemplateField CreateTagRoundWinPercentField()
        {
            TemplateField lcReturn = new TemplateField();

            lcReturn.ItemTemplate = new TagRoundWinPercentField();

            return lcReturn;
        }

        //Custom field for tag win percentage
        class TagRoundWinPercentField : ITemplate
        {
            public void InstantiateIn(Control acContainer)
            {
                Label lcLabel = new Label();
                acContainer.Controls.Add(lcLabel);
                lcLabel.DataBinding += new EventHandler(mpfTagWinPctLabelBinding);
            }

            private void mpfTagWinPctLabelBinding(object acSender, EventArgs e)
            {
                Label lcLabel = (Label)acSender;
                GridViewRow lcContainer = (GridViewRow)lcLabel.NamingContainer;

                object lcRoundsPlayed = DataBinder.Eval(lcContainer.DataItem, "rounds_played");
                object lcRoundsWon = DataBinder.Eval(lcContainer.DataItem, "round_wins");
                float lrWinPct = 0f;

                if (float.Parse(lcRoundsPlayed.ToString()) > 0)
                {
                    lrWinPct = float.Parse(lcRoundsWon.ToString()) / float.Parse(lcRoundsPlayed.ToString());
                }

                if (lcRoundsPlayed != DBNull.Value && lcRoundsWon != DBNull.Value)
                {
                    lcLabel.Text = String.Format("{0:0.0}", lrWinPct * 100f) + "%";
                    lcLabel.ForeColor = teColors.eeText;
                }
            }
        }

        static private TemplateField CreateRankField()
        {
            TemplateField lcReturn = new TemplateField();

            lcReturn.ItemTemplate = new RankField();

            return lcReturn;
        }

        //Custom field for player rank on a course
        class RankField : ITemplate
        {
            public void InstantiateIn(Control acContainer)
            {
                Label lcRankLabel = new Label();
                acContainer.Controls.Add(lcRankLabel);
                lcRankLabel.DataBinding += new EventHandler(mpfRankLabelBinding);
            }

            private void mpfRankLabelBinding(object acSender, EventArgs e)
            {
                Label lcLabel = (Label)acSender;
                GridViewRow lcContainer = (GridViewRow)lcLabel.NamingContainer;

                object lcRank = DataBinder.Eval(lcContainer.DataItem, "rank");
                object lcPlayers = DataBinder.Eval(lcContainer.DataItem, "pcount");

                if (lcRank != DBNull.Value && lcPlayers != DBNull.Value)
                {
                    lcLabel.Text = lcRank.ToString() + " / " + lcPlayers.ToString();
                    lcLabel.ForeColor = teColors.eeText;
                }
            }
        }
    }
}
