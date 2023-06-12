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
    public List<FileInfo> videoList { get; set; }

    public bool bwfSynced { get; set; }
    public bool videoSynced { get; set; }
}