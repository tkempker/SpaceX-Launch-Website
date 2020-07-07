using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using Telerik.Windows.Documents.Spreadsheet.Expressions.Functions;

namespace SpaceX_Launch_Website
{
    public partial class _Default : Page
    {
        protected List<string> Date { get; set; }
        protected List<DateTime> CentralTime { get; set; }
        protected List<string> RocketName { get; set; }
        protected List<bool> Success { get; set; }
        protected List<int> PayloadMass { get; set; }
        protected int[] PayloadRank { get; set; }


        protected void Page_Load(object sender, EventArgs e)
        {
            string url = String.Format("https://api.spacexdata.com/v3/launches");
            WebRequest request = WebRequest.Create(url);
            request.Method = "GET";
            HttpWebResponse response = null;
            response = (HttpWebResponse)request.GetResponse();

            string result = null;
            using (Stream stream = response.GetResponseStream())
            {
                StreamReader sr = new StreamReader(stream);
                result = sr.ReadToEnd();
                sr.Close();
            }

            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();

            dynamic data = serializer.Deserialize(result, typeof(object));
            var dataJson = data;
            object[] launches = dataJson;

            List<string> dateList = new List<string>();
            List<bool> successList = new List<bool>();
            List<string> nameList = new List<string>();
            List<int> massList = new List<int>();

            for (int i = 0; i < launches.Length - 1; i++)
            {
                dateList.Add(System.Convert.ToString(dataJson[i]["launch_date_local"]));
                successList.Add(System.Convert.ToBoolean(dataJson[i]["launch_success"]));
                nameList.Add(System.Convert.ToString(dataJson[i]["rocket"]["rocket_name"]));
                massList.Add(System.Convert.ToInt32(dataJson[i]["rocket"]["second_stage"]["payloads"][0]["payload_mass_lbs"]));
            }

            Date = dateList;
            RocketName = nameList;
            Success = successList;
            PayloadMass = massList;

            List<DateTime> timeList = new List<DateTime>();
            timeList = getCentralTime(Date);
            CentralTime = timeList;

            int[] payloadRankList = new int[massList.Count];
            payloadRankList = rankPayloads(massList);
            PayloadRank = payloadRankList;
        }

        protected void RadGrid1_NeedDataSource(object sender, GridNeedDataSourceEventArgs e)
        {
            (sender as RadGrid).DataSource = GetGridSource();
        }

        private List<DateTime> getCentralTime(List<string> timestamp)
        {
            List<DateTime> time = new List<DateTime>();
            for(int i = 0; i < timestamp.Count; i++)
            {
                DateTime newTime = System.Convert.ToDateTime(timestamp[i]);
                newTime.ToLocalTime();
                time.Add(newTime);
            }

            return time;
        }

        private int[] rankPayloads(List<int> payloadList)
        {
            List<int> temp = new List<int>();
            
            foreach(int num in payloadList)
            {
                temp.Add(num);
            }

            int[] rankList = new int[temp.Count];
            
            int rank = 1;

            while(temp.Count >= rank)
            {
                int curBiggest = temp.Max();
                int index = temp.IndexOf(curBiggest);
                rankList[index] = rank;
                temp[index] = -1;
                
                rank++;
            }
            
            return rankList;
        }

        private DataTable GetGridSource()
        {
            DataTable dataTable = new DataTable();

            DataColumn column = new DataColumn();
            column.DataType = Type.GetType("System.Int32");
            column.ColumnName = "LaunchID";
            dataTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = Type.GetType("System.DateTime");
            column.ColumnName = "Date";
            dataTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = Type.GetType("System.String");
            column.ColumnName = "RocketName";
            dataTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = Type.GetType("System.Boolean");
            column.ColumnName = "Success";
            dataTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = Type.GetType("System.Decimal");
            column.ColumnName = "PayloadMass";
            dataTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = Type.GetType("System.Int32");
            column.ColumnName = "PayloadRank";
            dataTable.Columns.Add(column);

            DataColumn[] PrimaryKeyColumns = new DataColumn[1];
            PrimaryKeyColumns[0] = dataTable.Columns["LaunchID"];
            dataTable.PrimaryKey = PrimaryKeyColumns;

            for(int i = 0; i < Date.Count; i++)
            {
                DataRow row = dataTable.NewRow();
                row["LaunchID"] = i + 1;
                row["Date"] = CentralTime[i];
                row["RocketName"] = RocketName[i];
                row["Success"] = Success[i];
                row["PayloadMass"] = PayloadMass[i];
                row["PayloadRank"] = PayloadRank[i];

                dataTable.Rows.Add(row);
            }
            

            return dataTable;

        }

    }
}