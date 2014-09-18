using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Drawing;
using System.Web;
using System.Net;
using System.IO;

//Class that sets random background image with CSS style
namespace classes
{
    public sealed class tcBgStyler
    {
        private static readonly tcBgStyler mcSelf = new tcBgStyler();
        private static string[] mpanImages;
        private static int mnIndex = 0;
        private static DateTime mcNextUpdate;

        public static tcBgStyler Instance
        {
            get
            {
                 return mcSelf;
            }
        }

        private tcBgStyler()
        {
            RefreshImageList();
        }

        private void RefreshImageList()
        {
            string lpanPath = HttpContext.Current.Request.MapPath("");

            mpanImages = Directory.GetFiles(lpanPath + "\\img\\bg-screen", "*.jpg");

            for (int i = 0; i < mpanImages.Length; i++)
            {
                mpanImages[i] = Path.GetFileName(mpanImages[i]);
            }

            mcNextUpdate = DateTime.UtcNow.AddSeconds(60*60);
        }

        public string GetBgImageStyle()
        {
            if (mpanImages.Length > 0)
            {
                if (DateTime.Compare(mcNextUpdate, DateTime.UtcNow) < 0)
                {
                    RefreshImageList();
                }

                mnIndex = new Random(DateTime.UtcNow.Millisecond).Next(0, mpanImages.Length);

                return "background-image:url('img/bg-screen/" + mpanImages[mnIndex] + "');background-repeat:no-repeat;background-attachment:fixed;background-position:center center;";
            }
            else
            {
                return "";
            }
        }
    }
}
