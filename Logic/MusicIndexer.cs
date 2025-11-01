using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace MusicManager.Logic;

class MusicIndexer {
    public static MusicLibrary? LoadFromIndexFile() {
        try {
            using var f = new FileStream(UserPreference.LibraryPath + "\\music_index.json", FileMode.Open, FileAccess.Read);
            var library = MusicLibrary.FromJSONReader(f);
            return library;
        } catch (FileNotFoundException) {
            return null;
        }
    }

    public static int CountMusicFiles(string searchDirectory) {
        var files = FindMusicFiles(searchDirectory);
        return files.Count;
    }

    public static MusicLibrary? UpdateIndex(
        Action<double> onProgress,
        in CancellationToken ctx
    ) {
        var files = FindMusicFiles(UserPreference.LibraryPath);
        if (files.Count == 0) {
            return null;
        }

        // インデックスし直すので、ライブラリは新規作成してよい
        var library = new MusicLibrary();

        for (var i = 0; i < files.Count; i++) {
            if (ctx.IsCancellationRequested) {
                return null;
            }

            var file = files[i];
            var meta = GetMusicMetadata(file);
            library.Tracks.Add(meta);

            if (i % 30 == 0) {
                onProgress((double)i / files.Count * 100);
            }
        }

        library.FillTrackCount();
        library.SortByImportedDate();

        using var f = new FileStream(UserPreference.LibraryPath + "\\music_index.json", FileMode.Create, FileAccess.Write);
        library.WriteJSON(f);
        f.Flush();

        return library;
    }

    public static void GenerateITLFile(in MusicLibrary library) {
        using var f = new StreamWriter(UserPreference.LibraryPath + "\\iTunes Music Library.xml", append: false);

        ITLUtil.WriteLibraryXMLHeader(f);
        for (var i = 0; i < library.Tracks.Count; i++) {
            var t = ITLTrack.FromMusicMetadata(library.Tracks[i], i);
            t.WriteTo(f);
        }
        ITLUtil.WriteLibraryXMLFooter(f, UserPreference.LibraryPath);

        f.Flush();
    }

    private static List<string> FindMusicFiles(string directory) {
        var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".mp3", ".m4a" };
        var files = Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories)
            .Where(file => extensions.Contains(Path.GetExtension(file)))
            .ToList();
        return files;
    }

    // 音楽ファイルからメタデータを取得する。
    // ファイル単体から推測できない、TrackNumber は取得しない。
    private static MusicTrack GetMusicMetadata(string path) {
        using var ps = PropertyStore.Open(path);

        var m = new MusicTrack() {
            // タグ
            Name = ps.GetString(NativePropertySystem.PKEY_Title),
            AlbumArtist = ps.GetString(NativePropertySystem.PKEY_Music_AlbumArtist),
            AlbumTitle = ps.GetString(NativePropertySystem.PKEY_Music_AlbumTitle),
            Artists = ps.GetStringList(NativePropertySystem.PKEY_Music_Artist),
            Genre = ps.GetStringList(NativePropertySystem.PKEY_Music_Genre),
            Year = (int)ps.GetUInt(NativePropertySystem.PKey_Media_Year),
            TrackNumber = (int)ps.GetUInt(NativePropertySystem.PKEY_Music_TrackNumber),
            DurationMilliSeconds = ps.GetUlong(NativePropertySystem.PKey_Media_Duration) / 10_000,
            // DiscNumber は PartOfSet から取得する

            // オーディオ
            // TODO: Format は拡張子から推測する
            Channels = (int)ps.GetUInt(NativePropertySystem.PKEY_Audio_ChannelCount),
            IsVBR = ps.GetBool(NativePropertySystem.PKEY_Audio_IsVariableBitRate),
            SampleRate = ps.GetUInt(NativePropertySystem.PKEY_Audio_SampleRate),
            Bitrate = ps.GetUInt(NativePropertySystem.PKEY_Audio_EncodingBitrate),
            Imported = ps.GetDateTime(NativePropertySystem.PKey_DateImported), // コンテンツの作成日; おおむね追加日として使用する

            // ファイル
            Path = path,
            Modified = ps.GetDateTime(NativePropertySystem.PKEY_DateModified),
            Created = ps.GetDateTime(NativePropertySystem.PKEY_DateCreated),
            SizeBytes = ps.GetUlong(NativePropertySystem.PKEY_Size),
        };

        // DiscNumeber, DiscCount を PartOfSet から取得する
        var partOfSet = ps.GetString(NativePropertySystem.PKEY_Music_PartOfSet);
        var partOfSetSplit = partOfSet.Split('/');
        if (partOfSetSplit.Length == 2) {
            if (int.TryParse(partOfSetSplit[0], out int discNumber)) {
                m.DiscNumber = discNumber;
            }
            if (int.TryParse(partOfSetSplit[1], out int discCount)) {
                m.DiscCount = discCount;
            }
        }

        return m;
    }
}