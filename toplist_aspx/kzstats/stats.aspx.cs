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
    public partial class stats : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            Panel4.EnableViewState = false;

            if (HttpContext.Current.Request.UserAgent.Contains("Chrome") == true || HttpContext.Current.Request.UserAgent.Contains("Firefox") == true)
            {
                gcBody.Attributes.Add("style", tcBgStyler.Instance.GetBgImageStyle());
            }
            DoPageLoad();
        }

        private void DoPageLoad()
        {
            Stopwatch lcStopWatch = new Stopwatch(); lcStopWatch.Reset(); lcStopWatch.Start();

            int lnTotalPlayers = 0;
            int lnTotalCountries = 0;
            int lnFinCourseNocheck = 0;
            int lnFindCourseCheck = 0;
            int lnNumMaps = 0;
            int lnNumMapRuns = 0;
            int lnNumCourses = 0;
            int lnUniqueCpRecord = 0;
            int lnUniqueNocheckRecord = 0;
            int lnNumChecks = 0;
            int lnNumTeles = 0;

            int lnTags = 0;
            int lnNinjaTags = 0;
            int lnTagged = 0;
            int lnRoundsPlayed = 0;
            int lnRoundWins = 0;
            int lnTotalUntaggedTimeSec = 0;
            
            GetPlayersAndCountries(out lnTotalPlayers, out lnTotalCountries);
            GetCourseFinishes(out lnFinCourseNocheck, out lnFindCourseCheck);
            GetNumMapsAndRuns(out lnNumMaps, out lnNumMapRuns);
            GetNumCourses(out lnNumCourses);
            GetChecksAndTeles(out lnUniqueCpRecord, out lnNumChecks, out lnNumTeles);
            GetUniqueNocheckCount(out lnUniqueNocheckRecord);
            GetTagStatRollup(out lnTags, out lnNinjaTags, out lnTagged, out lnRoundsPlayed, out lnRoundWins, out lnTotalUntaggedTimeSec);

            Panel4.Controls.Add(new LiteralControl("<div style=\"text-align: center;\">"));
            AddPanelLabel("General",true);
            Panel4.Controls.Add(new LiteralControl("</div>"));
            Panel4.Controls.Add(new LiteralControl("<hr>"));
            AddPanelLabel(lnTotalPlayers + " unique players from " + lnTotalCountries + " countries.<br>");
            AddPanelLabel(lnNumMaps + " maps with " + lnNumCourses + " courses.<br>");

            Panel4.Controls.Add(new LiteralControl("<div style=\"text-align: center;\">"));
            AddPanelLabel("<br>Runs", true);
            Panel4.Controls.Add(new LiteralControl("</div>"));
            Panel4.Controls.Add(new LiteralControl("<hr>"));
            AddPanelLabel(lnUniqueNocheckRecord + " unique nocheck records over " + lnFinCourseNocheck + " total runs.<br>");
            AddPanelLabel(lnUniqueCpRecord + " unique checkpoint records over " + lnFindCourseCheck + " total runs.<br>");
            AddPanelLabel("Checkpoint runs contain " + lnNumChecks + " total checkpoints and " + lnNumTeles + " teleports.<br>");

            Panel4.Controls.Add(new LiteralControl("<div style=\"text-align: center;\">"));
            AddPanelLabel("<br>Tag", true);
            Panel4.Controls.Add(new LiteralControl("</div>"));
            Panel4.Controls.Add(new LiteralControl("<hr>"));
            AddPanelLabel(lnRoundWins + " rounds of tag played.<br>");
            AddPanelLabel(lnTags + " total tags ("+ lnNinjaTags+" ninja tags).<br>");
            TimeSpan lcTimeSpan = TimeSpan.FromMinutes(lnRoundsPlayed * 10);
            AddPanelLabel("Total time spent playing tag: " + lcTimeSpan.Days + " days and " + lcTimeSpan.Hours + " hours<br>");

            Panel4.Controls.Add(new LiteralControl("<div style=\"text-align: center;\">"));
            AddMedals();
            AddRewards();
            Panel4.Controls.Add(new LiteralControl("</div>"));

            tcLinkPanel.AddLinkPanel(this);

            //Refresh all the gridviews with data contents
            Page.DataBind();
            Page.MaintainScrollPositionOnPostBack = true;

            lcStopWatch.Stop(); gcPageLoad.Text = lcStopWatch.ElapsedMilliseconds.ToString();
        }

        private void GetTagStatRollup(out int anTags, out int anNinjaTags, out int anTagged, out int anRoundsPlayed, out int anRoundWins, out int anTotalUntaggedTimeSec)
        {
            anTags = 0; anNinjaTags = 0; anTagged = 0; anRoundsPlayed = 0; anRoundWins = 0; anTotalUntaggedTimeSec = 0;
            tcMySqlCommand lcMySqlCommand = new tcMySqlCommand(@"SELECT SUM(tags), SUM(ninja), SUM(tagged), SUM(rounds_played), SUM(round_wins), SUM(total_untagged_time) FROM kzs_tag");
            MySqlDataReader lcMySqlReader = lcMySqlCommand.mcMySqlCommand.ExecuteReader();
            lcMySqlReader.Read();

            if (lcMySqlReader.HasRows)
            {
                anTags = lcMySqlReader.GetInt32(0);
                anNinjaTags = lcMySqlReader.GetInt32(1);
                anTagged = lcMySqlReader.GetInt32(2);
                anRoundsPlayed = lcMySqlReader.GetInt32(3);
                anRoundWins = lcMySqlReader.GetInt32(4);
                anTotalUntaggedTimeSec = lcMySqlReader.GetInt32(5);
            }

            lcMySqlCommand.Close();
        }

        private void AddPanelLabel(String apanText, bool aeBold = false)
        {
            Label lcLabel = new Label();
            lcLabel.Text = apanText;
            lcLabel.Font.Size = 12;
            lcLabel.Font.Name = "Arial";
            lcLabel.ForeColor = teColors.eeText;
            lcLabel.Font.Bold = aeBold;
            Panel4.Controls.Add(lcLabel);
        }

        private void AddMedals()
        {
            int lnBronzeCount = 0;
            int lnSilverCount = 0;
            int lnGoldCount = 0;
            int lnPlatCount = 0;
            int lnCurCount = 0;
            GetMedalCounts(out lnBronzeCount, out lnSilverCount, out lnGoldCount, out lnPlatCount);

            AddPanelLabel("<br>Total medals earned", true);
            Panel4.Controls.Add(new LiteralControl("<hr>"));

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
                        lnCurCount = lnBronzeCount;
                        break;
                    case teMedals.eeSilver:
                        lcImage.ImageUrl = "~/img/medals/silver.png";
                        lnCurCount = lnSilverCount;
                        break;
                    case teMedals.eeGold:
                        lcImage.ImageUrl = "~/img/medals/gold.png";
                        lnCurCount = lnGoldCount;
                        break;
                    case teMedals.eePlatinum:
                        lcImage.ImageUrl = "~/img/medals/platinum.png";
                        lnCurCount = lnPlatCount;
                        break;
                }
                
                Panel4.Controls.Add(lcImage);
                AddPanelLabel("&nbsp;" + lnCurCount + "&nbsp;&nbsp;&nbsp;&nbsp;");
            }
        }

        private void AddRewards()
        {
            int lnCount = 0;
            int lnTimebomb = 0; int lnPainkillers = 0; int lnSoccerball = 0; int lnStatsReadout = 0; int lnMoonboots = 0; int lnStrangeMojo = 0; int lnSlomo = 0; int lnBurningFeet = 0; int lnCloack = 0; int lnFastmo = 0; int lnTitle = 0; int lnAntiGrav = 0; int lnBootsOfHeight = 0;
            GetRewards(out lnTimebomb, out lnPainkillers, out lnSoccerball, out lnStatsReadout, out lnMoonboots, out lnStrangeMojo, out lnSlomo, out lnBurningFeet, out lnCloack, out lnFastmo, out lnTitle, out lnAntiGrav, out lnBootsOfHeight);

            AddPanelLabel("<br><br>Total rewards purchased", true);
            Panel4.Controls.Add(new LiteralControl("<hr>"));
            Panel4.Controls.Add(new LiteralControl("<div style=\"line-height:56px;\"><table align=\"center\">"));

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
                        lnCount = lnTimebomb;
                        break;
                    case teRewards.eePainKillers:
                        lcImage.ImageUrl = "~/img/rewards/painkillers.png";
                        lcRewardName.Text = "Painkillers";
                        lnCount = lnPainkillers;
                        break;
                    case teRewards.eeSoccerBall:
                        lcImage.ImageUrl = "~/img/rewards/soccerball.png";
                        lcRewardName.Text = "Soccer ball";
                        lnCount = lnSoccerball;
                        break;
                    case teRewards.eeStatsReadout:
                        lcImage.ImageUrl = "~/img/rewards/stats_readout.png";
                        lcRewardName.Text = "Stats readout";
                        lnCount = lnStatsReadout;
                        break;
                    case teRewards.eeMoonBoots:
                        lcImage.ImageUrl = "~/img/rewards/moonboots.png";
                        lcRewardName.Text = "Moon boots";
                        lnCount = lnMoonboots;
                        break;
                    case teRewards.eeStrangeMojo:
                        lcImage.ImageUrl = "~/img/rewards/strange_mojo.png";
                        lcRewardName.Text = "Strange mojo";
                        lnCount = lnStrangeMojo;
                        break;
                    case teRewards.eeSlowmo:
                        lcImage.ImageUrl = "~/img/rewards/slomo.png";
                        lcRewardName.Text = "Slow-mo";
                        lnCount = lnSlomo;
                        break;
                    case teRewards.eeBurningFeet:
                        lcImage.ImageUrl = "~/img/rewards/burningfeet.png";
                        lcRewardName.Text = "Burning feet";
                        lnCount = lnBurningFeet;
                        break;
                    case teRewards.eeCloak:
                        lcImage.ImageUrl = "~/img/rewards/cloak.png";
                        lcRewardName.Text = "Cloak";
                        lnCount = lnCloack;
                        break;
                    case teRewards.eeFastmo:
                        lcImage.ImageUrl = "~/img/rewards/boots_of_speed.png";
                        lcRewardName.Text = "Boots of speed";
                        lnCount = lnFastmo;
                        break;
                    case teRewards.eeCustomTitle:
                        lcImage.ImageUrl = "~/img/rewards/custom_message.png";
                        lcRewardName.Text = "Custom title";
                        lnCount = lnTitle;
                        break;
                    case teRewards.eePsychicAntiGravity:
                        lcImage.ImageUrl = "~/img/rewards/psychic_anti_gravity.png";
                        lcRewardName.Text = "Psychic anti-gravity";
                        lnCount = lnAntiGrav;
                        break;
                    case teRewards.eeBootsOfHeight:
                        lcImage.ImageUrl = "~/img/rewards/boots_of_height.png";
                        lcRewardName.Text = "Boots of height";
                        lnCount = lnBootsOfHeight;
                        break;
                }

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
                AddPanelLabel("&nbsp;" + lnCount + "&nbsp;&nbsp;&nbsp;&nbsp;");
                Panel4.Controls.Add(new LiteralControl("</td>"));
            }

            Panel4.Controls.Add(new LiteralControl("</tr></table></div>"));
        }

        private void GetRewards(out int anTimebomb, out int anPainkillers, out int anSoccerball, out int anStatsReadout, out int anMoonboots, out int anStrangeMojo, out int anSlomo, out int anBurningFeet, out int anCloack, out int anFastmo, out int anTitle, out int anAntiGrav, out int anBootsOfHeight)
        {
            anTimebomb = 0; anPainkillers = 0; anSoccerball = 0; anStatsReadout = 0; anMoonboots = 0; anStrangeMojo = 0; anSlomo = 0; anBurningFeet = 0; anCloack = 0; anFastmo = 0; anTitle = 0; anAntiGrav = 0; anBootsOfHeight = 0;

            tcMySqlCommand lcMySqlCommand = new tcMySqlCommand(@"SELECT SUM(timebomb) AS Timebomb, SUM(painkillers) AS Painkillers, SUM(soccerball) AS Soccerball, SUM(stats_readout) AS StatsReadout, SUM(moonboots) AS Moonboots,  SUM(strange_mojo) AS StrangeMojo, SUM(slowmo) AS Slomo,  SUM(burningfeet) AS Burningfeet, SUM(cloak) AS Cloak, SUM(fastmo) AS Fastmo, SUM(custom_title) AS Title, SUM(psychic_anti_gravity) AS AntiGrav, SUM(boots_of_height) AS BootsOfHeight FROM kzs_medals");
            MySqlDataReader lcMySqlReader = lcMySqlCommand.mcMySqlCommand.ExecuteReader();
            lcMySqlReader.Read();

            if (lcMySqlReader.HasRows)
            {
                anTimebomb      = lcMySqlReader.GetInt32(0);
                anPainkillers   = lcMySqlReader.GetInt32(1); 
                anSoccerball    = lcMySqlReader.GetInt32(2);
                anStatsReadout  = lcMySqlReader.GetInt32(3); 
                anMoonboots     = lcMySqlReader.GetInt32(4);
                anStrangeMojo   = lcMySqlReader.GetInt32(5);
                anSlomo         = lcMySqlReader.GetInt32(6);
                anBurningFeet   = lcMySqlReader.GetInt32(7);
                anCloack        = lcMySqlReader.GetInt32(8);
                anFastmo        = lcMySqlReader.GetInt32(9);
                anTitle         = lcMySqlReader.GetInt32(10);
                anAntiGrav      = lcMySqlReader.GetInt32(11);
                anBootsOfHeight = lcMySqlReader.GetInt32(12); 
            }

            lcMySqlCommand.Close();
        }

        private void GetMedalCounts(out int anBronzeCount, out int anSilverCount, out int anGoldCount, out int anPlatCount)
        {
            anBronzeCount = 0;
            anSilverCount = 0;
            anGoldCount = 0;
            anPlatCount = 0;

            tcMySqlCommand lcMySqlCommand = new tcMySqlCommand(@"SELECT SUM(bronze) AS Bronze, SUM(silver) AS Silver, SUM(gold) AS Gold, SUM(platinum) AS Plat FROM kzs_medals");
            MySqlDataReader lcMySqlReader = lcMySqlCommand.mcMySqlCommand.ExecuteReader();
            lcMySqlReader.Read();

            if (lcMySqlReader.HasRows)
            {
                anBronzeCount = lcMySqlReader.GetInt32(0);
                anSilverCount = lcMySqlReader.GetInt32(1);
                anGoldCount = lcMySqlReader.GetInt32(2);
                anPlatCount = lcMySqlReader.GetInt32(3);
            }

            lcMySqlCommand.Close();
        }

        private void GetUniqueNocheckCount(out int anUniqueNocheckRecord)
        {
            anUniqueNocheckRecord = 0;

            tcMySqlCommand lcMySqlCommand = new tcMySqlCommand(@"SELECT COUNT(DISTINCT(R.id)) FROM kzs_records AS R");
            MySqlDataReader lcMySqlReader = lcMySqlCommand.mcMySqlCommand.ExecuteReader();
            lcMySqlReader.Read();

            if (lcMySqlReader.HasRows)
            {
                anUniqueNocheckRecord = lcMySqlReader.GetInt32(0);
            }

            lcMySqlCommand.Close();
        }

        private void GetChecksAndTeles(out int anUniqueCpRecord, out int anNumChecks, out int anNumTeles)
        {
            anNumChecks = 0;
            anNumTeles = 0;
            anUniqueCpRecord = 0;

            tcMySqlCommand lcMySqlCommand = new tcMySqlCommand(@"SELECT COUNT(DISTINCT(Rcp.id)) AS Count, SUM(Rcp.cp) AS Cp, SUM(Rcp.tele) AS Tele FROM kzs_recordscp AS Rcp");
            MySqlDataReader lcMySqlReader = lcMySqlCommand.mcMySqlCommand.ExecuteReader();
            lcMySqlReader.Read();

            if (lcMySqlReader.HasRows)
            {
                anUniqueCpRecord = lcMySqlReader.GetInt32(0);
                anNumChecks = lcMySqlReader.GetInt32(1);
                anNumTeles = lcMySqlReader.GetInt32(2);
            }

            lcMySqlCommand.Close();
        }

        private void GetNumCourses(out int anNumCourses)
        {
            anNumCourses = 0;

            tcMySqlCommand lcMySqlCommand = new tcMySqlCommand(@"SELECT COUNT(*) AS NumCourses FROM kzs_courses");
            MySqlDataReader lcMySqlReader = lcMySqlCommand.mcMySqlCommand.ExecuteReader();
            lcMySqlReader.Read();

            if (lcMySqlReader.HasRows)
            {
                anNumCourses = lcMySqlReader.GetInt32(0);
            }

            lcMySqlCommand.Close();
        }

        private void GetNumMapsAndRuns(out int anNumMaps, out int anNumMapRuns)
        {
            anNumMaps = 0;
            anNumMapRuns = 0;

            tcMySqlCommand lcMySqlCommand = new tcMySqlCommand(@"SELECT COUNT(DISTINCT(M.id)) AS NumMaps, SUM(M.runs) AS SumMapRuns FROM kzs_maps AS M");
            MySqlDataReader lcMySqlReader = lcMySqlCommand.mcMySqlCommand.ExecuteReader();
            lcMySqlReader.Read();

            if (lcMySqlReader.HasRows)
            {
                anNumMaps = lcMySqlReader.GetInt32(0);
                anNumMapRuns = lcMySqlReader.GetInt32(1);
            }

            lcMySqlCommand.Close();
        }

        private void GetCourseFinishes(out int anFinCourseNocheck, out int anFindCourseCheck)
        {
            anFinCourseNocheck = 0;
            anFindCourseCheck = 0;

            tcMySqlCommand lcMySqlCommand = new tcMySqlCommand(@"SELECT SUM(C.fin) AS SumFin, SUM(C.fincp) AS SumFinCp FROM kzs_courses AS C");
            MySqlDataReader lcMySqlReader = lcMySqlCommand.mcMySqlCommand.ExecuteReader();
            lcMySqlReader.Read();

            if (lcMySqlReader.HasRows)
            {
                anFinCourseNocheck = lcMySqlReader.GetInt32(0);
                anFindCourseCheck = lcMySqlReader.GetInt32(1);
            }

            lcMySqlCommand.Close();
        }

        private void GetPlayersAndCountries(out int anTotalPlayers, out int anTotalCountries)
        {
            anTotalPlayers = 0;
            anTotalCountries = 0;

            tcMySqlCommand lcMySqlCommand = new tcMySqlCommand(@"SELECT COUNT(DISTINCT(P.id)) AS NumPlayers, COUNT(DISTINCT(P.country)) AS NumCountries FROM kzs_players AS P");
            MySqlDataReader lcMySqlReader = lcMySqlCommand.mcMySqlCommand.ExecuteReader();
            lcMySqlReader.Read();

            if (lcMySqlReader.HasRows)
            {
                anTotalPlayers = lcMySqlReader.GetInt32(0);
                anTotalCountries = lcMySqlReader.GetInt32(1);
            }

            lcMySqlCommand.Close();
        }
    }
}
