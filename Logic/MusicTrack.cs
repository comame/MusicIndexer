using System;
using System.Collections.Generic;

namespace MusicManager.Logic;

internal class  MusicTrack {
    // タグ
    public string Name { get; set; } = "";
    public string AlbumArtist { get; set; } = "";
    public string AlbumTitle { get; set; } = "";
    public List<string> Artists { get; set; } = [];
    public List<string> Genre { get; set; } = [];
    public int Year { get; set; } = 1970;
    public int TrackNumber { get; set; } = 0;
    public int TrackCount { get; set; } = 0;
    public int DiscNumber { get; set; } = 0;
    public int DiscCount { get; set; } = 0;
    public ulong DurationMilliSeconds { get; set; } = 0;

    // オーディオ
    public string Format { get; set; } = ""; // TODO: enumにする
    public int Channels { get; set; } = 0;
    public bool IsVBR { get; set; } = false;
    public uint SampleRate { get; set; } = 0;
    public uint Bitrate { get; set; } = 0;
    public DateTime Imported { get; set; } = new();

    // ファイル
    public string Path { get; set; } = "";
    public DateTime Modified { get; set; } = new();
    public DateTime Created { get; set; } = new();
    public ulong SizeBytes { get; set; } = 0;
}