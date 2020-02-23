// ===============================
// @AUTHOR      : 윤정도
// @CREATE DATE : 2020-02-22 오후 12:54:34   
// @PURPOSE     : 윈도우 세팅
// ===============================


using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PClient
{
    public static class Setting
    {
        private static string _P2PStartPath = Environment.GetFolderPath(Environment.SpecialFolder.System);
        public static string P2PStartPath   // property
        {
            get
            {
                lock (settingLock)
                    return _P2PStartPath;
            }
            set
            {
                lock (settingLock)
                    _P2PStartPath = value;
            }
        }

        private static string _P2PDownloadPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\P2PShare"; 
        public static string P2PDownloadPath   
        {
            get
            {
                lock (settingLock)
                    return _P2PDownloadPath;
            }
            set
            {
                lock (settingLock)
                    _P2PDownloadPath = value;
            }
        }


      

        private static object settingLock = new object();
        private static readonly string settingFilePath = "Setting.json";


        public static void Save()
        {
            JObject jObject = new JObject();
            jObject.Add("StartDirectory", P2PStartPath);
            jObject.Add("DownloadDirectory", P2PDownloadPath);
            File.WriteAllText(settingFilePath, jObject.ToString());
        }

        public static void Load()
        {
            if (File.Exists(settingFilePath) == false)
                return;

            try
            {
                JObject jObject = JObject.Parse(File.ReadAllText(settingFilePath));
                P2PStartPath = jObject["StartDirectory"].ToString();
                P2PDownloadPath = jObject["DownloadDirectory"].ToString();
            }
            catch
            {
                WindowLogger.WriteLineError("세팅파일을 불러오는데 실패하였습니다.");
            }
        }
    }
}
