using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Data;
using System.Diagnostics;
using System.Web;
using System.Web.UI;

namespace classes
{
    public class tcLinkPanel : Panel
    {
        public static void AddLinkPanel(Page acPage)
        {
            int lnIdx = 0;
            acPage.Header.Controls.AddAt(lnIdx++,new LiteralControl("<div align=\"center\">"));
            acPage.Header.Controls.AddAt(lnIdx++, new tcLinkPanel());
            acPage.Header.Controls.AddAt(lnIdx++, new LiteralControl("</div>"));
        }

        private tcLinkPanel()
        {
            int lnIdx = 0;

            this.Controls.AddAt(lnIdx++, new LiteralControl("<table align=\"center\" style=\"width:100%;height:100%\"> <tr> <td width=\"25%\" valign=\"middle\" align=\"center\"> "));
            this.Controls.AddAt(lnIdx++, CreateLink("KZmod.com", "http://www.kzmod.com"));

            this.Controls.AddAt(lnIdx++, new LiteralControl(" </td>  <td width=\"25%\" valign=\"middle\" align=\"center\">"));
            this.Controls.AddAt(lnIdx++, CreateLink("Map List", "~/maplist.aspx"));

            this.Controls.AddAt(lnIdx++, new LiteralControl(" </td>  <td width=\"25%\" valign=\"middle\" align=\"center\">"));
            this.Controls.AddAt(lnIdx++, CreateLink("Player Rankings", "~/playerranks.aspx"));

            this.Controls.AddAt(lnIdx++, new LiteralControl(" </td>  <td width=\"25%\" valign=\"middle\" align=\"center\">"));
            this.Controls.AddAt(lnIdx++, CreateLink("World Records", "~/wr.aspx"));

            this.Controls.AddAt(lnIdx++, new LiteralControl("</td> </tr> </table>"));

            this.Width = 760;
            this.Height = 32;
            this.BackImageUrl = "~/img/linkbar1.png";
            this.BackColor = Color.Transparent;
            this.HorizontalAlign = System.Web.UI.WebControls.HorizontalAlign.Center;
        }

        private HyperLink CreateLink(string apanText, string apanUrl)
        {
            HyperLink lcReturn = new HyperLink();

            lcReturn.Text = apanText;
            lcReturn.NavigateUrl = apanUrl;

            lcReturn.Font.Name = "Arial";
            lcReturn.Font.Size = 11;
            lcReturn.Font.Bold = true;
            lcReturn.ForeColor = teColors.eeLink;

            return lcReturn;
        }
    }
}
