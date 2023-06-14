using System.Collections.Generic;
using System.IO;
using VideoTimecode;
// ReSharper disable InconsistentNaming

namespace VoiSlateParser.Models;

public enum TkStatus
{
    notChecked = 0,
    ok = 1,
    bad = 2
}

public enum ShtStatus
{
    notChecked = 0,
    ok = 1,
    nice = 2
}

public class SlateLogItem
{
    public SlateLogItem(string scn,
        string sht,
        int tk,
        string filenamePrefix,
        string filenameLinker,
        int filenameNum,

        string tkNote,
        string shtNote,
        string scnNote,
        TkStatus okTk = TkStatus.notChecked,
        ShtStatus okSht = ShtStatus.notChecked)
    {
        this.scn = scn;
        this.sht = sht;
        this.tk = tk;
        this.filenamePrefix = filenamePrefix;
        this.filenameLinker = filenameLinker;
        this.filenameNum = filenameNum;
        this.tkNote = tkNote;
        this.shtNote = shtNote;
        this.scnNote = scnNote;
        this.okTk = okTk;
        this.okSht = okSht;
        this.bwfSynced = false;
    }

    public string fileName => filenamePrefix + filenameLinker + filenameNum.ToString();
    public string scn { get; set; }
    public string sht { get; set; }
    public int tk { get; set; }
    public string filenamePrefix { get; set; }
    public string filenameLinker { get; set; }
    public int filenameNum { get; set; }
    public string tkNote { get; set; }
    public string shtNote { get; set; }
    public string scnNote { get; set; }
    public TkStatus okTk { get; set; }
    public ShtStatus okSht { get; set; }
    
    public Timecode startTc { get; set; }
    public Timecode endTc { get; set; }
    
    public Timecode fileLength { get; set; }
    public List<FileInfo> bwfList { get; set; }
    public List<string> videoList { get; set; }
    public string ubits { get; set; }

    public bool bwfSynced { get; set; }
    public bool videoSynced { get; set; }

    public bool Contains(string filterText)
    {
        if (scn.Contains(filterText)) return true;
        if (sht.Contains(filterText)) return true;
        if (tkNote.Contains(filterText)) return true;
        if (shtNote.Contains(filterText)) return true;
        if (scnNote.Contains(filterText)) return true;
        if (fileName.Contains(filterText)) return true;
        if (bwfList != null)
        {
            if (BwfContainsFileter(filterText)) return true;
        }
        if (videoList != null)
        {
            if (VideoContainsFileter(filterText)) return true;
        }
        return false;
    }

    private bool VideoContainsFileter(string filterText)
    {
        // find if any items in videoList contains fileterText
        // if so, return true, else false.
        // if videoList is null, return true.
        // if videoList is empty, return true.
        // if videoList is not empty, return true if any item contains filterText.

        if (videoList == null)
        {
            {
                return true;
            }
        }

        if (videoList != null)
        {
            foreach (var video in videoList)
            {
                if (video.Contains(filterText))
                {
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private bool BwfContainsFileter(string filterText)
    {
    // a function to find if any items in bwfList.Name contains fileterText
    // if so, return true, else false.
    // if bwfList is null, return true.
    // if bwfList is empty, return true.
    // if bwfList is not empty, return true if any item contains filterText.
        if (bwfList == null)
        {
            {
                return true;
            }
        }

        if (bwfList != null)
        {
            foreach (var bwf in bwfList)
            {
                if (bwf.Name.Contains(filterText))
                {
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }


}