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

namespace kzstats
{
    public partial class player : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string lpanSteamId = Request.QueryString["id"];
            
            lpanSteamId = StaticMethods.CheckSqlReqValidity(lpanSteamId, "STEAM_0:1:5134837");
            gcLabelSteamId.Text = lpanSteamId.Replace("%3a", ":");

            if (Page.IsPostBack == false)
            {
                SetUpDropDown(lpanSteamId);
            }

            SetNameLabel(lpanSteamId);

            if (HttpContext.Current.Request.UserAgent.Contains("Chrome") == true || HttpContext.Current.Request.UserAgent.Contains("Firefox") == true)
            {
                gcBody.Attributes.Add("style", tcBgStyler.Instance.GetBgImageStyle());
            }
            DoPageLoad();
        }

        private void DoPageLoad()
        {
            Stopwatch lcStopWatch = new Stopwatch(); lcStopWatch.Reset(); lcStopWatch.Start();
            tcGridViewType lcGridViewType = new tcGridViewType(teGridViewType.eePlayerPage, teSubPageType.eePlayerPageNocheck);
            
            string lpanSteamId = Request.QueryString["id"];

            lpanSteamId = StaticMethods.CheckSqlReqValidity(lpanSteamId, "STEAM_0:1:5134837");

            gcLabelSteamId.Text = lpanSteamId.Replace("%3a", ":");

            lcGridViewType.meSub = GetGridTypeFromDropDownState();

            switch (lcGridViewType.meSub)
            {
                case teSubPageType.eePlayerPageStats:
                    ProcessStatsPage(lpanSteamId);
                    break;
                case teSubPageType.eePlayerPageTag:
                    try
                    {
                        ProcessTagPage(lpanSteamId);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("ProcessTagPage: " + e.Message);
                    }
                    break;
                default:
                    string lpanQuery = GetQueryFromDropDownState(lpanSteamId);
                    ProcessRecordsPage(lpanQuery, lcGridViewType);
                    break;
            }

            tcLinkPanel.AddLinkPanel(this);

            Panel4.DataBind();
            
            lcStopWatch.Stop(); gcPageLoad.Text = lcStopWatch.ElapsedMilliseconds.ToString();
        }

        private teSubPageType GetGridTypeFromDropDownState()
        {
            switch (gcDropDown.SelectedValue)
            {
                case "cp":
                    return teSubPageType.eePlayerPageCp;
                case "wr":
                    return teSubPageType.eePlayerPageWr;
                case "tag":
                    return teSubPageType.eePlayerPageTag;
                case "unfinnocheck":
                    return teSubPageType.eePlayerPageIncomplete;
                case "unfincp":
                    return teSubPageType.eePlayerPageIncomplete;
                case "stats":
                    return teSubPageType.eePlayerPageStats;
                default:
                    return teSubPageType.eePlayerPageNocheck;
            }
        }

        private void ProcessRecordsPage(string apanQuery,tcGridViewType aeType)
        {
            Label lcNoRecords = new Label();
            tcMySqlDataSource lcMySqlDataSource = new tcMySqlDataSource(apanQuery);

            DataTable lcDataTable = lcMySqlDataSource.GetDataTable();

            if (lcDataTable != null && lcDataTable.Rows.Count > 0)
            {
                tcGridView lcGridView = GridViewFactory.Create(lcDataTable, aeType);
                Panel4.Controls.AddAt(0, lcGridView);
            }
            else
            {
                lcNoRecords.Text = "No records";
                lcNoRecords.ForeColor = teColors.eeRedText;
                lcNoRecords.Font.Name = "Arial";

                Panel4.Controls.AddAt(0, lcNoRecords);
            }
        }

        private void ProcessTagPage(string apanSteamId)
        {
            int lnTags = 0;
            int lnTagsNinja = 0;
            int lnTagged = 0;
            int lnRoudWins = 0;
            int lnRoundsPlayed = 0;
            int lnMatchWins = 0;
            int lnTotalUntaggedTime = 0;
            int lnRoundMostUntagged = 0;
            int lnRoundLeastUntagged = 0;
            int lnRoundMostPowerups = 0;
            int lnRoundLeastPowerups = 0;
            float lrAvgRoundPowerup = 0f;
            float lrAvgRoundTag = 0f;
            float lrAvgRoundTagged = 0f;

            tcMySqlCommand lcMySqlCommand = new tcMySqlCommand(@"SELECT tags, ninja, tagged, round_wins, rounds_played, match_wins, total_untagged_time, round_most_untagged, round_least_untagged, round_most_powerup, round_least_powerup, avg_powerup_per_round, avg_tags_per_round, avg_tagged_per_round FROM
                                                                (SELECT id FROM kzs_players WHERE auth = ?auth) AS P
                                                                LEFT JOIN (SELECT * FROM kzs_tag) AS T
                                                                ON P.id = T.player");

            lcMySqlCommand.mcMySqlCommand.Parameters.Add("?auth", apanSteamId);

            MySqlDataReader lcMySqlReader = lcMySqlCommand.mcMySqlCommand.ExecuteReader();
            lcMySqlReader.Read();

            if (lcMySqlReader.HasRows)
            {
                lnTags = lcMySqlReader.GetInt32(0);
                lnTagsNinja = lcMySqlReader.GetInt32(1);
                lnTagged = lcMySqlReader.GetInt32(2);
                lnRoudWins = lcMySqlReader.GetInt32(3);
                lnRoundsPlayed = lcMySqlReader.GetInt32(4);
                lnMatchWins = lcMySqlReader.GetInt32(5);
                lnTotalUntaggedTime = lcMySqlReader.GetInt32(6);
                lnRoundMostUntagged = lcMySqlReader.GetInt32(7);
                lnRoundLeastUntagged = lcMySqlReader.GetInt32(8);
                lnRoundMostPowerups = lcMySqlReader.GetInt32(9);
                lnRoundLeastPowerups = lcMySqlReader.GetInt32(10);
                lrAvgRoundPowerup = lcMySqlReader.GetFloat(11);
                lrAvgRoundTag = lcMySqlReader.GetFloat(12);
                lrAvgRoundTagged = lcMySqlReader.GetFloat(13);
            }

            float lrWinPercentage = 0f;
            String lpanLeastPowerups = "~";
            String lpanLeastUntagged = "~";

            if (lnRoundsPlayed > 0)
            {
                lrWinPercentage = (lnRoudWins / (float)lnRoundsPlayed) * 100f;
            }
            if (lnRoundLeastUntagged < 9000)
            {
                lpanLeastUntagged = lnRoundLeastUntagged.ToString();
            }
            if (lnRoundLeastPowerups < 9000)
            {
                lpanLeastPowerups = lnRoundLeastPowerups.ToString();
            }

            tcLabel lcHeading = new tcLabel();
            tcLabel lcText = new tcLabel();
            lcHeading.ForeColor = teColors.eeRedText;
            lcHeading.Font.Bold = true;

            lcHeading.Text = "<br>General stats<br>";
            lcText.Text = @"Tags: " + lnTags + " (" + lnTagsNinja + " Ninja)" + ", Tagged: " + lnTagged +
                            "<br>Rounds played: " + lnRoundsPlayed + ", round wins: " + lnRoudWins + " (" + String.Format("{0:0.##}", lrWinPercentage) + "%)" +
                            "<br>Match wins: " + lnMatchWins +
                            "<br>Total untagged time: " + lnTotalUntaggedTime + "<br><br>";

            Panel4.Controls.Add(lcHeading);
            Panel4.Controls.Add(lcText);

            tcLabel lcRoundHeading = new tcLabel();
            tcLabel lcRoundText = new tcLabel();
            lcRoundHeading.ForeColor = teColors.eeRedText;
            lcRoundHeading.Font.Bold = true;

            lcRoundHeading.Text = "Round records<br>";
            lcRoundText.Text = @"Most untagged time: " + lnRoundMostUntagged + 
                            "<br>Least untagged time: " + lpanLeastUntagged + 
                            "<br>Most powerups: " + lnRoundMostPowerups +
                            "<br>Fewest powerups: " + lpanLeastPowerups + "<br><br>";

            Panel4.Controls.Add(lcRoundHeading);
            Panel4.Controls.Add(lcRoundText);

            tcLabel lcAvgHeading = new tcLabel();
            tcLabel lcAvgText = new tcLabel();
            lcAvgHeading.ForeColor = teColors.eeRedText;
            lcAvgHeading.Font.Bold = true;
            lcAvgHeading.Text = "Round averages<br>";

            lcAvgText.Text = "Tags: " + String.Format("{0:0.##}", lrAvgRoundTag) +
                            "<br>Tagged: " + String.Format("{0:0.##}", lrAvgRoundTagged) +
                            "<br>Powerups: " + String.Format("{0:0.##}", lrAvgRoundPowerup);

            Panel4.Controls.Add(lcAvgHeading);
            Panel4.Controls.Add(lcAvgText);

            lcMySqlCommand.Close();
        }

        private void ProcessStatsPage(string apanSteamId)
        {
            tcLabel lcHeading = new tcLabel();

            try
            {
                Panel4.HorizontalAlign = HorizontalAlign.Left;
                Panel4.BorderStyle = BorderStyle.Solid;
                Panel4.BorderColor = Color.Transparent;
                Panel4.BorderWidth = 32;

                //aliases, country, medals, maps finished
                Panel4.Controls.Add(new LiteralControl("<div align=\"center\" style=\"line-height:24px;\">"));
                SetMiscInfo(apanSteamId);

                SetAliases(apanSteamId);

                Panel4.Controls.Add(new LiteralControl("<div align=\"center\" style=\"line-height:24px;\">"));

                lcHeading.Text = "Medals earned";
                SetMedalLabels(apanSteamId);

                SetRewardLabels(apanSteamId);

                SetSteamCommunityInfo(apanSteamId);
                Panel4.Controls.Add(new LiteralControl("</div></div>"));
            }
            catch(Exception e)
            {
                Debug.WriteLine("PlayerStats: " + e.Message);
            }
        }

        private void SetMiscInfo(string apanSteamId)
        {
            tcMySqlCommand lcMySqlCommand = new tcMySqlCommand(@"SELECT  DATE_FORMAT(date,'%Y-%m-%d') AS Date,IFNULL(country,0), IFNULL(full,'Unknown') AS CountryFull, IFNULL(cp,0) AS RecordsCp, IFNULL(nocheck,0) AS Records, IFNULL(totalwr,0) AS WrTotal, IFNULL(curwr,0) AS WrCurrent FROM

                                                                (SELECT id,date,country FROM kzs_players WHERE auth = ?auth) AS P

                                                                LEFT JOIN (SELECT code,full FROM kzs_countries) AS CNT
                                                                ON CNT.code = P.country

                                                                LEFT JOIN (SELECT player,COUNT(DISTINCT(course))AS nocheck FROM kzs_records GROUP BY player) AS R
                                                                ON R.player = P.id

                                                                LEFT JOIN (SELECT player,COUNT(DISTINCT(course)) AS cp FROM kzs_recordscp GROUP BY player) AS RCP
                                                                ON RCP.player = P.id

                                                                LEFT JOIN (SELECT player, COUNT(*)  AS totalwr FROM kzs_wrs GROUP BY player) AS WRT
                                                                ON WRT.player = P.id

                                                                LEFT JOIN (SELECT player,COUNT(*) AS curwr FROM

                                                                            (SELECT * FROM

                                                                            (SELECT W.player,W.course,W.time,W.date,P.auth, C.invert FROM

                                                                            ((SELECT player, course, time, date FROM kzs_wrs) AS W

                                                                            LEFT JOIN(SELECT id,auth FROM kzs_players) AS P
                                                                            ON P.id = W.player

                                                                            LEFT JOIN (SELECT invert,id FROM kzs_courses) AS C
                                                                            ON C.id = W.course)

                                                                            ORDER BY course, date, CASE 1 WHEN C.invert THEN Time ELSE -Time END DESC) AS F

                                                                            GROUP BY course) AS FF WHERE auth = ?auth) AS WRCUR
                                                                ON WRCUR.player = P.id");

            lcMySqlCommand.mcMySqlCommand.Parameters.Add("?auth", apanSteamId);
            
            MySqlDataReader lcReader = lcMySqlCommand.mcMySqlCommand.ExecuteReader();
            lcReader.Read();
            
            string lpanStartDate = lcReader.GetString(0);
            string lpanCountryCode = lcReader.GetString(1);
            string lpanCountryFull = lcReader.GetString(2);
            int lnRecCp = lcReader.GetInt32(3);
            int lnRec = lcReader.GetInt32(4);
            int lnWrTotal = lcReader.GetInt32(5);
            int lnWrcur = lcReader.GetInt32(6);
            
            lcReader.Close(); lcMySqlCommand.Close();

            tcLabel lcDate = new tcLabel();
            tcLabel lcCountry = new tcLabel();
            tcLabel lcRecords = new tcLabel();
            tcLabel lcWr = new tcLabel();

            tcLabel lcHeading = new tcLabel();
            lcHeading.Text = "General information";
            lcHeading.ForeColor = teColors.eeRedText;
            lcHeading.Font.Bold = true;
            Panel4.Controls.Add(lcHeading);
            Panel4.Controls.Add(new LiteralControl("<br>"));

            lcDate.Text = "Playing since " + lpanStartDate + " from ";
            Panel4.Controls.Add(lcDate);
            System.Web.UI.WebControls.Image lcImage = new System.Web.UI.WebControls.Image();
            lcImage.ImageUrl = "~/img/flags/" + lpanCountryCode + ".gif";
            Panel4.Controls.Add(lcImage);
            lcCountry.Text = " " + lpanCountryFull;
            Panel4.Controls.Add(lcCountry);

            Panel4.Controls.Add(new LiteralControl("<br>"));
            lcRecords.Text = "Nochecked " + lnRec + " courses, finished " + lnRecCp + " courses with checkpoints";
            Panel4.Controls.Add(lcRecords);

            Panel4.Controls.Add(new LiteralControl("<br>"));
            lcWr.Text = "Held " + lnWrTotal + " total world records (" + lnWrcur + " current)";
            Panel4.Controls.Add(lcWr);
        }

        private void SetSteamCommunityInfo(string apanSteamId)
        {
            tcLabel lcHeading = new tcLabel();
            lcHeading.Text = "Steam information";
            lcHeading.ForeColor = teColors.eeRedText;
            lcHeading.Font.Bold = true;
            //Panel4.Controls.Add(new LiteralControl("<br>"));
            //Panel4.Controls.Add(lcHeading);
            Panel4.Controls.Add(new LiteralControl("<br>"));

            HyperLink lcLink = new HyperLink();
            lcLink.ForeColor = teColors.eeLink;
            lcLink.Text = "Steam community profile";
            lcLink.NavigateUrl = StaticMethods.GetSteamProfileLink(apanSteamId);
            
            //System.Web.UI.WebControls.Image lcImage = new System.Web.UI.WebControls.Image();
            //lcImage.ImageUrl = StaticMethods.GetPlayerImageUrl(apanSteamId);
            //lcImage.Width = lcImage.Height = 96;
            //lcImage.ImageAlign = ImageAlign.Middle;
            
            //Panel4.Controls.Add(lcImage);
            //Panel4.Controls.Add(new LiteralControl("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;"));
            Panel4.Controls.Add(lcLink);
        }

        private void SetAliases(string apanSteamId)
        {
            int lnCount = 0;
            tcLabel lcAliasesLabel = new tcLabel();
            lcAliasesLabel.Text = "Aliases: ";

            tcMySqlCommand lcMySqlCommand = new tcMySqlCommand(@"SELECT name FROM kzs_names WHERE player = (SELECT id FROM kzs_players WHERE auth = ?auth LIMIT 1) ORDER BY count DESC LIMIT 5");

            lcMySqlCommand.mcMySqlCommand.Parameters.Add("?auth", apanSteamId);
                
            MySqlDataReader lcReader = lcMySqlCommand.mcMySqlCommand.ExecuteReader();

            while (lnCount < 5 && lcReader.HasRows && !lcReader.IsClosed && lcReader.Read())
            {
                lcAliasesLabel.Text = lcAliasesLabel.Text + lcReader.GetString(0) + ", ";

                lnCount++;
            }

            lcAliasesLabel.Text = lcAliasesLabel.Text.Substring(0, lcAliasesLabel.Text.Length - 2);

            Panel4.Controls.Add(new LiteralControl("<br>"));
            Panel4.Controls.Add(lcAliasesLabel);

            lcReader.Close();
            lcMySqlCommand.Close();
        }

        private void SetUpDropDown(string apanSteamId)
        {
            string lpanNumNocheck = "";
            string lpanNumCheck = "";
            string lpanNumWr = "";
            string lpanNumUnfinNocheck = "";
            string lpanNumUnfinCp = "";
            int lnNumWr = 0;

            tcMySqlCommand lcMySqlCommand = new tcMySqlCommand(@"SELECT IFNULL(R.nocheck,0),IFNULL(RCP.cp,0),IFNULL(WR.wr,0),IFNULL(C.total,0) FROM

                                                                (SELECT id FROM kzs_players WHERE auth = ?steamid) AS P

                                                                LEFT JOIN (SELECT player,COUNT(DISTINCT(course))AS nocheck FROM kzs_records GROUP BY player) AS R
                                                                ON R.player = P.id

                                                                LEFT JOIN (SELECT player,COUNT(DISTINCT(course)) AS cp FROM kzs_recordscp GROUP BY player) AS RCP
                                                                ON RCP.player = P.id

                                                                LEFT JOIN (SELECT player, COUNT(*)  AS wr FROM kzs_wrs GROUP BY player) AS WR
                                                                ON WR.player = P.id

                                                                LEFT JOIN (SELECT COUNT(DISTINCT(id)) AS total FROM kzs_courses) AS C
                                                                ON C.total");

            lcMySqlCommand.mcMySqlCommand.Parameters.Add("?steamid", apanSteamId);
                
            MySqlDataReader lcReader = lcMySqlCommand.mcMySqlCommand.ExecuteReader();
            lcReader.Read();

            if (lcReader.HasRows == true)
            {
                lpanNumNocheck = " (" + lcReader.GetInt32(0) + ")";
                lpanNumCheck = " (" + lcReader.GetInt32(1) + ")";

                lnNumWr = lcReader.GetInt32(2);
                lpanNumWr = " (" + lnNumWr + ")";
                lpanNumUnfinNocheck = " (" + (lcReader.GetInt32(3) - lcReader.GetInt32(0)) + ")";
                lpanNumUnfinCp = " (" + (lcReader.GetInt32(3) - lcReader.GetInt32(1)) + ")";
            }

            lcMySqlCommand.Close();

            gcDropDown.AutoPostBack = true;
            gcDropDown.SelectedIndexChanged += new EventHandler(cb_DropDownTextChange);

            /*
            gcDropDown.Items.Add(new ListItem("Nocheck records" + lpanNumNocheck, "nocheck"));
            gcDropDown.Items.Add(new ListItem("Checkpoint records" + lpanNumCheck, "cp"));

            if (lnNumWr > 0)
            {
                gcDropDown.Items.Add(new ListItem("World records" + lpanNumWr, "wr"));
            }

            gcDropDown.Items.Add(new ListItem("Tag stats", "tag"));
            gcDropDown.Items.Add(new ListItem("Player stats", "stats"));
            gcDropDown.Items.Add(new ListItem("Unfinished nocheck" + lpanNumUnfinNocheck, "unfinnocheck"));
            gcDropDown.Items.Add(new ListItem("Unfinished checkpoint" + lpanNumUnfinCp, "unfincp"));
            */

            //TODO: add links for ie
            List<string> lcTextList = new List<string>();
            List<string> lcKeywordList = new List<string>();

            lcTextList.Add("Nocheck records" + lpanNumNocheck); lcKeywordList.Add("nocheck");
            lcTextList.Add("Checkpoint records" + lpanNumCheck); lcKeywordList.Add("cp");
            if (lnNumWr > 0)
            {
                lcTextList.Add("World records" + lpanNumWr); lcKeywordList.Add("wr");
            }
            lcTextList.Add("Tag stats"); lcKeywordList.Add("tag");
            lcTextList.Add("Player stats"); lcKeywordList.Add("stats");
            lcTextList.Add("Unfinished nocheck" + lpanNumUnfinNocheck); lcKeywordList.Add("unfinnocheck");
            lcTextList.Add("Unfinished checkpoint" + lpanNumUnfinCp); lcKeywordList.Add("unfincp");

            string[] lacText = lcTextList.ToArray();
            string[] lacKeyword = lcKeywordList.ToArray();

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
                    lcLink.NavigateUrl = "~/player.aspx?id=" + apanSteamId + "&sel=" + lacKeyword[i];
                    lcLink.Font.Name = "Arial";
                    lcLink.Font.Size = 11;
                    lcLink.Font.Bold = true;
                    lcLink.ForeColor = teColors.eeLink;
                    gcLinks.Controls.Add(lcLink);

                    if (i + 1 < lacText.Length && i != 3)
                    {
                        gcLinks.Controls.Add(new LiteralControl("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;"));
                    }
                    else if(i == 3)
                    {
                        gcLinks.Controls.Add(new LiteralControl("<br>"));
                    }
                    else
                    {
                        gcLinks.Controls.Add(new LiteralControl("<br><br>"));
                    }
                }
            }
        }

        private void SetNameLabel(string apanSteamId)
        {
            try
            {
                //Set name label
                tcMySqlCommand lcMySqlCommand = new tcMySqlCommand("SELECT name FROM kzs_players WHERE auth = ?steamid LIMIT 1;");
                lcMySqlCommand.mcMySqlCommand.Parameters.Add("?steamid", apanSteamId);

                MySqlDataReader lcMySqlReader = lcMySqlCommand.mcMySqlCommand.ExecuteReader();
                lcMySqlReader.Read();
                gcLabelName.Text = lcMySqlReader.GetString(0);

                lcMySqlCommand.Close();
            }
            catch (Exception e)
            {
                Debug.WriteLine("SetPageLabels: " + e.Message);
            }
        }

        private void SetRewardLabels(string apanSteamId)
        {
            try 
            {
                //Set medals labels
                tcMySqlCommand lcMySqlCommand = new tcMySqlCommand("SELECT timebomb,painkillers,soccerball,stats_readout,moonboots,strange_mojo,slowmo,burningfeet,cloak,fastmo,custom_title,psychic_anti_gravity,boots_of_height FROM kzs_medals WHERE player = (SELECT id FROM kzs_players WHERE auth = ?steamid);");
                lcMySqlCommand.mcMySqlCommand.Parameters.Add("?steamid", apanSteamId);
                MySqlDataReader lcMySqlReader = lcMySqlCommand.mcMySqlCommand.ExecuteReader();
                lcMySqlReader.Read();

                Panel4.Controls.Add(new LiteralControl("<br><br>"));
                tcLabel lcHeading = new tcLabel();
                lcHeading.Text = "Rewards purchased";
                lcHeading.ForeColor = teColors.eeRedText;
                lcHeading.Font.Bold = true;
                Panel4.Controls.Add(lcHeading);
                Panel4.Controls.Add(new LiteralControl("<br><div style=\"line-height:56px;\"><table>"));

                for (int lnRewardIndex = 0; lnRewardIndex < (int)teRewards.eeMax; lnRewardIndex++)
                {
                    System.Web.UI.WebControls.Image lcImage = new System.Web.UI.WebControls.Image();
                    lcImage.ImageAlign = ImageAlign.Middle;
                    lcImage.Height = lcImage.Width = 48;
                    lcImage.ToolTip = ((teRewards)lnRewardIndex).ToString().Substring(2);

                    tcLabel lcRewardName = new tcLabel();

                    switch ((teRewards)lnRewardIndex)
                    {
                        case teRewards.eeTimeBomb:
                            lcImage.ImageUrl = "~/img/rewards/timebomb.png";
                            lcRewardName.Text = "Timebomb";
                            break;
                        case teRewards.eePainKillers:
                            lcImage.ImageUrl = "~/img/rewards/painkillers.png";
                            lcRewardName.Text = "Painkillers";
                            break;
                        case teRewards.eeSoccerBall:
                            lcImage.ImageUrl = "~/img/rewards/soccerball.png";
                            lcRewardName.Text = "Soccer ball";
                            break;
                        case teRewards.eeStatsReadout:
                            lcImage.ImageUrl = "~/img/rewards/stats_readout.png";
                            lcRewardName.Text = "Stats readout";
                            break;
                        case teRewards.eeMoonBoots:
                            lcImage.ImageUrl = "~/img/rewards/moonboots.png";
                            lcRewardName.Text = "Moon boots";
                            break;
                        case teRewards.eeStrangeMojo:
                            lcImage.ImageUrl = "~/img/rewards/strange_mojo.png";
                            lcRewardName.Text = "Strange mojo";
                            break;
                        case teRewards.eeSlowmo:
                            lcImage.ImageUrl = "~/img/rewards/slomo.png";
                            lcRewardName.Text = "Slow-mo";
                            break;
                        case teRewards.eeBurningFeet:
                            lcImage.ImageUrl = "~/img/rewards/burningfeet.png";
                            lcRewardName.Text = "Burning feet";
                            break;
                        case teRewards.eeCloak:
                            lcImage.ImageUrl = "~/img/rewards/cloak.png";
                            lcRewardName.Text = "Cloak";
                            break;
                        case teRewards.eeFastmo:
                            lcImage.ImageUrl = "~/img/rewards/boots_of_speed.png";
                            lcRewardName.Text = "Boots of speed";
                            break;
                        case teRewards.eeCustomTitle:
                            lcImage.ImageUrl = "~/img/rewards/custom_message.png";
                            lcRewardName.Text = "Custom title";
                            break;
                        case teRewards.eePsychicAntiGravity:
                            lcImage.ImageUrl = "~/img/rewards/psychic_anti_gravity.png";
                            lcRewardName.Text = "Psychic anti-gravity";
                            break;
                        case teRewards.eeBootsOfHeight:
                            lcImage.ImageUrl = "~/img/rewards/boots_of_height.png";
                            lcRewardName.Text = "Boots of height";
                            break;
                    }

                    tcLabel lcNumRewards = new tcLabel();
                    lcNumRewards.Text = "&nbsp;" + lcMySqlReader.GetInt32(lnRewardIndex).ToString() + "&nbsp;&nbsp;&nbsp;&nbsp;";

                    if (lnRewardIndex % 2 == 0)
                    {
                        if (lnRewardIndex == 0)
                        {
                            Panel4.Controls.Add(new LiteralControl("<tr>"));
                        }
                        else
                        {
                            Panel4.Controls.Add(new LiteralControl("</tr>"));
                        }
                    }

                    Panel4.Controls.Add(new LiteralControl("<td>"));
                    Panel4.Controls.Add(lcImage);
                    Panel4.Controls.Add(new LiteralControl("</td><td>"));
                    Panel4.Controls.Add(lcRewardName);
                    Panel4.Controls.Add(new LiteralControl("</td><td>"));
                    Panel4.Controls.Add(lcNumRewards);
                    Panel4.Controls.Add(new LiteralControl("</td>"));
                }

                Panel4.Controls.Add(new LiteralControl("</tr></table></div>"));
                lcMySqlCommand.Close();
            }
            catch (Exception e)
            {
                Debug.WriteLine("SetRewardLabels: " + e.Message);
            }
        }

        private void SetMedalLabels(string apanSteamId)
        {
            try 
            {
                //Set medals labels
                tcMySqlCommand lcMySqlCommand = new tcMySqlCommand("SELECT bronze,silver,gold,platinum FROM kzs_medals WHERE player = (SELECT id FROM kzs_players WHERE auth = ?steamid);");
                lcMySqlCommand.mcMySqlCommand.Parameters.Add("?steamid", apanSteamId);
                MySqlDataReader lcMySqlReader = lcMySqlCommand.mcMySqlCommand.ExecuteReader();
                lcMySqlReader.Read();

                Panel4.Controls.Add(new LiteralControl("<br>"));
                tcLabel lcHeading = new tcLabel();
                lcHeading.Text = "Medals earned";
                lcHeading.ForeColor = teColors.eeRedText;
                lcHeading.Font.Bold = true;
                Panel4.Controls.Add(lcHeading);
                Panel4.Controls.Add(new LiteralControl("<br>"));

                for (int lnMedalIndex = 0; lnMedalIndex < (int)teMedals.eeMax; lnMedalIndex++)
                {
                    System.Web.UI.WebControls.Image lcImage = new System.Web.UI.WebControls.Image();
                    lcImage.ImageAlign = ImageAlign.TextTop;
                    lcImage.Height = lcImage.Width = 16;
                    lcImage.ToolTip = ((teMedals)lnMedalIndex).ToString().Substring(2);

                    switch ((teMedals)lnMedalIndex)
                    {
                        case teMedals.eeBronze:
                            lcImage.ImageUrl = "~/img/medals/bronze.png";
                            break;
                        case teMedals.eeSilver:
                            lcImage.ImageUrl = "~/img/medals/silver.png";
                            break;
                        case teMedals.eeGold:
                            lcImage.ImageUrl = "~/img/medals/gold.png";
                            break;
                        case teMedals.eePlatinum:
                            lcImage.ImageUrl = "~/img/medals/platinum.png";
                            break;
                    }

                    Label lcNumMedals = new Label();
                    lcNumMedals.Text = "&nbsp;" + lcMySqlReader.GetInt32(lnMedalIndex).ToString() + "&nbsp;&nbsp;&nbsp;&nbsp;";
                    lcNumMedals.Font.Size = 12;
                    lcNumMedals.Font.Name = "Arial";
                    lcNumMedals.ForeColor = teColors.eeText;
                    
                    Panel4.Controls.Add(lcImage);
                    Panel4.Controls.Add(lcNumMedals);
                }

                lcMySqlCommand.Close();
            }
            catch (Exception e)
            {
                Debug.WriteLine("SetMedalLabels: " + e.Message);
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

        private string GetQueryFromDropDownState(string apanSteamId)
        {
            switch (gcDropDown.SelectedValue)
            {
                case "cp":
                    return @"SELECT FIN.Map AS Map, FIN.Course AS Course, FIN.CourseID AS CourseID, FIN.FTime AS Time, 
                            FIN.Cp AS Cp,FIN.Teles AS Teles,
                            FIN.pcount AS pcount, FIN.rank AS rank, FIN.Server AS Server FROM
                            
                            (SELECT M.name AS Map, C.name AS Course, C.id AS CourseID,

                            CASE 1 WHEN C.invert THEN Rtimes.maxtime ELSE Rtimes.mintime END AS FTime,
                            CASE 1 WHEN C.invert THEN Rmax.cp ELSE Rmin.cp END AS Cp,
                            CASE 1 WHEN C.invert THEN Rmax.tele ELSE Rmin.tele END AS Teles,
                            pcount,
                            rank,
                            CASE 1 WHEN C.invert THEN Smax.name ELSE Smin.name END AS Server FROM

                            (SELECT id FROM kzs_players WHERE auth = '" + apanSteamId + @"') AS P

                            LEFT JOIN (SELECT course, id,min(time) AS mintime, max(time) AS maxtime, player FROM kzs_recordscp GROUP BY player,course,id) AS Rtimes
                            ON Rtimes.player = P.id

                            LEFT JOIN (SELECT id,time,player FROM kzs_recordscp) AS Rminid
                            ON Rminid.time = Rtimes.mintime AND Rminid.player = Rtimes.player

                            LEFT JOIN (SELECT id,time,player FROM kzs_recordscp) AS Rmaxid
                            ON Rmaxid.time = Rtimes.maxtime AND Rmaxid.player = Rtimes.player

                            LEFT JOIN (SELECT id,tele,cp,map,time,server,player FROM kzs_recordscp) AS Rmin
                            ON Rmin.time = Rtimes.mintime AND Rmin.player = P.id AND Rmin.id = Rminid.id

                            LEFT JOIN (SELECT id,tele,cp,map,time,server,player FROM kzs_recordscp) AS Rmax
                            ON Rmax.time = Rtimes.maxtime AND Rmax.player = P.id AND Rmax.id = Rmaxid.id

                            LEFT JOIN (SELECT id,name FROM kzs_maps) AS M
                            ON M.id = Rmin.map

                            LEFT JOIN (SELECT id,name,invert FROM kzs_courses) AS C
                            ON C.id = Rtimes.course

                            LEFT JOIN (SELECT id,name FROM kzs_servers) AS Smin
                            ON Smin.id = Rmin.server

                            LEFT JOIN (SELECT id,name FROM kzs_servers) AS Smax
                            ON Smax.id = Rmax.server
                            
                            LEFT JOIN (SELECT course,COUNT(DISTINCT(player)) AS pcount FROM kzs_recordscp GROUP BY course) AS TOT
                            ON TOT.course = Rtimes.course
                            
                            LEFT JOIN(SELECT ( CASE course WHEN @CurCourse THEN @CurRow := @curRow + 1 ELSE @CurRow := 1 AND @CurCourse := course END)  AS rank,
                                                                            F.* FROM
                                                    (SELECT DISTINCT course,player FROM
                                                    (SELECT course,player,time AS Rtime FROM kzs_recordscp ORDER BY course, Rtime ASC) AS SUBR) AS F, 
                                                    (SELECT @CurRow := 0, @CurCourse := '') AS RNK GROUP BY player,course) AS RNKS
                            ON RNKS.player = P.id AND RNKS.course = C.id

                            ORDER BY Map,Course,CASE 1 WHEN C.invert THEN FTime ELSE -FTime END  DESC) AS FIN GROUP BY FIN.CourseID ORDER BY Map,Course";

                case "wr":
                    return @"SELECT  BP.Map, BP.Course, BP.CourseId, BP.Time,BP.Date,

                            CASE 1 WHEN BP.ThisPlayer = CURWRS.PlayerId AND BP.Time = CURWRS.Time THEN 'Current' ELSE 'Beaten' END AS Status

                            FROM

                            (SELECT F.* FROM

                            (SELECT C.invert,WR.player AS ThisPlayer,M.name AS Map,C.name AS Course,C.id AS CourseId,WR.time AS Time,DATE_FORMAT(WR.date,'%Y-%m-%d') AS Date FROM

                            (SELECT player,map,course,time,date FROM kzs_wrs WHERE player = (SELECT id FROM kzs_players WHERE auth = '" + apanSteamId + @"' LIMIT 1)) AS WR

                            LEFT JOIN (SELECT name,id FROM kzs_maps) AS M
                            ON M.id = WR.map

                            LEFT JOIN (SELECT name,id,invert FROM kzs_courses) AS C
                            ON C.id = WR.course

                            ORDER BY Date DESC, CASE 1 WHEN C.invert THEN Time ELSE -Time END  DESC) AS F

                            ORDER BY F.Map ASC, F.Course ASC,Date DESC, CASE 1 WHEN F.invert THEN Time ELSE -Time END  DESC) AS BP

                            LEFT JOIN (
                                                            SELECT Time,PlayerId,F.Player,F.CourseId FROM

                                                            (SELECT P.id AS PlayerId,M.name AS Map, C.name AS Course, C.id AS CourseId, P.id AS CurHolder, P.name AS Player, P.auth AS SteamId, P.country AS Country, WR.time AS Time, DATE_FORMAT(WR.date,'%Y-%m-%d') AS Date FROM

                                                            (SELECT player,map,course,time,date FROM kzs_wrs) AS WR

                                                            LEFT JOIN (SELECT name,id,auth,country FROM kzs_players) AS P
                                                            ON P.id = WR.player

                                                            LEFT JOIN (SELECT name,id FROM kzs_maps) AS M
                                                            ON M.id = WR.map

                                                            LEFT JOIN (SELECT name,id,invert FROM kzs_courses) AS C
                                                            ON C.id = WR.course

                                                             ORDER BY Map,Course,Date DESC, CASE 1 WHEN C.invert THEN Time ELSE -Time END DESC) AS F
                                 
                                                             GROUP BY Map,Course ORDER BY map,course) AS CURWRS
                            ON CURWRS.CourseId = BP.CourseId

                            ORDER BY Status DESC, map,course";

                case "unfincp":
                    return @"SELECT F.Map AS Map, F.Course AS Course, F.CourseID FROM

                            (SELECT M.name AS Map, C.name AS Course, C.id AS CourseID, IFNULL(R.count,0) AS Count FROM 

                            (SELECT id,map,name FROM kzs_courses) AS C

                            LEFT JOIN (SELECT name,id FROM kzs_maps) AS M
                            ON M.id = C.map

                            LEFT JOIN (SELECT COUNT(*) AS count,course FROM kzs_recordscp WHERE player = (SELECT id FROM kzs_players WHERE auth = '" + apanSteamId + @"' LIMIT 1) GROUP BY course) AS R
                            ON R.course = C.id) AS F

                            WHERE F.count = 0 ORDER BY Map, Course ASC";

                case "unfinnocheck":
                    return @"SELECT F.Map AS Map, F.Course AS Course, F.CourseID FROM

                            (SELECT M.name AS Map, C.name AS Course, C.id AS CourseID, IFNULL(R.count,0) AS Count FROM 

                            (SELECT id,map,name FROM kzs_courses) AS C

                            LEFT JOIN (SELECT name,id FROM kzs_maps) AS M
                            ON M.id = C.map

                            LEFT JOIN (SELECT COUNT(*) AS count,course FROM kzs_records WHERE player = (SELECT id FROM kzs_players WHERE auth = '" + apanSteamId + @"' LIMIT 1) GROUP BY course) AS R
                            ON R.course = C.id) AS F

                            WHERE F.count = 0 ORDER BY Map, Course ASC";

                case "nocheck":
                    return @"SELECT FIN.Map AS Map, FIN.Course AS Course, FIN.CourseID AS CourseID, FIN.FTime AS Time, FIN.pcount AS pcount, FIN.rank AS rank, FIN.Server AS Server FROM
                            (SELECT M.name AS Map, C.name AS Course, C.id AS CourseID,

                            CASE 1 WHEN C.invert THEN Rtimes.maxtime ELSE Rtimes.mintime END AS FTime,
                            pcount,
                            rank,
                            CASE 1 WHEN C.invert THEN Smax.name ELSE Smin.name END AS Server FROM

                            (SELECT id FROM kzs_players WHERE auth = '" + apanSteamId + @"') AS P

                            LEFT JOIN (SELECT id,course, min(time) AS mintime, max(time) AS maxtime, player FROM kzs_records GROUP BY player,course,id) AS Rtimes
                            ON Rtimes.player = P.id

                            LEFT JOIN (SELECT id,map,time,server,player FROM kzs_records) AS Rmin
                            ON Rmin.time = Rtimes.mintime AND Rmin.player = P.id AND Rtimes.id = Rmin.id

                            LEFT JOIN (SELECT id,map,time,server,player FROM kzs_records) AS Rmax
                            ON Rmax.time = Rtimes.maxtime AND Rmax.player = P.id AND Rtimes.id = Rmax.id

                            LEFT JOIN (SELECT id,name FROM kzs_maps) AS M
                            ON M.id = Rmin.map

                            LEFT JOIN (SELECT id,name,invert FROM kzs_courses) AS C
                            ON C.id = Rtimes.course

                            LEFT JOIN (SELECT id,name FROM kzs_servers) AS Smin
                            ON Smin.id = Rmin.server

                            LEFT JOIN (SELECT id,name FROM kzs_servers) AS Smax
                            ON Smax.id = Rmax.server
                            
                            LEFT JOIN (SELECT course,COUNT(DISTINCT(player)) AS pcount FROM kzs_records GROUP BY course) AS TOT
                            ON TOT.course = Rtimes.course
                            
                            LEFT JOIN(SELECT ( CASE course WHEN @CurCourse THEN @CurRow := @curRow + 1 ELSE @CurRow := 1 AND @CurCourse := course END)  AS rank,
                                                                            F.* FROM
                                                    (SELECT DISTINCT course,player FROM
                                                    (SELECT course,player,time AS Rtime FROM kzs_records ORDER BY course, Rtime ASC) AS SUBR) AS F, 
                                                    (SELECT @CurRow := 0, @CurCourse := '') AS RNK GROUP BY player,course) AS RNKS
                            ON RNKS.player = P.id AND RNKS.course = C.id
                            
                            ORDER BY Map,Course,CASE 1 WHEN C.invert THEN FTime ELSE -FTime END  DESC) AS FIN GROUP BY FIN.CourseID ORDER BY Map,Course";
                    
                default:
                    return null;
            }
        }
    }
}
