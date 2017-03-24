using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using System.Threading;
using xxx.Library.Data;
using xxx.Library.Data.SP;
using xxx.Web;
using xxx.Web.Caches;

public partial class OnlineReport : BasePage
{
    List<string> DisplaySiteNameList = new List<string>();
    protected void Page_Load(object sender, EventArgs e)
    {
        string eventDate = string.IsNullOrEmpty(Request["eventDate"]) ? System.DateTime.Now.ToString("MM/dd/yyyy") : Request["eventDate"].ToString();
        bool Excel = string.IsNullOrEmpty(Request["Excel"]) ? false : Convert.ToBoolean(Request["Excel"]);

        #region Load Template  -------------------
        PageParser.SetTemplateDir("../template/");
        if (Excel)
        {
            PageParser.SetTemplateFile("MonthlyReport_LogExcel.htm");
            PageParser.UpdateBlock("ReportLog");
            Response.ContentType = "application/vnd.ms-excel";
            Response.AddHeader("Content-Disposition", "attachment; filename=OnlineReport_" + eventDate + ".xls");
        }
        else
        {
            PageParser.SetTemplateFile("OnlineReport.html");
        }
        #endregion -------------------------------

        PageParser.SetVariable("eventDate", eventDate);
 
        _OutputData(eventDate, "Date");
        _OutputData(eventDate, "Year");

        if (Excel)
            PageParser.ParseBlock("ReportLog");
    }

    private void _OutputData(string eventDate, string Type)
    {
        #region getData ---
        DataTable dt = new DataTable();
        DataSet ds = new DataSet();
        if (Type == "Year")
        {
            string queryYear = string.Format("01/01/{0}", Convert.ToDateTime(eventDate).Year.ToString());
            try
            {
                string execSPName = "";
                string CacheKey = string.Format("CacheKey_{0}", queryYear);
                string _QueryString = String.Format("{0}  @Date='{1}',@Type='{2}'", execSPName, queryYear, "Y");
                ds = QueryCache.GetDataSetResult(
                            Common.RemoteQueryUrl,
                            CacheKey,
                            Common.ConnectionString,
                            _QueryString,
                            30,
                            86400000,
                            180);

                if (ds == null)
                {
                    dt = dt;
                }
                else
                {
                    if (ds.Tables.Count > 0)
                        dt = ds.Tables[0];
                }
            }
            catch
            {
                dt = dt;
            }
        }
        else
        {
            dt = dt;
        }
        #endregion -----------

        if (DisplaySiteNameList.Count == 0)
            DisplaySiteNameList = _getDisplaySiteNameList(dt); 

        if (Type == "Year")
            _parseYearData(eventDate, dt);
        else
            _parseSiteData(eventDate, dt);
    }

    private void _parseSiteData(string eventDate, DataTable dt)
    {
        try
        {
            StringBuilder sbSiteData = new StringBuilder();
            int MaxRowLenth = 0;
            if (dt.Rows.Count > 0)
                MaxRowLenth = dt.Rows.Count;

            _arrangeTitle(dt, DisplaySiteNameList, out sbSiteData);
                                            
            #region parse data ---
            //--- sequence of local time displays by TimeSequenceList ---//
            //--- sequence of table.[logdate] query by GTM -4 ---//
            StringBuilder sbTD = new StringBuilder();
            string _tmpTRBlock = "<tr id='TableData'>{0}</tr>";
            string _tmpTDBlock = "<td>{0}</td>";
            int[] TimeSequenceList = new int[] { 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
            PageParser.SetVariable("TimeSequenceListLength", TimeSequenceList.Length.ToString());

            for (int j = 0; j < TimeSequenceList.Length; j++)
            {
                sbTD = new StringBuilder();
                string _tmpColumn2 = "{0} ({1})/ {2}";
                string _tmpTotal = "{{Total}}";
                int Total = 0;
                DateTime TimeSequence = Convert.ToDateTime(eventDate + " " + TimeSequenceList[j] + ":00:00");
                string LocalTime = string.Format(" {0} - {1} ", TimeSequence.ToString("HH:mm"), TimeSequence.AddHours(1).ToString("HH:mm"));
                sbTD.AppendFormat(_tmpTDBlock, LocalTime);

                if (j < MaxRowLenth)
                {
                    DataRow[] drs = dt.Select("", "logdate asc"); //GTM +8
                    DataRow dr = drs[j];
                    string Column2 = string.Format(_tmpColumn2 dr["Today"].ToString(), dr["Today1"].ToString(), dr["Today2"].ToString());

                    sbTD.AppendFormat(_tmpTDBlock, _tmpTotal);
                    sbTD.AppendFormat(_tmpTDBlock, Function.formatnumber(Convert.ToDouble(dr["column1"]), 0));
                    sbTD.AppendFormat(_tmpTDBlock, Column2);

                    foreach (string SiteName in DisplaySiteNameList)
                    {
                        int SiteCnt = 0;
                        if (dr.Table.Columns.Contains(SiteName))
                            SiteCnt = Convert.ToInt32(dr[SiteName]);

                        Total += SiteCnt;
                        sbTD.AppendFormat(_tmpTDBlock, Function.formatnumber(Convert.ToDouble(SiteCnt), 0));
                    }

                    sbSiteData.AppendFormat(_tmpTRBlock, sbTD.ToString());
                    sbSiteData.Replace(_tmpTotal, Function.formatnumber(Convert.ToDouble(Total), 0));
                }
                else
                {
                    sbTD.AppendFormat(_tmpTDBlock, "0"); //Total
                    sbTD.AppendFormat(_tmpTDBlock, "0"); //Column1
                    sbTD.AppendFormat(_tmpTDBlock, string.Format(_tmpColumn2, "0", "0", "0"));

                    for (int i = 0; i < DisplaySiteNameList.Count; i++)
                    {
                        sbTD.AppendFormat(_tmpTDBlock, "0");
                    }

                    sbSiteData.AppendFormat(_tmpTRBlock, sbTD.ToString());
                }
            }
            #endregion -----------
            PageParser.SetVariable("SiteData", sbSiteData.ToString());
        }
        catch (Exception ex)
        {
            Logger.Log2File(ex.Message + ex.StackTrace);
            SendExceptionMail(ex);
        }
    }

    private void _parseYearData(string eventDate, DataTable dt)
    { 
        try
        {
            StringBuilder sbYearData = new StringBuilder();
            #region parse data ---
            //--- sequence of table.[logdate] query by GTM -4 ---//
            StringBuilder sbTD = new StringBuilder();
            string _tmpTRBlock = "<tr>{0}</tr>";
            string _tmpTDBlock = "<th>{0}</th>";

            for (int j = 0; j < 3; j++)
            {
                sbTD = new StringBuilder();
                string _tmpTotal = "{{Total}}";
                int Total = 0;
                int queryYear = (Convert.ToDateTime(eventDate).Year - 2) + j; //Year - 2: set first year of row
                if(j==0)//first yearly user
                {
                    sbTD.AppendFormat("<th {1} >{0}</th>", "Yearly Concurrent User", "rowspan='3' colspan='3'");
                }

                sbTD.AppendFormat(_tmpTDBlock, queryYear.ToString()); //Year

                DataRow[] drs = dt.Select(string.Format("logdate1={0}", queryYear));
                if (drs.Length > 0)
                {
                    DataRow dr = drs[0];
                    //sbTD.AppendFormat(_tmpTDBlock, Convert.ToDateTime(dr["logdate"].ToString()).Year.ToString());

                    foreach (string SiteName in DisplaySiteNameList)
                    {
                        int SiteCnt = 0;
                        if (dr.Table.Columns.Contains(SiteName))
                            SiteCnt = Convert.ToInt32(dr[SiteName]);
                        Total += SiteCnt;

                        sbTD.AppendFormat(_tmpTDBlock, Function.formatnumber(Convert.ToDouble(SiteCnt), 0));
                    }

                    sbYearData.AppendFormat(_tmpTRBlock, sbTD.ToString());

                    sbYearData.Replace(_tmpTotal, Function.formatnumber(Convert.ToDouble(Total), 0));
                }
                else
                {
                    for (int i = 0; i < DisplaySiteNameList.Count; i++)
                    {
                        sbTD.AppendFormat(_tmpTDBlock, "0");
                    }

                    sbYearData.AppendFormat(_tmpTRBlock, sbTD.ToString());
                }

            }
            #endregion -----------
            PageParser.SetVariable("YearData", sbYearData.ToString());
        }
        catch (Exception ex)
        {
            Logger.Log2File(ex.Message + ex.StackTrace);
            SendExceptionMail(ex);
        }
    
    }

    private List<string> _getDisplaySiteNameList(DataTable dt)
    {
        List<string> HideSiteName = new List<string>(new string[] { "AAA", "BBB", "CCC", "DDD", "EEE"});

        foreach (DataColumn column in dt.Columns)
        {
            string Site = column.ColumnName;
            if (!HideSiteName.Contains(Site.ToLower()))
                DisplaySiteNameList.Add(Site);
        }

        return DisplaySiteNameList;
    }

    private void _arrangeTitle(DataTable dt, List<string> DisplaySiteNameList, out StringBuilder sbSiteData)
    {
        sbSiteData = new StringBuilder();
        List<string> ExtraSiteTitleList = new List<string>(new string[] { "Local Time", "Total", "Column1", "Column2" });

        #region arrange sequence of title ------
        //--- sequence of title displays by ExtraTitleList & SiteNameList --//
        StringBuilder sbTH = new StringBuilder();
        string _tmpTRBlock = "<tr>{0}</tr>";
        string _tmpTHBlock = "<th id='SupertableFixedCol'>{0}</th>";

        foreach (string Title in ExtraSiteTitleList)
        {
            sbTH.AppendFormat(_tmpTHBlock, Title);
        }

        foreach (string Title in DisplaySiteNameList)
        {
            sbTH.AppendFormat(_tmpTHBlock, _renameSite(Title));
        }
        #endregion -----------------------------

        sbSiteData.AppendFormat(_tmpTRBlock, sbTH.ToString());
    }


}
