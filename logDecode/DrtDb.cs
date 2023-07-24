using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Data;
using System.Windows.Forms;
using System.Globalization;

namespace DRT
{
    class DrtDb
    {

        public static List<DataSet> DefineByExcel(DataSet dsDefine)
        {
            List<DataSet> result = new List<DataSet>();
            List<string> DataSetNames = new List<string>();

            // collect Excel Sheet Names (w/o $) into DataSetNames
            foreach (DataTable dt in dsDefine.Tables)
            {
                string ss = dt.TableName;
                if (ss.LastIndexOf('$') > 0)
                {
                    ss = ss.Substring(0, ss.Length - 1);
                }
                if (!DataSetNames.Contains(ss))
                    DataSetNames.Add(ss);
            }

            foreach (string s in DataSetNames)
            {
                string ss = s;
                if (dsDefine.Tables.Contains(ss + "$"))
                    ss = ss + "$";
                string sDefineTableName = "";
                string sDefineColName = "";
                string sDefineColSpec = "";
                DataTable dt = dsDefine.Tables[ss];
                if (dt == null)
                    continue;
                // check keywords from column name
                foreach (DataColumn c in dt.Columns)
                {
                    string n = c.ColumnName;
                    string m = n.ToLower();
                    if ((sDefineTableName == "") && m.Contains("tab"))
                    {
                        sDefineTableName = n;
                        continue;
                    }
                    if ((sDefineColName == "") && m.Contains("col") && !m.Contains("spec"))
                    {
                        sDefineColName = n;
                        continue;
                    }
                    if ((sDefineColSpec == "") && m.Contains("col") && m.Contains("spec"))
                    {
                        sDefineColSpec = n;
                        continue;
                    }
                }
                if ((sDefineTableName == "") || (sDefineColName == "") || (sDefineColSpec == ""))
                    continue;
                DataSet newDs = new DataSet(s);
                result.Add(newDs);
                foreach (DataRow r in dt.Rows)
                {
                    string tName = r[sDefineTableName].ToString();
                    DataTable newDt = newDs.Tables[tName];
                    if (newDt == null)
                    {
                        //newDt = new DataTable(tName);
                        newDt = newDs.Tables.Add(tName);
                    }
                    string cName = r[sDefineColName].ToString();
                    string cSpec = r[sDefineColSpec].ToString().ToLower();
                    DataColumn newCol = newDt.Columns[cName];
                    if (newCol == null)
                    {
                        //                        newCol = new DataColumn(cName);
                        newCol = newDt.Columns.Add(cName);
                    }
                    newCol.ExtendedProperties.Add("preDef", true);
                    /*
                     * I: int64, D:Decimal, t:datetime, +incremental, -hidden, *key, ^unique, r:readonly
                     */

                    string qSpec = "";
                    do
                    {
                        Match matchQ = Regex.Match(cSpec, @"\[(\w*)\]");
                        if (!matchQ.Success)
                            break;
                        qSpec += matchQ.Groups[0].Value.ToLower();
                        switch (matchQ.Groups[1].Value.ToLower())
                        {
                            case "cb":
                                newCol.ExtendedProperties.Add("dgvColType", "cb");//typeof( //DataGridViewComboBoxCell));
                                    //DataGridViewComboBoxColumn));
                                break;
                            case "chk":
                                newCol.ExtendedProperties.Add("dgvColType", "chk");//typeof( //DataGridViewCheckBoxCell));
                                    //DataGridViewCheckBoxColumn));
                                break;
                        }
                        cSpec = cSpec.Replace(matchQ.Groups[0].Value, "");
                    } while (true);

                    do
                    {
                        Match matchQ = Regex.Match(cSpec, @"{[^}]*}");
                        if (!matchQ.Success)
                            break;
                        qSpec += matchQ.Groups[0].Value.ToLower();
                        cSpec = cSpec.Replace(matchQ.Groups[0].Value, "");
                    } while (true); 

                    if (qSpec.Length > 0)
                        newCol.ExtendedProperties.Add("qSpec", qSpec);

                    string sSpec = "";
                    for (int i = 0; i < cSpec.Length; i++)
                    {
                        sSpec += cSpec[i];
                        switch (cSpec[i])
                        {
                            case 'f':
                                newCol.DataType = typeof(Double);
                                break;
                            case 'i':
                                newCol.DataType = typeof(Int64);
                                break;
                            case 'd':
                                newCol.DataType = typeof(Decimal);
                                break;
                            case 't':
                                newCol.DataType = typeof(DateTime);
                                break;
                            case '+':
                                newCol.AutoIncrement = true;
                                break;
                            case '^':
                                newCol.Unique = true;
                                break;
                            case '*':
                                int k = newDt.PrimaryKey.Length;
                                if (k == 0)
                                {
                                    //DataColumn[] keys = new DataColumn[1];
                                    //keys[0] = newCol;
                                    newDt.PrimaryKey = new DataColumn[] { newCol };
                                }
                                else
                                {
                                    DataColumn[] keys = new DataColumn[k + 1];
                                    Array.Copy(newDt.PrimaryKey, keys, k);
                                    keys[k] = newCol;
                                    newDt.PrimaryKey = keys;
                                }
                                break;
                            case '-':
                                // newCol.ExtendedProperties.Add("dgvHidden", true);    //130618
                                // column.ExtendedProperties["TimeStamp"].ToString())
                                //.Site .Name += "-";
                                break;
                        }
                    }
                    if (sSpec.Length > 0)
                        newCol.ExtendedProperties.Add("sSpec", sSpec);
                }
            }
            return result;
        }

        public static void DataSetFromXmlSafely(DataSet ds, string xmlName)
        {
            if (!File.Exists(xmlName))
                return;
            using (DataSet dsTemp1 = new DataSet())
            {
                dsTemp1.ReadXml(xmlName);
                foreach (DataTable dt in ds.Tables)
                {
                    DataTable dt1 = dsTemp1.Tables[dt.TableName];
                    if (dt1 == null)
                        continue;
                    foreach (DataColumn c1 in dt1.Columns)
                    {
                        if (dt.Columns[c1.ColumnName] == null)
                            dt.Columns.Add(c1.ColumnName);
                    }
                }
            }

            ds.ReadXml(xmlName, XmlReadMode.IgnoreSchema);
        }

        public static void dgvApplyQSpec(DataGridView dgv)
        {
            DataTable dt = null;
            dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;

            foreach (DataGridViewColumn dgvC in dgv.Columns)
            {
                DataColumn dtC = dt.Columns[dgvC.HeaderText];
                if (dtC == null)
                    continue;
 //               dgvC.DisplayIndex = dtC.Ordinal;
                string qSpec = "";
                string sSpec = "";
                try
                { qSpec = dtC.ExtendedProperties["qSpec"].ToString().ToLower(); }
                catch (Exception ee) { }
                try
                { sSpec = dtC.ExtendedProperties["sSpec"].ToString().ToLower(); }
                catch (Exception ee) { }
                Match match;
                // AutoSizeColumnsMode [a0]~[a7]
                match = Regex.Match(qSpec, @"\[a(\d*)\]", RegexOptions.IgnoreCase);
                if (match.Success)
                    switch (match.Groups[1].Value.ToLower())
                    {
                        case "0":
                            dgvC.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                            break;
                        case "1":
                            dgvC.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
                            break;
                        case "2":
                            dgvC.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                            break;
                        case "3":
                            dgvC.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                            break;
                        case "4":
                            dgvC.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCellsExceptHeader;
                            break;
                        case "5":
                            dgvC.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                            break;
                        case "6":
                            dgvC.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                            break;
                        case "7":
                            dgvC.AutoSizeMode = DataGridViewAutoSizeColumnMode.NotSet;
                            break;
                    }
                // Width [w##]
                match = Regex.Match(qSpec, @"\[w(\d*)\]", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    int w = Convert.ToInt32(match.Groups[1].Value.ToLower());
                    if ((w > 0) && (w < 500))
                    {
                        dgvC.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                        dgvC.Width = w;
                    }
                }
#if AAA
                // Read access level [r#]
                match = Regex.Match(qSpec, @"\[r(\d*)\]", RegexOptions.IgnoreCase);
                if (match.Success)
                    switch (match.Groups[1].Value.ToLower())
                    {
                        case "0":
                            break;
                        case "1":
                            dgvC.Visible = false;
                            break;
                    }
                // {...} DefaultCellStyle format
                match = Regex.Match(qSpec, @"{([^}]*)}", RegexOptions.IgnoreCase);
                if (match.Success)
                    dgvC.DefaultCellStyle.Format = match.Groups[1].Value;
                //dgvC.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                if (dgvC.HeaderText.ToLower().Contains("idx"))
                {
                    dgvC.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCellsExceptHeader;
                    dgvC.MinimumWidth = 20;
                }
                if (Convert.ToBoolean(dtC.ExtendedProperties["dgvHidden"]) || sSpec.Contains('-'))
                {
                    dgvC.Visible = false;
                }
                if (sSpec.Contains('r') || (dtC.Expression != ""))
                {
                    dgvC.ReadOnly = true;
                    dgvC.DefaultCellStyle.BackColor = Color.FromArgb(192, 192, 192);
                    dgvC.DefaultCellStyle.SelectionBackColor = Color.FromArgb(160, 160, 160);
                }
#endif
            }

        }

        // C# Binary File Compare - http://stackoverflow.com/a/968980
        // check if two stream the same content
        public static bool StreamEqualsFromEnd(Stream stream1, Stream stream2)
        {
            const int bufferSize = 2048;
            byte[] buffer1 = new byte[bufferSize]; //buffer size
            byte[] buffer2 = new byte[bufferSize];
            stream1.Position = Math.Max(0, stream1.Length - stream2.Length);
            stream2.Position = Math.Max(0, stream2.Length - stream1.Length);
            while (true)
            {
                int count1 = stream1.Read(buffer1, 0, bufferSize);
                int count2 = stream2.Read(buffer2, 0, bufferSize);

                if (count1 != count2)
                    return false;

                if (count1 == 0)
                    return true;
                
                // You might replace the following with an efficient "memcmp"
                if (!buffer1.Take(count1).SequenceEqual(buffer2.Take(count2)))
                    return false;
            }
        }

        // save only when Xml changed
        public static bool saveChangedXmlFromDs(DataSet ds, string name, XmlWriteMode mode)
        {
            bool result = true;
            try
            {
                if (File.Exists(name))
                {
                    using (MemoryStream newStream = new MemoryStream())
                    using (var oldStream = new FileStream(name, FileMode.Open))
                    {
                        ds.WriteXml(newStream, mode);
                        result = !StreamEqualsFromEnd(newStream, oldStream);
                    }
                    if (result)
                        File.Delete(name);
                }
                if (result)
                    ds.WriteXml(name, mode);
                //result = (File.Exists(name));
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public void DataTableToCSV(DataTable dt, string strFilePath, bool app) // strFilePath 為輸出檔案路徑 (含檔名)
        {
            StreamWriter sw = new StreamWriter(strFilePath, app);
            string csv = "\t";

            int intColCount = dt.Columns.Count;

            if (dt.Columns.Count > 0)
                sw.Write(dt.Columns[0]);
            for (int i = 1; i < dt.Columns.Count; i++)
                sw.Write(csv + dt.Columns[i]);

            sw.Write(sw.NewLine);
            foreach (DataRow dr in dt.Rows)
            {
                if (dt.Columns.Count > 0 && !Convert.IsDBNull(dr[0]))
                    sw.Write(dr[0].ToString());
                for (int i = 1; i < intColCount; i++)
                    sw.Write(csv + dr[i].ToString());
                sw.Write(sw.NewLine);
            }
            sw.Close();
        }

        public static void DataSetToCSV(DataSet ds, string strFilePath, bool app, bool ifName)
        {
            StreamWriter sw = new StreamWriter(strFilePath, app);
            if (ifName)
            {
                sw.WriteLine(ds.DataSetName);
            }
            sw.Close();

            foreach (DataTable dt in ds.Tables)
            {
                DataTableToCSV(dt, strFilePath, true, ifName);
            }
        }

        public static void DataTableToCSV(DataTable dt, string strFilePath, bool app, bool ifName) // strFilePath 為輸出檔案路徑 (含檔名)
        {
            StreamWriter sw = new StreamWriter(strFilePath, app);
            string csv = "\t";

            int intColCount = dt.Columns.Count;

            if (ifName)
            {
                sw.WriteLine();
                sw.WriteLine(dt.TableName);
            }

            if (dt.Columns.Count > 0)
                sw.Write(dt.Columns[0]);
            for (int i = 1; i < dt.Columns.Count; i++)
                sw.Write(csv + dt.Columns[i]);

            sw.Write(sw.NewLine);
            foreach (DataRow dr in dt.Rows)
            {
                if (dt.Columns.Count > 0 && !Convert.IsDBNull(dr[0]))
                    sw.Write(dr[0].ToString());
                for (int i = 1; i < intColCount; i++)
                    sw.Write(csv + dr[i].ToString());
                sw.Write(sw.NewLine);
            }
            sw.Close();
        }

        public static string GetPrettyDate(Decimal dd, string s)
        {
            string ddStr = dd.ToString();
            DateTime d;
            switch (ddStr.Length)
            {
                case 14:    // yyyyMMddHHmmss
                    d = DateTime.ParseExact(dd.ToString(), "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                    break;
                case 12:    // yyMMddHHmmss
                    d = DateTime.ParseExact(dd.ToString(), "yyMMddHHmmss", CultureInfo.InvariantCulture);
                    break;
                default:
                    return ("NA");
            }
            //if (dd <= 19000101000000)
            //    return ("NA");
            string pretty = GetPrettyDate(d);
            if (pretty == null)  //(d == null)
                return string.Format("{0:" + s + "}", d);
            else
            {
                return pretty;
            }
        }

        // http://www.dotnetperls.com/pretty-date
        public static string GetPrettyDate(DateTime d)
        {
            // 1.
            // Get time span elapsed since the date.
            TimeSpan s = DateTime.Now.Subtract(d);

            // 2.
            // Get total number of days elapsed.
            int dayDiff = (int)s.TotalDays;

            // 3.
            // Get total number of seconds elapsed.
            int secDiff = (int)s.TotalSeconds;

            // 4.
            // Don't allow out of range values.
            if (dayDiff < 0 || dayDiff >= 31)
            {
                return null;
            }

            // 5.
            // Handle same-day times.
            if (dayDiff == 0)
            {
                // A.
                // Less than one minute ago.
                if (secDiff < 60)
                {
                    return "just now";
                }
                // B.
                // Less than 2 minutes ago.
                if (secDiff < 120)
                {
                    return "1 minute ago";
                }
                // C.
                // Less than one hour ago.
                if (secDiff < 3600)
                {
                    return string.Format("{0} minutes ago",
                        Math.Floor((double)secDiff / 60));
                }
                // D.
                // Less than 2 hours ago.
                if (secDiff < 7200)
                {
                    return "1 hour ago";
                }
                // E.
                // Less than one day ago.
                if (secDiff < 86400)
                {
                    return string.Format("{0} hours ago",
                        Math.Floor((double)secDiff / 3600));
                }
            }
            // 6.
            // Handle previous days.
            if (dayDiff == 1)
            {
                return "yesterday";
            }
            if (dayDiff < 7)
            {
                return string.Format("{0} days ago",
                dayDiff);
            }
            if (dayDiff < 31)
            {
                return string.Format("{0} weeks ago",
                Math.Ceiling((double)dayDiff / 7));
            }
            return null;
        }
    }
}
