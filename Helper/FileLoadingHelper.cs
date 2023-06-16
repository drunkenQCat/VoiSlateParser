using System.Collections.Generic;
using System;
using System.Data;
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

    public AlePaser Ale;
    private EnumerableRowCollection<DataRow> _queryVideo;
    private EnumerableRowCollection<DataRow> _queryBwf;
    private EnumerableRowCollection<DataRow> _queryTimeline;

    public void GetBwf(string folderPath) //method to list all .wav files in given folder
    {
        DirectoryInfo directory = new DirectoryInfo(folderPath); //create directory object for given folder path
        FileInfo[] files = directory.GetFiles("*.wav", SearchOption.AllDirectories); //get all .wav files in directory
        
        foreach (FileInfo file in files) //loop through each .wav file
        {
            WavList.Add(file); //add file to list of .wav files
        }
        MappingRecordFileInfo();
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
        MappingRecordFileInfo();
        MapppingAleInfo();

    }

    void MappingRecordFileInfo()
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

    void MapppingAleInfo()
    {
        if (LogList.Count == 0 || Ale == null)
        return;
        foreach (var item in LogList)
        {
            BwfTimeCode bwfTime = new(item.startTc, item.endTc);
            var query = _queryVideo.Where(info =>
            {
                var startTime = info.Field<string>("Start TC");
                var endTime = info.Field<string>("End TC");
                BwfTimeCode videoTime = new BwfTimeCode(startTime, endTime, bwfTime.FramRate);
                return Comparor.IsTimeCrossed(bwfTime, videoTime);
            });
            item.videoList = query;
        }
    }   

    public void WriteMetaData()
    {
        foreach (var item in LogList)
        {
            foreach (var bwf in item.bwfList)
            {
                Track tr = new(bwf.FullName);
                tr.AdditionalFields["ixml.SCENE"] = item.scn + "-" + item.sht;
                tr.AdditionalFields["ixml.TAKE"] = item.tk.ToString();
                tr.AdditionalFields["ixml.NOTE"] = item.scnNote + "," + item.shtNote;
                tr.AdditionalFields["ixml.CIRCLED"] = (item.okTk == TkStatus.ok) ? "TRUE" : "FALSE";
                tr.AdditionalFields["ixml.TAKE_TYPE"] = (item.okTk == TkStatus.bad) ? "NO_GOOD" : "";
                tr.AdditionalFields["ixml.WILD_TRACK"] = (item.tkNote.Contains("wild")) ? "TRUE" : "FALSE";
                tr.AdditionalFields["bext.description"] = item.tkNote;
                tr.SaveAsync();
            }
        }
    }

    public void WriteAleData(string path)
    {
        if (!Ale.dt.Columns.Contains("Flags")) Ale.dt.Columns.Add("Flags");
        if (!Ale.dt.Columns.Contains("Good Take")) Ale.dt.Columns.Add("Good Take");
        if (!Ale.dt.Columns.Contains("Description")) Ale.dt.Columns.Add("Description");
        if (!Ale.dt.Columns.Contains("Scene")) Ale.dt.Columns.Add("Scene");
        if (!Ale.dt.Columns.Contains("Shot")) Ale.dt.Columns.Add("Shot");
        if (!Ale.dt.Columns.Contains("Take")) Ale.dt.Columns.Add("Take");
        if (!Ale.dt.Columns.Contains("Environment")) Ale.dt.Columns.Add("Environment");
        foreach (var item in LogList)
        {
            foreach (var video in item.videoList)
            {
                video["Scene"] = item.scn;
                video["Shot"] = item.sht;
                video["Take"] = item.tk;
                video["Flags"] = (item.okSht == ShtStatus.notChecked) ? "" 
                                    :(item.okSht == ShtStatus.nice) ? "Blue"
                                    : "Green";
                video["Good Take"] = (item.okSht == ShtStatus.notChecked) ? "":"1";
                video["Description"] = item.shtNote;
                video["Environment"] = item.scnNote;
            }
        }
        Ale.dt.AcceptChanges();
        path = path + "output." + Ale.parserType;
        Ale.WriteAle(path);
    }

    public void GetAle(string path)
    {
        FileInfo ale = new(path);
        Ale = new AlePaser(ale);
        // linq, fild the "resolution" in dt, if it is not empty, "FileType" equal to "A"
        _queryTimeline = from row in Ale.dt.AsEnumerable()
            where row.Field<string>("Resolution") != "" 
                  && row.Field<string>("Clip Directory") == ""
            select row;
        _queryBwf = from row in Ale.dt.AsEnumerable()
            where row.Field<string>("Resolution") == "" 
                  && row.Field<string>("File Name").Contains(".wav")
            select row;
        _queryVideo = from row in Ale.dt.AsEnumerable()
            where row.Field<string>("Resolution") != ""
                  && row.Field<string>("Clip Directory") != ""
            orderby row.Field<string>("Start TC")
            select row;
        Ale.dt.AcceptChanges();
        MapppingAleInfo();
    }
}