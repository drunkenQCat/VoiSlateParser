using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using ATL;
using VideoTimecode;
using ATL.AudioData;
using Microsoft.VisualBasic.Logging;
using VoiSlateParser.Models;
using VoiSlateParser.Utilities;

namespace VoiSlateParser.Helper;

class FileLoadingHelper
{
    public static FileLoadingHelper Instance = new FileLoadingHelper();

    public List<FileInfo> WavList = new();

    public List<SlateLogItem> LogList = new();

    public void GetBwf(string folderPath) //method to list all .wav files in given folder
    {
        DirectoryInfo directory = new DirectoryInfo(folderPath); //create directory object for given folder path
        FileInfo[] files = directory.GetFiles("*.wav", SearchOption.AllDirectories); //get all .wav files in directory
        
        foreach (FileInfo file in files) //loop through each .wav file
        {
            WavList.Add(file); //add file to list of .wav files
        }
        MappingInfo();
    }
    public void GetLogs(string jsonPath) //method to list all .wav files in given folder
    {
        LogList.Clear();
        string jsonText = File.ReadAllText(jsonPath);
        var list = Newtonsoft.Json.Linq.JArray.Parse(jsonText);
        foreach (var item in list)
        {

            string scn = item["scn"]!.ToString();
            string sht = item["sht"]!.ToString();
            int tk = int.Parse(item["tk"]!.ToString());
            string filenamePrefix = item["filenamePrefix"]!.ToString();
            string filenameLinker = item["filenameLinker"]!.ToString();
            int filenameNum = int.Parse(item["filenameNum"]!.ToString());

            string tkNote = item["tkNote"]!.ToString();
            string shtNote = item["shtNote"]!.ToString();
            string scnNote = item["scnNote"]!.ToString();
            TkStatus okTk = (TkStatus)Enum.Parse(typeof(TkStatus), item["okTk"]!.ToString());
            ShtStatus okSht = (ShtStatus)Enum.Parse(typeof(ShtStatus), item["okSht"]!.ToString());

            SlateLogItem newLogItem = new SlateLogItem(
                scn,
                sht,
                tk,
                filenamePrefix,
                filenameLinker,
                filenameNum,
                tkNote,
                shtNote,
                scnNote,
                okTk,
                okSht);
            LogList.Add(newLogItem);
        }
        MappingInfo();

    }

    void MappingInfo()
    {
        if (LogList.Count == 0 || WavList.Count == 0)
            return;
        foreach (var item in LogList)
        {
            var name = item.fileName;
            var query =
                from info in WavList
                where info.Name.Contains(name)
                orderby info.Name
                select info;
            var files = query.ToList();
            item.bwfList = files;
            Timecode invalidTime = new(0, FrameRate.FrameRate24);
            try
            {
                Track bwf = new Track(files[0].FullName);
                BwfTimeCode thisBwfTC = new(bwf);
                item.startTc = thisBwfTC.StartTc??invalidTime;
                item.endTc = thisBwfTC.EndTc??invalidTime;
                item.ubits = thisBwfTC.Ubits;
                item.bwfSynced = true;
            }
            catch
            {
                item.bwfSynced = false;
            }
        }
    }
}