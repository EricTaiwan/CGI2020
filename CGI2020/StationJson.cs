using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CGI2020
{
    public partial class StationSetting
    {

        public class jsonData
        {
            public int ScanSecond { get; set; }
            public int ScanStartMinute { get; set; }
            public int AddEndum { get; set; }
            public string InputPath { get; set; }
            public string OutputPath { get; set; }
            public string ProcessMode { get; set; }
            public CustCols custcols { get; set; }
            public List<ConfigFiles> configfiles { get; set; }
        }
        public class CustCols
        {
            public string station { get; set; }
            public string BBB { get; set; }
        }
        public class ConfigFiles
        {
            public string Tag { get; set; }
            public string InputFile { get; set; }
            public string OutputPath { get; set; }
            public string Titles { get; set; }
            public string Cols { get; set; }
        }

        public List<jsonData> GetStatonPIEs(string jsonFilePath)
        {
            // 1. 讀取路徑內檔案
            DirectoryInfo dir = new DirectoryInfo(jsonFilePath);
            FileInfo[] files = dir.GetFiles("*", SearchOption.AllDirectories);

            List<jsonData> stationjasonData = new List<jsonData>();

            // 2. 讀取 staion 資料 
            foreach (FileInfo file in files)
            {
                // 2. read 所有設定
                using (StreamReader r = new StreamReader(file.FullName))
                {
                    string json2 = r.ReadToEnd();
                    jsonData items2 = JsonConvert.DeserializeObject<jsonData>(json2);
                    stationjasonData.Add(items2);
                }
            }

            return stationjasonData;

        }
    }



    public class StationConfig
    {
        public List<Station1> Stations { get; set; }
        public void GetStatonPIEs(string jsonFilePath)
        {
            // 1. 讀取路徑內檔案
            DirectoryInfo dir = new DirectoryInfo(jsonFilePath);
            FileInfo[] files = dir.GetFiles("*", SearchOption.AllDirectories);
            
            Stations = new List<Station1>();

            // 2. 讀取 staion 資料 
            foreach (FileInfo file in files)
            {
                // 2. read 所有設定
                using (StreamReader r = new StreamReader(file.FullName))
                {
                    string json2 = r.ReadToEnd();
                    Station1 items2 = JsonConvert.DeserializeObject<Station1>(json2);
                    Stations.Add(items2);
                }
            }
        }

        public StationConfig(String jsonFilePath)
        {
            GetStatonPIEs(jsonFilePath);            
        }
    }

    public class Station1
    {        
        public string StationName { get; set; }
        public int SleepMins { get; set; }
        public string InputPath { get; set; }
        public string OutputPath { get; set; }
        public int ProcessMode { get; set; }
        public List<ConfigFile> ConfigFiles { get; set; }
        public List<InputItem> InputItems { get; set; }
        public List<PE> PEs { get; set; }
        public string LastFileName { get; set; }
        public string NewLastFileName { get; set; }
        public DateTime LastFileTime
        {
            get
            {

                DateTime _lastFileTime = DateTime.TryParseExact(Path.GetFileNameWithoutExtension(LastFileName), "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out _lastFileTime) ? _lastFileTime : default(DateTime);

                return _lastFileTime;
            }
        }
        public Station1()
        {
            ConfigFiles = new List<ConfigFile>();
            InputItems = new List<InputItem>();
        }
        public class ConfigFile
        {
            public string Tag { get; set; }
            public string OutputPath { get; set; }
            public string Titles { get; set; }
            public string Cols { get; set; }
            public string[] Title 
            {
                get { return Titles.Split(',').Select(p => p.Trim()).ToArray(); }
            }
            public string[] Col
            {
                get { return Cols.Split(',').Select(p => p.Trim()).ToArray(); }
            }
            public DateTime CurrentTIM { get; set; }
            public List<Line> Lines { get; set; }
            public ConfigFile()
            {
                Lines = new List<Line>();
            }
            public class Line
            {
                public DateTime TIM { get; set; }
                public List<string> ColItems { get; set; }
            }

        }
        public class InputItem
        {
            public string FileName { get; set; }
            public DateTime TIM { get; set; }
            public List<string> Header { get; set; }
            public List<string> Line { get; set; }
        }

    }
}
