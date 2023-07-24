using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;


namespace DRT
{
    class Stru
    {
        public struct logField
        {
            public String Name;
            //public String Type;
            public int Len;
            public String Var;
            public Double Base;
            public String Desc;
            public int Addr;
        }

        public struct logStru
        {
            public String Name;
            public int Len;
            public int ChksumAddr;
            public logField[] Fields;
            public long Start;
            public long End;
        }

        #region dtCopySafely() - Copy DataTable more robust
        /// <summary>
        /// Copy DataTable more robust
        /// </summary>
        /// <param name="target">target DataTable</param>
        /// <param name="source">source DataTable</param>
        public static void dtCopySafely(DataTable target, DataTable source)
        {
            foreach (DataRow row in source.Rows)
            {
                target.ImportRow(row);
            }
        }
        #endregion

        #region dtNameContainsAll() - get DataTable that contains "all" input string
        /// <summary>
        /// get DataTable that contains "all" input string
        /// </summary>
        /// <param name="ds">DataSet</param>
        /// <param name="allString">string array</param>
        /// <returns>DataTable</returns>
        public static DataTable dtNameContainsAll(DataSet ds, String[] allString)
        {
            DataTable dtOut = null;
            foreach (DataTable dt in ds.Tables)
            {
                String name = dt.TableName.ToLower();
                bool match = true;
                foreach (String s in allString)
                    if (!name.Contains(s))
                    {
                        match = false;
                        break;
                    }
                if (!match) continue;
                dtOut = dt;
                break;
            }
            return dtOut;
        }
        #endregion

        #region logStruFromDt() - define logStru from a DataTable
        /// <summary>
        /// define logStru from a Datatable
        /// </summary>
        /// <param name="dtStru">DataTable contains Stru definition</param>
        /// <returns>logStru</returns>
        public static logStru logStruFromDt(DataTable dtStru)
        {
            logStru myStru = new logStru();
            List<logField> myList = new List<logField>();
            int StruLen = 0;

            foreach (DataRow r in dtStru.Rows)
            {
                logField thisField = new logField();
                thisField.Name = r["Name"].ToString();
                thisField.Var  = r["Var"].ToString();
                thisField.Desc = r["Desc"].ToString();
                thisField.Base = 1;

                try
                {
                    thisField.Addr = Convert.ToInt32(r["Addr"]);
                }
                catch (Exception)
                {
                }

                StruLen = Math.Max(StruLen, thisField.Addr);

                try
                {
                    thisField.Len = Convert.ToInt32(r["Len"]);
                    thisField.Base = Convert.ToDouble(r["Base"]);
                }
                catch (Exception)
                {
                    thisField.Base = 1;
                }

                if (thisField.Base == 0) thisField.Base = 1;
                if ((myStru.ChksumAddr <= 0) && thisField.Name.ToLower().Contains("chksumaddr"))
                    myStru.ChksumAddr = thisField.Addr;
                if ((myStru.Len <= 0) && thisField.Name.ToLower().Contains("strulenaddr"))
                    myStru.Len = thisField.Addr;
                if ((myStru.Start <= 0) && thisField.Name.ToLower().Contains("startoffsetaddr"))
                    myStru.Start = thisField.Addr;
                if ((myStru.End <= 0) && thisField.Name.ToLower().Contains("endoffsetaddr"))
                    myStru.End = thisField.Addr;
                myList.Add(thisField);
            }
            myStru.Fields = myList.ToArray();
            if (myStru.Len <= 0) myStru.Len  = StruLen;
            return myStru;
        }
        #endregion

        #region newDtFromLogStru() - prepare DataTable to contain decoded Stru
        /// <summary>
        ///  prepare DataTable to contain decoded Stru
        /// </summary>
        /// <param name="thisStru">logStru</param>
        /// <returns>DataTable</returns>
        public static DataTable newDtFromLogStru(logStru thisStru)
        {
            DataTable dt = new DataTable();
            foreach (logField field in thisStru.Fields)
            {
                String colName = field.Name;
                DataColumn newCol;

                if (((newCol = dt.Columns[colName]) == null) && (field.Len > 0))
                    newCol = dt.Columns.Add(colName);
                if (newCol == null)
                    continue;
                switch (field.Var.ToLower()[0])
                {
                    case 'u':
                    case 's':
                    case 'f':
                        newCol.DataType = typeof(Double);
                        break;
                }
            }
            return dt;
        }
        #endregion

        public static int ChksumStru(logStru thisStru, byte[] buffer)
        {
            int sum = 0;
            bool hasData = false;

            for (int i = 0; i < thisStru.ChksumAddr; i++)
            {
                int raw = buffer[i];
                if ((raw > 0) && (raw < 255))
                    hasData = true;
                sum += raw;
            }
            sum = sum & 255;

            if (hasData)
                return sum;
            else
                return -1;
        }

        public static String dumpStru(logStru thisStru, String separator, String showName, String fieldNameOnly)
        {
            String Lines = "";
            foreach (logField var in thisStru.Fields)
            {
                if (var.Len <= 0) continue;
                int varFrom = var.Addr;
                int varTo = varFrom + var.Len - 1;
                if (Lines.Length > 0) Lines += separator;
                if (fieldNameOnly.Length > 0) // todo;
                {
                    string fieldName = fieldNameOnly.ToLower();
                    if (fieldName.Contains("name"))
                    {
                        Lines += var.Name;
                        continue;
                    }
                    if (fieldName.Contains("desc"))
                    {
                        Lines += var.Desc;
                        continue;
                    }
                }
                if (showName.ToLower().Contains("size"))
                {
                    if (varFrom == varTo)
                    {
                        Lines += "'" + varFrom.ToString("x2");
                    }
                    else
                    {
                        Lines += varFrom.ToString("x2") + "~" + varTo.ToString("x2");
                    }
                }
                else
                {
                    Lines += ""; // todo
                }
            }
            return Lines;
        }

        public static String DataRowToCSV(DataRow dr)
        {
            //String csv = "\t";
            String csv = ",";
            String temp = "";
            int colCount = dr.Table.Columns.Count;

            if (colCount > 0 && !Convert.IsDBNull(dr[0]))
                temp += dr[0].ToString();
            for (int i = 1; i < colCount; i++)
                temp += csv + dr[i].ToString();

            return temp;
        }

        public static DataRow newDataRowFromLogStru(DataTable dt, logStru thisStru, byte[] buffer)
        {
            DataRow dRow = dt.NewRow( );
            foreach (logField var in thisStru.Fields)
            {
                if (var.Len <= 0) continue;

                int intLen = var.Len;
                int varLen = var.Len;
                int j = var.Addr;
                int stepj = 1;
                String tempStr = "";
                int tempInt = 0;
                Double tempNum = 0;

                /*if (var.Len == 3)     // debug only
                    tempNum++;*/    

                switch (var.Var.ToLower()[0])
                {
                    case 'c':   // reverse byte order
                        j = var.Addr + var.Len - 1;
                        stepj = -1;
                        break;
                    case 'u':   // support non power-of-2 (1,2,4,8) integer
                    case 's':
                    case 'f':
                        intLen = 1;
                        while (intLen < var.Len)
                            intLen <<= 1;
                        varLen = intLen;
                        break;
                }
                byte[] rawByte = new byte[varLen];

                for (int i = 0; i < var.Len; i++)
                {
                    if (j >= thisStru.Len)
                        break;
                    rawByte[i] = buffer[j];
                    j += stepj;
                }

                switch (var.Var.ToLower()[0])
                {
                    case 'n': //0~9 A~Z
                        if (rawByte[0] < 10)
                        {
                            dRow[var.Name] = Convert.ToString(rawByte[0]);
                        }
                        else
                        {
                            char c = Convert.ToChar(rawByte[0] + 55);
                            dRow[var.Name] = Convert.ToString(c);
                        }
                        break;
                    case 'b':   // binary
                        for (int i = 0; i < var.Len; i++)
                        {   // high byte (higher address) in the front
                            tempStr = Convert.ToString(rawByte[i], 2).PadLeft(8, '0').Insert(4, " ")
                                + (tempStr.Length > 0 ? " " : "") + tempStr;
                        }
                        dRow[var.Name] = tempStr;
                        break;
                    case 'h':   // hex
                        for (int i = 0; i < var.Len; i++)
                        {   // todo, byte order ??
                            //tempStr = tempStr + (tempStr.Length > 0 ? " " : "") + rawByte[i].ToString("X2");
                            tempStr = rawByte[i].ToString("X2") + (tempStr.Length > 0 ? " " : "") + tempStr;
                        }
                        dRow[var.Name] = tempStr;
                        break;
                    case 'c':   // string
                    case 'd':
                        // http://isanhsu.blogspot.tw/2012/03/stringbyte-c.html
                        dRow[var.Name] = System.Text.Encoding.Default.GetString(rawByte);
                        break;
                    case 's':   // signed int
                        switch (varLen)
                        {
                            case 1:
                                tempInt = rawByte[0];
                                dRow[var.Name] = (tempInt - 256 * (tempInt >> 7)) * var.Base;
                                break;
                            case 2:
                                dRow[var.Name] = BitConverter.ToInt16(rawByte, 0) * var.Base;
                                break;
                            case 4:
                                dRow[var.Name] = BitConverter.ToInt32(rawByte, 0) * var.Base;
                                break;
                            case 8:
                                dRow[var.Name] = BitConverter.ToInt64(rawByte, 0) * var.Base;
                                break;
                            default:
                                // dRow[varName] = BitConverter.ToInt64(rawByte, 0);
                                break;
                        }
                        break;
                    default:    // unsigned int or float
                        switch (varLen)
                        {
                            case 1:
                                dRow[var.Name] = rawByte[0];
                                break;
                            case 2:
                                dRow[var.Name] = BitConverter.ToUInt16(rawByte, 0);
                                break;
                            case 4:
                                dRow[var.Name] = BitConverter.ToUInt32(rawByte, 0);
                                break;
                            case 8:
                                dRow[var.Name] = BitConverter.ToUInt64(rawByte, 0);
                                break;
                            default:
                                // dRow[varName] = BitConverter.ToUInt64(rawByte, 0);
                                break;
                        }
                        if (var.Var.ToLower()[0] == 'u')
                            dRow[var.Name] = Convert.ToDouble(dRow[var.Name]) * var.Base;
                        // todo, float
                        break;
                }
            }
            return dRow;
        }
    }
}
