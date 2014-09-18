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
using System.Security.Cryptography;
using System.Text;

namespace kzstats
{
    public partial class genkey : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Random lcRand = new Random();
            int lnRandVal = lcRand.Next();
            string lcString = lnRandVal.ToString("X") + lnRandVal.ToString("X");

            gcKeyLabel.Text = lcString + "-" + StaticMethods.CalculateMD5Hash(lcString).Substring(0, 8);

            if (HttpContext.Current.Request.UserAgent.Contains("Chrome") == true || HttpContext.Current.Request.UserAgent.Contains("Firefox") == true)
            {
                gcBody.Attributes.Add("style", tcBgStyler.Instance.GetBgImageStyle());
            }
        }
    }
}

