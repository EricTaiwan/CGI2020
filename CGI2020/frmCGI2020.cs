using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Configuration;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Linq.Dynamic;
using NCalc;
using NCalc.Domain;
using System.Text.RegularExpressions;
using System.Collections.Specialized;

namespace CGI2020
{
    public partial class frmCGI : Form
    {

        static object locker = new object();
        public DateTime LastJobTime = DateTime.Now.TruncateToMinuteStart();
        public LogView logView = new LogView();

        // 讀取 AppSettings
        
        public int iScanSecond = 600;

        public string sInputPath = ConfigurationManager.AppSettings["InputPath"];
        public string sOutputPath = ConfigurationManager.AppSettings["OutputPath"];

        public string sStationSettingPath = ConfigurationManager.AppSettings["StationSettingPath"];

        public string sLastInputFile = ConfigurationManager.AppSettings["LastInputFile"];

        public string sTitles = ConfigurationManager.AppSettings["Titles"];
        public string sCols = ConfigurationManager.AppSettings["Cols"];

        public StationConfig stationConfig;

        public LastFiles cgiLastFiles = new LastFiles();
        public frmCGI()
        {
            InitializeComponent();
            txtInputPath.Text = sInputPath;
            txtOutputPath.Text = sOutputPath;
            txtLastInputFile.Text = sLastInputFile;
        }
        private void frmCSV_Load(object sender, EventArgs e)
        {
            string AP_Path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string AP_FileName = Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            
            this.Text = this.Text + String.Format(" v{0}", System.Reflection.AssemblyName.GetAssemblyName(AP_Path + @"\" + AP_FileName).Version);

            try
            {
                // 讀取站別資料
                stationConfig = new StationConfig(sStationSettingPath);

                btnTXT2CSV_Click(null, null);

            }
            catch (Exception ex)
            {
                logView.Add("Error:" + ex.Message + ":" + ex.InnerException);
            }

        }
        private void frmCSV_Shown(object sender, EventArgs e)
        {


        }
        // 更新畫面資料
        private void timer1_Tick(object sender, EventArgs e)
        {
            // 1.Do something
            // TimerJob1();

            var sStatusMsg = $" >> 檔案處理狀態 {DateTime.Now: yyyy-MM-dd HH:mm:ss}";
            if (grpLogs.Text != sStatusMsg) // 每秒只更新一回
                grpLogs.Text = sStatusMsg;

            // 2.處理顯示畫面
            if (logView.LogList.Count > 0)
            {
                lstMsg.Items.Insert(0, logView.LogList[0].ToString());
                logView.LogList.RemoveAt(0);

                // 只保留 1000 筆
                if (lstMsg.Items.Count >= 1000)
                    lstMsg.Items.RemoveAt(lstMsg.Items.Count - 1);
            }
        }
        private void TimerJob1(bool isAuto = true)
        {
            DateTime JobTime = DateTime.Now.TruncateToMinuteStart();

            // 1.Do something
            if (JobTime.Minute % (iScanSecond / 60) == 0 &&
                (JobTime - LastJobTime).Minutes > 0)
            {
                LastJobTime = DateTime.Now.TruncateToMinuteStart();
                ThreadPool.QueueUserWorkItem((state) =>
                {
                    logView.Add("啟動檢查 CGI 檔案...");

                });
            }
            else
            {
                if (!isAuto)
                {
                    CheckNextJobTime(JobTime);
                }
            }
        }
        private void CheckNextJobTime(DateTime JobTime)
        {
            for (int moreMinute = 1; moreMinute <= 60; moreMinute++)
            {
                DateTime NextJobTime = JobTime.AddMinutes(moreMinute);

                if (NextJobTime.Minute % (iScanSecond / 60) == 0 &&
                    (NextJobTime - LastJobTime).Minutes > 0)
                {
                    logView.Add($"下回轉檔時間: {NextJobTime: yyyy-MM-dd HH:mm:ss}. 參數 ScanSecond = {iScanSecond}");
                    break;
                }
            }
        }
        private void frmCSV_FormClosing(object sender, FormClosingEventArgs e)
        {
            string Msg = "確定離開此程式? ";
            if (MessageBox.Show(Msg, this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                int workerThreads = 0;
                int maxWordThreads = 0;
                int compleThreads = 0;
                ThreadPool.GetAvailableThreads(out workerThreads, out compleThreads);
                ThreadPool.GetMaxThreads(out maxWordThreads, out compleThreads);
                //當可用的線數與池程池最大的線程相等時表示線程池中所有的線程已經完成
                if (workerThreads != maxWordThreads)
                {
                    MessageBox.Show("資料處理中, 請稍候再關閉程式.", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    e.Cancel = true;
                }
            }
            else
            {
                e.Cancel = true;
            }

        }
        private void btnTXT2CSV_Click(object sender, EventArgs e)
        {
            // 1. 讀取所有 Station + PE 設定
            // 2. 讀取最後處理 Station CSV 檔案的下一筆, 依檔名排序
            // 3. 讀取 PE Title, 欄位
            // 4. 處理資料對應, 欄位計算
            // 5. HH 寫入
            // 5.1. 如 HH 存在則新加一行
            // 5.2. 如 HH 不存在則新增
            // 6. Month 寫入
            // 5.1. 如 Month 存在則新加一行
            // 6.2. 如 Month 不存在則新增
            // 7. 記錄最後處理之 CSV 檔案
            // 8. 回 2. 點再下一筆
            ThreadPool.QueueUserWorkItem((state) => {
                logView.Add("啟動檢查...");

                foreach(Station1 sta in stationConfig.Stations)
                {
                    CSVInput(sta);
                }

               }
            );

        }
        private void CSVInput(Station1 sta)
        {
            logView.Add("讀取資料:" + sta.StationName );

            // 更新最後讀取資料
            cgiLastFiles.Reload();

            // 清除舊資料
            sta.InputItems.Clear();

            sta.LastFileName = cgiLastFiles.DataItems.Where(x => x.Key == sta.StationName).Count() > 0
                ? cgiLastFiles.DataItems.Where(x => x.Key == sta.StationName).FirstOrDefault().Value : "";

            // 讀取 CSV file
            DirectoryInfo dir = new DirectoryInfo(sta.InputPath);

            String sLastFileName72Hr = default(DateTime).ToString("yyyyMMddHHmmss") + ".txt";
            if (sta.LastFileTime != default(DateTime))
                sLastFileName72Hr = sta.LastFileTime.AddHours(-72).ToString("yyyyMMddHHmmss") + ".txt";

            // 1.找出最後一筆記錄及之前 72 小時記錄
            FileInfo[] files = dir.GetFiles("*", SearchOption.AllDirectories)
                .Where(x => x.Name.Contains(".csv") && string.Compare(x.Name, sLastFileName72Hr) >= 0)
                .OrderBy(x => x.Name).ToArray();

            // 增加到 staion
            foreach (FileInfo file in files)
            {
                try
                {   // 2.Open the text file using a stream reader.
                    List<dynamic> records;
                    using (var reader = new StreamReader(file.FullName))
                    {
                        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                        {
                            records = csv.GetRecords<dynamic>().ToList();

                            var record = records[0];
                            var obj1 = record as System.Dynamic.ExpandoObject;
                            DateTime tim = default(DateTime);
                            Station1.InputItem dataitem = new Station1.InputItem()
                            {
                                Header = obj1.Select(a => a.Key).ToList(),
                                Line = obj1.Select(a => a.Value.ToString()).ToList()
                            };

                            // 加入 tim
                            DateTime.TryParseExact(dataitem.Line[dataitem.Header.IndexOf("TIM")], "yyyy-MM-ddTHH:mm:ss+08:00", null, System.Globalization.DateTimeStyles.None, out tim);
                            dataitem.TIM = tim;

                            dataitem.FileName = file.Name;

                            sta.InputItems.Add(dataitem);
                        }
                    }
                    // 記錄最後一次檔名
                    sta.NewLastFileName = file.Name;
                }
                catch (IOException err)
                {
                    logView.Add("讀取CGI檔案錯誤: " + file.Name + ", ERROR." + err.Message);
                }
            }

            foreach (Station1.ConfigFile cfg in sta.ConfigFiles)
            {

                logView.Add("統計資料中: " + cfg.Tag);

                // 計算資料
                cfg.Lines = DoAcc(sta, cfg.Tag);

                // 正式寫入檔案 HH
                DoOutputHH(cfg, sta.OutputPath);
                // 正式寫入檔案 month
                DoOutputMonth(cfg, sta.OutputPath);
            }

            // 最新最後資料
            cgiLastFiles.UpdateLastFile(sta.StationName, sta.NewLastFileName);

        }

        private void DoOutputHH(Station1.ConfigFile cfg, string staOutputPath)
        {

            logView.Add("執行寫入HH檔案程式: " + cfg.Tag);

            if (cfg.Lines.Count > 0)
            {

                // 2.正式寫入檔案 HH
                DateTime MinTIM = cfg.Lines.Min(x => x.TIM);
                DateTime MaxTIM = cfg.Lines.Max(x => x.TIM);
                TimeSpan timeSpan = MaxTIM - MinTIM;

                for (int ihour = 0; ihour <= Math.Ceiling((MaxTIM - MinTIM).TotalHours); ihour++)
                {
                    string filename = MinTIM.AddHours(ihour).ToString("yyyyMMddHH") + ".txt";
                    DateTime dtFrom = MinTIM.AddHours(ihour).TruncateToHourStart();
                    DateTime dtTo = MinTIM.AddHours(ihour + 1).TruncateToHourStart();

                    List<List<string>> lines = (cfg.Lines.Where(x => x.TIM >= dtFrom && x.TIM < dtTo).Select(l => l.ColItems)).ToList();

                    if (lines.Count > 0)
                        CSVWriteFile(cfg.Title, lines, staOutputPath + @"\" + cfg.OutputPath + @"\" + filename);

                }
                logView.Add("寫入 HH 檔案完成: " + cfg.Tag);
            }
            else
            {
                logView.Add("無 HH 資料更新: " + cfg.Tag);
            }

        }

        private void DoOutputMonth(Station1.ConfigFile cfg, string staOutputPath)
        {

            logView.Add("執行寫入 Month 檔案程式: " + cfg.Tag);

            if (cfg.Lines.Count > 0)
            {

                // 2.正式寫入檔案 HH
                DateTime MinTIM = cfg.Lines.Min(x => x.TIM);
                DateTime MaxTIM = cfg.Lines.Max(x => x.TIM);
                TimeSpan timeSpan = MaxTIM - MinTIM;

                for (int imonth = 0; imonth <= E1Functions.GetMonthDifference(MaxTIM, MinTIM) ; imonth++)
                {
                    string filename = MinTIM.AddMonths(imonth).ToString("yyyyMM") + ".txt";
                    DateTime dtFrom = MinTIM.AddMonths(imonth).TruncateToMonthStart();
                    DateTime dtTo = MinTIM.AddMonths(imonth + 1).TruncateToMonthStart();

                    List<List<string>> lines = (cfg.Lines.Where(x => x.TIM >= dtFrom && x.TIM < dtTo).Select(l => l.ColItems)).ToList();

                    if (lines.Count > 0)
                        CSVWriteFile(cfg.Title, lines, staOutputPath + @"\" + cfg.OutputPath + @"\" + filename);

                }
                logView.Add("寫入 Month 檔案完成: " + cfg.Tag);
            }
            else
            {
                logView.Add("無 Month 資料更新: " + cfg.Tag);
            }

        }

        private static List<Station1.ConfigFile.Line> DoAcc(Station1 sta, string tag)
        {
            List<Station1.InputItem> processInputItem = sta.InputItems.Where(x => string.Compare(x.FileName, sta.LastFileName) > 0).ToList();
            Station1.ConfigFile cfg = sta.ConfigFiles.Where(x => x.Tag == tag).FirstOrDefault();
            cfg.Lines.Clear(); // 先清空, 避免累積

            // 無資料返回
            if (cfg == null)
                return null;

            // 1.讀取資料
            foreach (Station1.InputItem inputitem in processInputItem)
            {
                List<string> outputline = new List<string>();
                DateTime tim = default(DateTime);
                // 處理 TIM
                if (inputitem.Header.IndexOf("TIM") != -1)
                {
                    DateTime.TryParseExact(inputitem.Line[inputitem.Header.IndexOf("TIM")], "yyyy-MM-ddTHH:mm:ss+08:00", null, System.Globalization.DateTimeStyles.None, out tim);
                    cfg.CurrentTIM = tim;
                }

                foreach (string col in cfg.Col)
                {
                    int colIndex = inputitem.Header.IndexOf(col);
                    string colData = "";

                    // 處理特殊欄位
                    if (col == "TIMD")
                    {
                        colIndex = inputitem.Header.IndexOf("TIM");
                        colData = inputitem.Line[colIndex].Substring(0, 10);
                    }
                    else if (col == "TIMT")
                    {
                        colIndex = inputitem.Header.IndexOf("TIM");
                        colData = inputitem.Line[colIndex].Substring(11, 8);
                    }
                    else if (col == "TIM")
                    {
                        // 特別客製, TIM 產生二個欄位
                        colIndex = inputitem.Header.IndexOf("TIM");
                        colData = inputitem.Line[colIndex].Substring(0, 10);
                        outputline.Add(colData);

                        colData = inputitem.Line[colIndex].Substring(11, 8);
                    }
                    else if (colIndex == -1)
                    {
                        // 關鍵字 calc() 為
                        string exp = Regex.Match(col, @"(?<=calc\()(.*)(?=\))").Groups[1].Value;

                        if (string.IsNullOrEmpty(exp))
                        {   // 輸入文字，直接輸入
                            colData = col;
                        }
                        else
                        {
                            // 檢查是否為 acc, 得到 #筆數
                            bool isAcc = (Regex.Match(col, @"(?<=acc\()(.*)(?=\))").Groups[1].Value != "");
                            int countAcc = 0;
                            if (isAcc)
                            {
                                countAcc = int.TryParse(Regex.Match(col, @"(?<=#)([^\)]*)").Groups[1].Value, out countAcc) ? countAcc : 0;
                                exp = exp.Replace("#" + countAcc.ToString(), "");
                            }

                            // init 計算式   
                            Expression e = new Expression(exp, EvaluateOptions.NoCache);

                            // 處理參數
                            e.EvaluateParameter += delegate (string name, ParameterArgs args)
                            {
                                int _colIndex = inputitem.Header.IndexOf(name);
                                if (_colIndex != -1)
                                {
                                    args.Result = inputitem.Line[_colIndex].Trim();
                                }
                            };

                            // 處理客制 function
                            e.EvaluateFunction += delegate (string name, FunctionArgs args)
                            {
                                switch (name)
                                {
                                    case "diff":
                                        //args.Result = Diff(pe, sta.DataItems, args.Parameters[0]);
                                        DateTime currentTIM = cfg.CurrentTIM;
                                        List<Station1.InputItem> lastrawdata = sta.InputItems.Where(x => x.TIM <= currentTIM).Select(s => s).OrderByDescending(t => t.TIM).Take(2).ToList();
                                        Station1.InputItem lastDataItem = lastrawdata.FirstOrDefault();
                                        args.Result = Diff(lastrawdata, args.Parameters[0], currentTIM);
                                        break;
                                    case "acc":
                                        args.Result = Acc(cfg, sta.InputItems, args.Parameters[0], countAcc);
                                        break;
                                }

                            };

                            // 計算式，數字去尾數 0.100 -> 0.1
                            colData = DoExpressionEvaluate(e);
                        }
                    }
                    else
                    { // 一般取值，不計算
                        colData = inputitem.Line[colIndex].Trim();
                    }

                    outputline.Add(colData);
                }

                if (outputline.Count > 0)
                {
                    cfg.Lines.Add(new Station1.ConfigFile.Line { TIM = tim, ColItems = outputline });
                }
            }

            return cfg.Lines;
        }
        private void CSVInput(Station sta)
        {
            logView.Add("讀取資料.");
            
            // 讀取 CSV file
            DirectoryInfo dir = new DirectoryInfo(sta.InputPath);

            String sLastFileName72Hr = default(DateTime).ToString("yyyyMMddHHmmss") + ".txt";
            if (sta.lastFileTime != default(DateTime))
                sLastFileName72Hr = sta.lastFileTime.AddHours(-72).ToString("yyyyMMddHHmmss") + ".txt";

            // 1.找出最後一筆記錄及之前 72 小時記錄
            FileInfo[] files = dir.GetFiles("*", SearchOption.AllDirectories)
                .Where(x => x.Name.Contains(".csv") && string.Compare(x.Name, sLastFileName72Hr) >= 0)
                .OrderBy(x => x.Name).ToArray();

            // 增加到 staion
            foreach (FileInfo file in files)
            {
                try
                {   // 2.Open the text file using a stream reader.
                    List<dynamic> records;
                    using (var reader = new StreamReader(file.FullName))
                    {
                        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                        {
                            records = csv.GetRecords<dynamic>().ToList();

                            var record = records[0];
                            var obj1 = record as System.Dynamic.ExpandoObject;
                            DateTime tim = default(DateTime);
                            Station.DataItem dataitem = new Station.DataItem()
                            {
                                Header = obj1.Select(a => a.Key).ToList(),
                                Line = obj1.Select(a => a.Value.ToString()).ToList()
                            };                           

                            // 加入 tim
                            DateTime.TryParseExact(dataitem.Line[dataitem.Header.IndexOf("TIM")], "yyyy-MM-ddTHH:mm:ss+08:00", null, System.Globalization.DateTimeStyles.None, out tim);
                            dataitem.TIM = tim;

                            dataitem.FileName = file.Name;

                            sta.DataItems.Add(dataitem);
                        }
                    }
                    // 記錄最後一次檔名
                    sta.newLastFileName = file.Name;
                }
                catch (IOException err)
                {
                    logView.Add("讀取CGI檔案錯誤: " + file.Name + ", ERROR." + err.Message);
                }
            }

            PE pe = new PE();
            pe.Tag = "E1_TEST_OW";

            //pe.Titles = new string[] { "編號", "日期", "時間", "雨量刻度", "10分鐘雨量", "每小時雨量", "3小時累積雨量", "6小時累積雨量", "12小時累積雨量", "24小時累積雨量", "48小時累積雨量", "72小時累積雨量" };            
            //pe.Cols = new string[] { "TEST_001", "TIMD", "TIMT", "DI_0 Cnt", @"calc(diff([DI_0 Cnt]))", @"calc(acc(diff([DI_0 Cnt])#6))", @"calc(acc(diff([DI_0 Cnt])#18))", @"calc(acc(diff([DI_0 Cnt])#36))", @"calc(acc(diff([DI_0 Cnt])#72))", @"calc(acc(diff([DI_0 Cnt])#144))", @"calc(acc(diff([DI_0 Cnt])#288))", @"calc(acc(diff([DI_0 Cnt])#432))" };

            pe.Titles = sTitles.Split(',').Select(p => p.Trim()).ToArray();
            pe.Cols = sCols.Split(',').Select(p => p.Trim()).ToArray();

            string[] processCols = pe.Cols;

            logView.Add("統計資料中...");

            List<Station.DataItem> processDataItem = sta.DataItems.Where(x => string.Compare(x.FileName, sta.lastFileName) > 0).ToList();
            // 1.讀取資料
            foreach (Station.DataItem dataitem in processDataItem)
            {
                List<string> _outline = new List<string>();
                DateTime tim = default(DateTime);
                // 處理 TIM
                if (dataitem.Header.IndexOf("TIM") != -1)
                {
                    DateTime.TryParseExact(dataitem.Line[dataitem.Header.IndexOf("TIM")], "yyyy-MM-ddTHH:mm:ss+08:00", null, System.Globalization.DateTimeStyles.None, out tim);
                    pe.CurrentTIM = tim;
                }

                foreach (string col in processCols)
                {
                    int colIndex = 0;
                    string colData = "";
                    colIndex = dataitem.Header.IndexOf(col);
                    // 處理特殊欄位
                    if (col == "TIMD")
                    {
                        colIndex = dataitem.Header.IndexOf("TIM");
                        colData = dataitem.Line[colIndex].Substring(0, 10);
                    }
                    else if (col == "TIMT")
                    {
                        colIndex = dataitem.Header.IndexOf("TIM");
                        colData = dataitem.Line[colIndex].Substring(11, 8);
                    }
                    else if (colIndex == -1)
                    {
                        // 關鍵字 calc() 為
                        string exp = Regex.Match(col, @"(?<=calc\()(.*)(?=\))").Groups[1].Value;

                        if (string.IsNullOrEmpty(exp))
                        {   // 輸入文字，直接輸入
                            colData = col;
                        }
                        else
                        {

                            // 檢查是否為 acc, 得到 #筆數
                            bool isAcc = (Regex.Match(col, @"(?<=acc\()(.*)(?=\))").Groups[1].Value != "");
                            int countAcc = 0;
                            if (isAcc)
                            {
                                countAcc = int.TryParse(Regex.Match(col, @"(?<=#)([^\)]*)").Groups[1].Value, out countAcc) ? countAcc : 0;
                                exp = exp.Replace("#" + countAcc.ToString(), "");
                            }

                            // init 計算式   
                            Expression e = new Expression(exp, EvaluateOptions.NoCache);

                            // 處理參數
                            e.EvaluateParameter += delegate (string name, ParameterArgs args)
                            {
                                int _colIndex = dataitem.Header.IndexOf(name);
                                if (_colIndex != -1)
                                {
                                    args.Result = dataitem.Line[_colIndex].Trim(); 
                                }
                            };

                            // 處理客制 function
                            e.EvaluateFunction += delegate (string name, FunctionArgs args)
                            {
                                switch (name)
                                {
                                    case "diff":
                                        //args.Result = Diff(pe, sta.DataItems, args.Parameters[0]);
                                        DateTime currentTIM = pe.CurrentTIM;
                                        List<Station.DataItem> lastrawdata = sta.DataItems.Where(x => x.TIM <= currentTIM).Select(s => s).OrderByDescending(t => t.TIM).Take(2).ToList();
                                        Station.DataItem lastDataItem = lastrawdata.FirstOrDefault();
                                        args.Result = Diff(lastrawdata, args.Parameters[0], currentTIM);
                                        break;
                                    case "acc":
                                        args.Result = Acc(pe, sta.DataItems, args.Parameters[0], countAcc);
                                        break;
                                }

                            };

                            // 計算式，數字去尾數 0.100 -> 0.1
                            colData = DoExpressionEvaluate(e);
                        }
                    }
                    else
                    { // 一般取值，不計算
                        colData = dataitem.Line[colIndex].Trim();
                    }

                    _outline.Add(colData);
                }

                if (_outline.Count > 0)
                {
                    pe.Lines.Add(new PE.Line { TIM = tim, ColItems = _outline });
                }
            }

            logView.Add("開始寫入HH檔案...");

            if( pe.Lines.Count > 0 )
            {

                // 2.正式寫入檔案 HH
                DateTime MinTIM = pe.Lines.Min(x => x.TIM);
                DateTime MaxTIM = pe.Lines.Max(x => x.TIM);
                TimeSpan timeSpan = MaxTIM - MinTIM;

                for (int ihour = 0; ihour <= Math.Ceiling((MaxTIM - MinTIM).TotalHours); ihour++)
                {
                    string filename = MinTIM.AddHours(ihour).ToString("yyyyMMddHH") + ".txt";
                    DateTime dtFrom = MinTIM.AddHours(ihour).TruncateToHourStart();
                    DateTime dtTo = MinTIM.AddHours(ihour + 1).TruncateToHourStart();

                    List<List<string>> lines = (pe.Lines.Where(x => x.TIM >= dtFrom && x.TIM < dtTo).Select(l => l.ColItems)).ToList();

                    if (lines.Count > 0)
                        CSVWriteFile(pe.Titles, lines, sta.OutputPath + @"\" + filename);

                }
                logView.Add("寫入 HH 檔案完成.");
            }
            else
            {
                logView.Add("無 HH 資料更新. ");
            }

            // 最新最後資料
            cgiLastFiles.UpdateLastFile(sta.Tag, sta.newLastFileName);

            // 正式寫入檔案 month

        }
        public static object Diffv1(PE pe, List<Station.DataItem> rawdata, Expression expression)
        {
            var result = 0;
            // 取得前一筆資料
            Station.DataItem lastDataItem = rawdata.Where(x=> x.TIM < pe.CurrentTIM).OrderByDescending(t=>t.TIM).FirstOrDefault();

            if(lastDataItem != null)
            {
                int lastColIndex = lastDataItem.Header.IndexOf(expression.ParsedExpression.ToString().Replace("[", "").Replace("]", ""));
                // 取得欄位 Index 
                if(lastColIndex!=-1)
                {
                    decimal lastColNum = decimal.TryParse(lastDataItem.Line[lastColIndex], out lastColNum) ? lastColNum : 0;
                    decimal thisColNum = decimal.TryParse(expression.Evaluate().ToString(), out thisColNum) ? thisColNum : 0;

                    // 如果前一筆大於此筆則不相減
                    if (lastColNum > thisColNum)
                        lastColNum = 0;

                    return thisColNum - lastColNum;
                }
            }
            else
            {
                return result;
            }

            return result;

        }

        public static object Diff(List<Station.DataItem> rawdata, Expression expression, DateTime currentTIM)
        {
            decimal totalAcc = 0;
            Station.DataItem lastDataItem = rawdata.FirstOrDefault();

            expression.EvaluateParameter += delegate (string name, ParameterArgs args)
            {

                foreach (Station.DataItem _rawdata in rawdata)
                {
                    if (currentTIM != default(DateTime))
                    {
                        Station.DataItem thisDataItem = _rawdata;

                        int thisColIndex = thisDataItem.Header.IndexOf(name);
                        int lastColIndex = lastDataItem.Header.IndexOf(name);

                        if (lastColIndex != -1)
                        {
                            decimal lastColNum = decimal.TryParse(thisDataItem.Line[lastColIndex], out lastColNum) ? lastColNum : 0;
                            decimal thisColNum = decimal.TryParse(lastDataItem.Line[thisColIndex], out thisColNum) ? thisColNum : 0;

                            // 如果前一筆大於此筆則不相減
                            if (lastColNum > thisColNum)
                                lastColNum = 0;

                            totalAcc = totalAcc + (thisColNum - lastColNum);
                        }

                        lastDataItem = thisDataItem;
                    }
                }

                args.Result = totalAcc;
            };

            return expression.Evaluate();

        }
        public static object Acc(Station1.ConfigFile cfg, List<Station1.InputItem> rawdata, Expression expression, int CountAcc)
        {
            var result = CountAcc;

            // init 計算式   
            Expression _e = new Expression(expression.ParsedExpression, EvaluateOptions.NoCache);
            DateTime currentTIM = cfg.CurrentTIM;

            List<Station1.InputItem> lastrawdata = rawdata.Where(x => x.TIM <= currentTIM).Select(s => s).OrderByDescending(t => t.TIM).Take(CountAcc + 1).ToList();
            Station1.InputItem lastDataItem = lastrawdata.FirstOrDefault();

            // 處理客制 function
            _e.EvaluateFunction += delegate (string name, FunctionArgs args)
            {
                switch (name)
                {
                    case "diff":
                        args.Result = Diff(lastrawdata, args.Parameters[0], currentTIM);
                        break;
                }
            };

            return _e.Evaluate();

        }
        public static object Diff(List<Station1.InputItem> rawdata, Expression expression, DateTime currentTIM)
        {
            decimal totalAcc = 0;
            Station1.InputItem lastDataItem = rawdata.FirstOrDefault();

            expression.EvaluateParameter += delegate (string name, ParameterArgs args)
            {

                foreach (Station1.InputItem _rawdata in rawdata)
                {
                    if (currentTIM != default(DateTime))
                    {
                        Station1.InputItem thisDataItem = _rawdata;

                        int thisColIndex = thisDataItem.Header.IndexOf(name);
                        int lastColIndex = lastDataItem.Header.IndexOf(name);

                        if (lastColIndex != -1)
                        {
                            decimal lastColNum = decimal.TryParse(thisDataItem.Line[lastColIndex], out lastColNum) ? lastColNum : 0;
                            decimal thisColNum = decimal.TryParse(lastDataItem.Line[thisColIndex], out thisColNum) ? thisColNum : 0;

                            // 如果前一筆大於此筆則不相減
                            if(lastColNum > thisColNum)
                                lastColNum = 0;
                                                            
                            totalAcc = totalAcc + (thisColNum - lastColNum);
                        }

                        lastDataItem = thisDataItem;
                    }
                }

                args.Result = totalAcc;
            };

            return expression.Evaluate();

        }
        public static object Acc(PE pe, List<Station.DataItem> rawdata, Expression expression, int CountAcc)
        {
            var result = CountAcc;

            // init 計算式   
            Expression _e = new Expression(expression.ParsedExpression, EvaluateOptions.NoCache);
            DateTime currentTIM = pe.CurrentTIM;

            List<Station.DataItem> lastrawdata = rawdata.Where(x => x.TIM <= currentTIM).Select(s => s).OrderByDescending(t => t.TIM).Take(CountAcc+1).ToList();
            Station.DataItem lastDataItem = lastrawdata.FirstOrDefault();

            // 處理客制 function
            _e.EvaluateFunction += delegate (string name, FunctionArgs args)
            {
                switch (name)
                {
                    case "diff":                   
                        args.Result = Diff(lastrawdata, args.Parameters[0], currentTIM);
                        break;
                }
            };

            return _e.Evaluate();

        }
        private static string DoExpressionEvaluate(Expression e)
        {
            string colData;
            decimal decimalNumber;
            if (decimal.TryParse(e.Evaluate().ToString(), out decimalNumber))
                colData = decimalNumber.DecimalToString();
            else
                colData = e.Evaluate().ToString();
            return colData;
        }
        private static void CSVWriteFile(string[] Titles, List<List<string>> Lines, string FilePathName)
        {
            // 檢查檔案是否存在
            bool isNotExists = !File.Exists(FilePathName);
            // 檢查目錄
            if (!Directory.Exists(Path.GetDirectoryName(FilePathName)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePathName));
            }

            using (var writer = new StreamWriter(FilePathName, true))
            using (var csv = new CsvSerializer(writer, CultureInfo.InvariantCulture))
            {
                {
                    if (isNotExists)
                    {
                        csv.Write(Titles);
                    }
                    
                    foreach (var record in Lines)
                    {
                        csv.WriteLine();
                        csv.Write(record.ToArray());
                    }
                }
            }
            // 檢查無 lock 則改名 .tmp -> .txt 避免 lock 造成 web 無法讀檔

        }

    }
    public class Station
    {
        int SleepMins { get; set; }
        public string Tag { get; set; }
        public string InputPath { get; set; }
        public string OutputPath { get; set; }
        public int ProcessMode { get; set; }
        public List<DataItem> DataItems { get; set; }
        public List<PE> PEs { get; set; }
        public string lastFileName { get; set; }
        public string newLastFileName { get; set; }
        public DateTime lastFileTime
        {
            get 
            {
                
                DateTime _lastFileTime = DateTime.TryParseExact(Path.GetFileNameWithoutExtension(lastFileName), "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out _lastFileTime) ? _lastFileTime : default(DateTime);

                return _lastFileTime;
            }
        }
        public Station()
        {
            DataItems = new List<DataItem>();
        }

        public class DataItem
        {
            public string FileName { get; set; }
            public DateTime TIM { get; set; }
            public List<string> Header { get; set; }
            public List<string> Line { get; set; }
        }
    }
    public class PE
    {
        public string Tag { get; set; }
        public string Name { get; set; }
        public string OutputPath { get; set; }
        public string[] Titles { get; set; }
        public string[] Cols { get; set; }
        public List<Line> Lines { get; set; }
        public DateTime CurrentTIM { get; set; }
        public string content { get; set; }
        public PE()
        {
            Lines = new List<Line>();
        }
        public class Line
        {
            public DateTime TIM { get; set; }
            public List<string> ColItems { get; set; }
        }
    }
    public class LogView
    {
        public List<string> LogList { get; set; }
        public void Add(string Msg)
        {
            LogList.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + Msg);
        }
        public LogView()
        {
            LogList = new List<string>();
        }
    }

    public class LastFiles
    {
        public Dictionary<string, string> DataItems { get; set; }

        public void UpdateLastFile(string Key, string Value)
        {            
            E1Extentions.AppSetting("LastInputFile:" + Key, Value);
        }
        public void Reload()
        {
            DataItems = ConfigurationManager.AppSettings.AllKeys
                .Where(key => key.Contains("LastInputFile:"))
                .ToDictionary(k => k.Replace("LastInputFile:", ""), v => ConfigurationManager.AppSettings[v]);
        }

        public LastFiles()
        {
            Reload();
        }

    }

}

