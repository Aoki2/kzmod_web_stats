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
using System.Globalization;

namespace classes
{
    //Can't use ViewState in dynamic pages with grid views, this solution will have to work for now
    static public class tcSortDir
    {
        static string mpanDir = "ASC";

        static public string GetDir()
        {
            mpanDir = mpanDir == "DESC" ? "ASC" : "DESC";

            return mpanDir;
        }
    }

    //Grid view with event handlers for sorting, paging and hyperlink URL encoding
    public class tcGridView : GridView
    {
        int mnCheckpoints = 0;
        private tcGridViewType mcType;

        private int mnMapListPrevMapIdx = 0;

        public tcGridView()
        {
            this.Sorting += new GridViewSortEventHandler(this.mpfSorting);
            this.RowDataBound += new GridViewRowEventHandler(this.mpfRowDataBound);
            this.PageIndexChanging += new GridViewPageEventHandler(this.mpfPageIndexChanging);

            mcType = new tcGridViewType(teGridViewType.eeNone, teSubPageType.eeSubPageNone);
        }

        public tcGridView(tcGridViewType acType)
        {
            this.Sorting += new GridViewSortEventHandler(this.mpfSorting);
            this.RowDataBound += new GridViewRowEventHandler(this.mpfRowDataBound);
            this.PageIndexChanging += new GridViewPageEventHandler(this.mpfPageIndexChanging);

            mcType = acType;
        }

        protected void mpfPageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            if (sender != null)
            {
                this.PageIndex = e.NewPageIndex;
                this.DataBind();
            }
        }

        protected void mpfSorting(object sender, GridViewSortEventArgs e)
        {
            string lpanSortDir;
            SqlDataSource lcSqlDataSource = this.DataSource as SqlDataSource;
            DataView lcDataView = ((DataTable)((GridView)sender).DataSource).DefaultView;

            if (lcDataView != null)
            {
                lpanSortDir = GetSortDirection(e.SortExpression.ToString());
                
                lcDataView.Sort = e.SortExpression + " " + lpanSortDir;

                this.DataSource = lcDataView;
                this.DataBind();
            }
        }
        
        private string GetSortDirection(string apanSortExpression)
        {
            return tcSortDir.GetDir();
        }

        private TableCell LimitStringLength(TableCell acTableCell, string apanHeaderText)
        {
            if (apanHeaderText == "Server" && acTableCell.Text.Length > 48)
            {
                acTableCell.ToolTip = acTableCell.Text;
                acTableCell.Text = acTableCell.Text.Substring(0, 47);
            }

            return acTableCell;
        }

        protected void mpfRowDataBound(object acSender, GridViewRowEventArgs e)
        {
            TableCell lcTableCell;
            DataControlField lcField;

            if (mnCheckpoints < 0 || mnCheckpoints > 1)
            {
                mnCheckpoints = 0;
            }

            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                for (int i = 0; i < this.Columns.Count; i++)
                {
                    lcField = this.Columns[i];

                    if (lcField is BoundField)
                    {
                        lcTableCell = e.Row.Cells[i];

                        lcTableCell = LimitStringLength(lcTableCell, lcField.HeaderText);
                    }

                    //If this field is a hyperlink, encode the URL
                    if (lcField is HyperLinkField)
                    {
                        lcTableCell = e.Row.Cells[i];

                        if (lcTableCell.Controls.Count > 0 && lcTableCell.Controls[0] is HyperLink)
                        {
                            HyperLink lcHyperLink = (HyperLink)lcTableCell.Controls[0];

                            HyperLinkField lcHyperLinkField = (HyperLinkField)lcField;

                            if (!String.IsNullOrEmpty(lcHyperLinkField.DataNavigateUrlFormatString))
                            {
                                string[] aapanDataUrlFields = new string[lcHyperLinkField.DataNavigateUrlFields.Length];

                                for (int j = 0; j < aapanDataUrlFields.Length; j++)
                                {
                                    object lcObj = DataBinder.Eval(e.Row.DataItem, lcHyperLinkField.DataNavigateUrlFields[j]);
                                    aapanDataUrlFields[j] = HttpUtility.UrlEncode(lcObj == null ? "" : lcObj.ToString());
                                }

                                lcHyperLink.NavigateUrl = String.Format(lcHyperLinkField.DataNavigateUrlFormatString, aapanDataUrlFields) + "&cp=" + mnCheckpoints;
                                lcHyperLink.ForeColor = teColors.eeLink;
                                lcHyperLink.Font.Bold = true;
                                lcHyperLink.Font.Underline = false;
                            }
                        }

                        //If map list page and expanding courses
                        if (mcType.meSub == teSubPageType.eeMapsListCourses && lcField is HyperLinkField && lcField.HeaderText == "Map")
                        {
                            int lnCurRow = e.Row.RowIndex;

                            if (lnCurRow > 0 && this.Rows.Count > mnMapListPrevMapIdx)
                            {
                                TableCell lcPrevCell = this.Rows[mnMapListPrevMapIdx].Cells[i];

                                if (lcTableCell.Controls.Count > 0 && lcTableCell.Controls[0] is HyperLink &&
                                    lcPrevCell.Controls.Count > 0 && lcPrevCell.Controls[0] is HyperLink)
                                {
                                    HyperLink lcHyperLink = (HyperLink)lcTableCell.Controls[0];
                                    HyperLink lcPrevHyperLink = (HyperLink)lcPrevCell.Controls[0];

                                    if (lcPrevHyperLink.NavigateUrl.Contains(lcHyperLink.Text) == true)
                                    {
                                        e.Row.Cells[0].Text = "";
                                    }
                                    else
                                    {
                                        mnMapListPrevMapIdx = e.Row.RowIndex;
                                    }
                                }
                            }
                        }
                    }
                    //Else if this field is time, format it nicely
                    else if (lcField is BoundField && lcField.HeaderText == "Time")
                    {
                        try
                        {
                            lcTableCell = e.Row.Cells[i];

                            lcTableCell.Width = 64;


                            lcTableCell.Text = StaticMethods.TimeToString(float.Parse(lcTableCell.Text));
                        }
                        catch(Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                    }
                    //Else if this is an average tag stat that should be formatted
                    else if (lcField is BoundField && mcType.meSub == teSubPageType.eePlayerListTagStats && lcField.HeaderText.Contains("Avg"))
                    {
                        try
                        {
                            lcTableCell = e.Row.Cells[i];

                            lcTableCell.Width = 64;


                            lcTableCell.Text = String.Format("{0:0.0}", float.Parse(lcTableCell.Text));
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                    }
                    //Else if this is untagged time
                    else if (lcField is BoundField && mcType.meSub == teSubPageType.eePlayerListTagStats && lcField.HeaderText.Contains("Most untagged"))
                    {
                        try
                        {
                            lcTableCell = e.Row.Cells[i];

                            lcTableCell.Width = 64;


                            lcTableCell.Text = StaticMethods.TimeToString(float.Parse(lcTableCell.Text), false);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                    }
                    //Else if player page WR listing, color the status field to indicate if the WR is current or has been beaten
                    else if (lcField is BoundField && lcField.HeaderText == "Status")
                    {
                        try
                        {
                            lcTableCell = e.Row.Cells[i];

                            if (lcTableCell.Text == "Beaten")
                            {
                                lcTableCell.ForeColor = teColors.eeRedText;
                            }
                            else if (lcTableCell.Text == "Current")
                            {
                                lcTableCell.ForeColor = teColors.eeGreenText;
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                    }
                }
            }
        }
    }
}
