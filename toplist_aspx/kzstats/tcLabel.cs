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
    public class tcLabel : Label
    {
        public tcLabel()
        {
            base.ForeColor = teColors.eeText;
            base.Font.Name = "Arial";
            base.Font.Size = 11;
        }

        public tcLabel(Color aeColor, string apanFontName = "Arial", int anFontSize = 11, bool aeBold = false)
        {
            base.ForeColor = aeColor;
            base.Font.Name = apanFontName;
            base.Font.Size = anFontSize;
            base.Font.Bold = aeBold;
        }
    }
}
