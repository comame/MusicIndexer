using System;
using System.Collections.Generic;

namespace MusicManager.Logic;

internal class  MusicTrack {
    // タグ
    public string Name = "";
    public string AlbumArtist = "";
    public string AlbumTitle = "";
    public List<string> Artists = [];
    public List<string> Genre = [];
    public int Year = 1970;
    public int TrackNumber = 0;
    public int TrackCount = 0;
    public int DiscNumber = 0;
    public int DiscCount = 0;
    public ulong DurationMilliSeconds = 0;

    // オーディオ
    public string Format = ""; // TODO: enumにする
    public int Channels = 0;
    public bool IsVBR = false;
    public uint SampleRate = 0;
    public uint Bitrate = 0;
    public DateTime Imported = new();

    // ファイル
    public string Path = "";
    public DateTime Modified = new();
    public DateTime Created = new();
    public ulong SizeBytes = 0;
}