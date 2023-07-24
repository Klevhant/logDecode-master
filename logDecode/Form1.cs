using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace logDecode
{
    public partial class logDecodeForm : Form
    {
        // todo, xlsx not working
        private String mapFileNameFull = "DataSetDefine-logdecode-RTC.xls";
        private DataSet dsStru;

        private String workDir = "";
        private String workDirFull = "";
        private String mapDir = "EEPROM MAP";
        private String mapDirFull = "";
        private String[] mapList;
        //private String mapFileNameFull = "";
        public String myScriptInfo;

        public logDecodeForm()
        {
            InitializeComponent();
        }

        private void logDecodeForm_Load(object sender, EventArgs e)
        {
            Assembly assem = Assembly.GetEntryAssembly();
            AssemblyName assemName = assem.GetName();
            Version ver = assemName.Version;

            //todo; generate Script Info as LogDecode###.ahk - yymmdd-HHMMSS
            // Form Title = application - version
            this.Text = assemName.Name + " - " + ver.ToString();
            myScriptInfo = assemName.Name + " - " + ver.ToString();

            if (workDir == "")
            {
                workDirFull = Path.GetFullPath(".");
            }
            else
            {
                if (!Directory.Exists(workDir))
                    Directory.CreateDirectory(workDir);
                workDirFull = Path.GetFullPath(workDir);
            }

            if (mapDir == "")
            {
                mapDirFull = Path.GetFullPath(".");
            }
            else
            {
                if (!Directory.Exists(mapDir))
                    Directory.CreateDirectory(mapDir);
                mapDirFull = Path.GetFullPath(mapDir);
            }

            loadMapList();
        }

        private void loadMapList()
        {
        	SearchOption mySearchOption = SearchOption.AllDirectories;
        	mapList = Directory.GetFiles(mapDirFull, "EEPROM*.xls", mySearchOption);
        	
        	updateCbItemFromArray(cbMapList, mapList, "");
        	
        }

        private void updateCbItemFromArray(ComboBox cb, String[] sList, String sDefault)
        {
        	int selIdx = -1;
            int oldSel = cb.SelectedIndex;
            int newSel = -1;
            newSel = oldSel;    // ToDo

            cb.Items.Clear();
            if (sDefault != "")
            {
            	selIdx = Array.IndexOf(sList, sDefault);
            	if (selIdx > 0)
            	{
	                newSel = selIdx;
            	}
	            /*Match matchIdx0 = Regex.Match(tempStr, @"<\d+>");
	            if (!matchIdx0.Success)*/
            }
            for (int i = 0; i < sList.Length; i++)
            {
            	String tempStr = string.Format("<{0:0}> {1}", i, 
            		              Path.GetFileName(sList[i]).Replace("EEPROM MAP", ""));
	                cb.Items.Add(tempStr);
            }
          	cb.SelectedIndex = Math.Max(0, Math.Min(newSel, sList.Length-1));
        }

        private int idxFromCb(ComboBox cb)
        {
            string sIdx = "";
            int iIdx = -1;
            Match matchIdx0 = Regex.Match(cb.Text, @"<\d+>");

            if (matchIdx0.Success)
            {
                Match matchIdx1 = Regex.Match(matchIdx0.Groups[0].Value, @"\d+");
                if (matchIdx1.Success)
                {
                    sIdx = matchIdx1.Groups[0].Value;
                    iIdx = Convert.ToInt32(sIdx);
                }
            }

            return (iIdx >= 0) ? iIdx : -1;
        }

        
        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            String sFile1 = "";
            //sFile1 = @"D:\Dropbox\0K\(K15)\4-work\KMI_log\3.5AH\2015-11-06_17-41-40_CA0311000001_eeprom_dump.txt";
            String outNameLog, outNameLogNG, outNameErr, outNameID;

            //MessageBox.Show(cbMapList.Text);
            
            //todo; generate GUI window to show progress
            // todo, read input filename from command line

            if (!File.Exists(sFile1))
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Title = "Select file";
                dialog.InitialDirectory = ".\\";
                dialog.Filter = "Klever Biactron Vehicle Log (*.*)|*.*";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    sFile1 = (dialog.FileName);
                }
                else
                {
                    sFile1 = @"D:\Dropbox\0K\(K15)\4-work\KMI_log\Q25 150806 Jeffrey\2015-08-06_13-28-46_B9031139T025_eeprom_dump.txt";
                    sFile1 = @"D:\Dropbox\0K\(K15)\4-work\Q\FIR\150908 Daniel\2015-09-08_09-18-17_00Empty___ID_eeprom_dump.txt";
                }
            }

            if (!File.Exists(sFile1)) return;

            String sFp = Path.GetDirectoryName(sFile1);
//            String sExt = Path.GetExtension(sFile1);
            String sFN_ = Path.GetFileNameWithoutExtension(sFile1);

            // generate output filename (add numbers to avoid overwrite)  
            String sKey = "";
            using (null)  // todo ?
            {
                int i = 0;
                do
                {
                    outNameLog = string.Format(@"{0}\{1}{2}.csv", sFp, sFN_, sKey);
                    outNameLogNG = string.Format(@"{0}\{1}{2}_SUMNG.csv", sFp, sFN_, sKey);
                    outNameErr = string.Format(@"{0}\{1}{2}_err.csv", sFp, sFN_, sKey);
                    outNameID = string.Format(@"{0}\{1}{2}_ID.csv", sFp, sFN_, sKey);

                    bool fExist = false;
                    foreach (String sFN2 in new String[] {outNameLog, outNameLogNG, outNameErr, outNameID})
                    {
                        if (File.Exists(sFN2))
                        {
                            fExist = true;
                            break;
                        }
                    }
                    if (!fExist) break;     // all files not exist

                    i++;
                    sKey = string.Format("_{0:0}", i);
                } while (true);
            }
            //todo; check for an offset binary file, leading((+/-##)) 
            //todo;   FileGetSize, file1size, %file1%
            //todo, struKey to be flexible
            /*int[] posStart = { 0, 512, 6272 };
            int[] posEnd = { 512, 6272, 131072 };*/
            // List<DataTable> dtStru = new List<DataTable>();
            
            mapFileNameFull = mapList[idxFromCb(cbMapList)];
            String mapName = Path.GetFileNameWithoutExtension(mapFileNameFull).Replace("EEPROM MAP", "");
            
            try
            {
                // Load DataSet Definitions
                dsStru =
                    DRT.ExcelDb.GetExcelToDataSet(mapFileNameFull, false);      // 第一行是標題
            }
            catch (Exception ex)
            {
                String message = "Failed with below error.  Program will terminate.\n\n" + ex.Message;
                String caption = "ERROR !!";
                var result = MessageBox.Show(message, caption,
                                             MessageBoxButtons.OK,
                                             MessageBoxIcon.Error);
                //Environment.Exit(Environment.ExitCode);
                return;
            }            
            
            DataTable dtStru = null;
            String[] struKey = { "id", "err", "log" };
            String[] outName = { outNameID, outNameErr, outNameLog };
            String[] outNameNG = { "", "", outNameLogNG };

            //todo; if (ifOffset)
            String files = "";
            int[] loopTypes = new int[] { 0, 1, 2 };
            foreach (int typeNum in loopTypes)
            {
                String typeID = struKey[typeNum];

                // init Stru
                // get DataTable name contains "stru" & { "id", "err", "log" }
                dtStru = DRT.Stru.dtNameContainsAll(dsStru, new String[] { "stru", typeID });
                DRT.Stru.logStru thisStru = DRT.Stru.logStruFromDt(dtStru);
                int recLen = thisStru.Len;
                long readPos = thisStru.Start;
                long endPos = thisStru.End;

                DataTable dtRec = DRT.Stru.newDtFromLogStru(thisStru);

                // init File Heading  
                String Line0 = mapName + "," + DRT.Stru.dumpStru(thisStru, ",", "size", "");
                String Line1 = myScriptInfo + " " + "Address," + DRT.Stru.dumpStru(thisStru, ",", "", "Desc") + ",Calculated Sum";
                String Line2 = "ADDR," + DRT.Stru.dumpStru(thisStru, ",", "", "Name") + ",SUM";
                String outHeading = Line1 + "\n" + Line0 + "\n" + Line2;

                StreamWriter sw = null;
                StreamWriter sw2 = null;
                using (Stream source = File.OpenRead(sFile1))
                //            using ()
                {
                    byte[] buffer = new byte[2048];
                    int bytesRead;
                    while ((readPos < endPos) && (source.Seek(readPos, SeekOrigin.Begin) >= 0) 
                        && ((bytesRead = source.Read(buffer, 0, recLen)) > 0))
                    {
                        //                    String thisAddrHex = Convert.ToString(readPos, 16);
                        String thisAddrHex = "0x" + readPos.ToString("x5");
                        /*if ((readPos>2300) && (readPos<2500))
                        	MessageBox.Show(thisAddrHex);*/
                        readPos += recLen;

                        DataRow r = DRT.Stru.newDataRowFromLogStru(dtRec, thisStru, buffer);
                        dtRec.Rows.Add(r);
                        int thisSum = DRT.Stru.ChksumStru(thisStru, buffer);
                        if (thisSum < 0) continue;

                        String RTC_dump = "";
                        try
                        {
                            Int32 RTC_YMD = (Convert.ToUInt16(r["RTC0_G_Y_ANG"]) << 8) + Convert.ToUInt16(r["RTC1_G_X_ANG"]);
                            Int32 RTC_HMS = (Convert.ToUInt16(r["RTC2_K_BAT_AD"]) << 8) + Convert.ToUInt16(r["RTC3_K_AVG_TQ"]);
                            int RTC_Y = RTC_YMD >> 9;
                            int RTC_M = (RTC_YMD >> 5) % 16;
                            int RTC_D = RTC_YMD % 32;
                            int RTC_HH = RTC_HMS >> 11;
                            int RTC_MM = (RTC_HMS >> 5) % 64;
                            int RTC_SS = (RTC_HMS % 32) * 2;
                            RTC_dump = string.Format(",{0},{1},{2},{3},{4},{5}", RTC_Y, RTC_M, RTC_D, RTC_HH, RTC_MM, RTC_SS);
                        }
                        catch (Exception)
                        {
                        }
                        String Line = thisAddrHex + "," + DRT.Stru.DataRowToCSV(r) + "," + thisSum.ToString() + RTC_dump;

                        if ((outNameNG[typeNum] == "") || thisSum == Convert.ToInt16(r["CHKSUM"]))
                        {
                            if (!File.Exists(outName[typeNum]))      // Write Heading for new output file
                            {
                                if (RTC_dump.Length > 0)
                                    outHeading += ",Y,M,D,HH,MM,SS";
                                sw = new StreamWriter(outName[typeNum], true);
                                sw.WriteLine(outHeading);
                                files += "\n" + outName[typeNum];
                            }
                            if (sw != null)
                                sw.WriteLine(Line);
                        }
                        else
                        {
                            if (!File.Exists(outNameNG[typeNum]))      // Write Heading for new output file
                            {
                                sw2 = new StreamWriter(outNameNG[typeNum], true);
                                sw2.WriteLine(outHeading);
                                files += "\n" + outNameNG[typeNum];
                            }
                            if (sw2 != null)
                                sw2.WriteLine(Line);
                        }
                        //if (readPos >= endPos) break;
                    }
                }
                if (sw != null) sw.Close();
                if (sw2 != null) sw2.Close();
            }
            MessageBox.Show("Finished !!\n" + files);
        }

    }
}
