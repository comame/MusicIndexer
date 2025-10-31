using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace MusicManager.Logic {
    class Music {
        public static int CountMusicFiles(string searchDirectory) {
            var files = FindMusicFiles(searchDirectory);
            return files.Count;
        }

        public static bool SearchMusicFilesAndExportITLFile(
            in CancellationToken ctx,
            string searchDirectory,
            string outDirectory,
            Action<int, int, bool> onProgress
        ) {
            var files = FindMusicFiles(searchDirectory);
            if (files.Count == 0) {
                return true;
            }

            List<MusicMetadata> musics = [];

            for (var i = 0; i < files.Count; i++) {
                if (ctx.IsCancellationRequested) {
                    return false;
                }

                var file = files[i];
                var meta = GetMusicMetadata(file);
                musics.Add(meta);

                if (i % 30 == 0) {
                    onProgress(i, files.Count, false);
                }
            }

            onProgress(0, 0, true);

            FillTrackCount(ref musics);
            musics.Sort((a, b) => a.Imported.CompareTo(b.Imported));

            using (var f = new StreamWriter(outDirectory + "\\iTunes Music Library.xml", append: false)) {
                ITLUtil.WriteLibraryXMLHeader(f);
                for (var i = 0; i < musics.Count; i++) {
                    var t = ITLTrack.FromMusicMetadata(musics[i], i);
                    t.WriteTo(f);
                }
                ITLUtil.WriteLibraryXMLFooter(f, searchDirectory);

                f.Flush();
            }

            onProgress(files.Count, files.Count, false);

            return true;
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
        private static MusicMetadata GetMusicMetadata(string path) {
            using var ps = PropertyStore.Open(path);

            var m = new MusicMetadata() {
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

        public static void FillTrackCount(ref List<MusicMetadata> musics) {
            // アルバムキー -> musics のインデックス
            Dictionary<string, List<int>> albums = [];

            // 楽曲をアルバムごとにまとめる
            // アルバムアーティストとアルバム名が一致したら、同一アルバムとみなす
            for (var i = 0; i < musics.Count; i++) {
                var m = musics[i];
                var albumKey = $"{m.AlbumArtist} - {m.AlbumTitle}";
                if (!albums.ContainsKey(albumKey)) {
                    albums[albumKey] = [];
                }
                albums[albumKey].Add(i);
            }

            // TrackCount を DiscNumber ごとに集計する
            foreach (var key in albums.Keys) {
                var trackCountOfDisc = new Dictionary<int, int>(); // DiscNumber -> TrackCount
                foreach (var index in albums[key]) {
                    var m = musics[index];
                    var tn = m.DiscNumber;
                    if (!trackCountOfDisc.ContainsKey(tn)) {
                        trackCountOfDisc[tn] = 1;
                        continue;
                    }
                    trackCountOfDisc[tn]++;
                }

                foreach (var index in albums[key]) {
                    musics[index].TrackCount = trackCountOfDisc[musics[index].DiscNumber];
                }
            }
        }
    }

    internal class MusicMetadata {
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

        public override string ToString() {
            //return "<MusicMetadata:" + string.Join(", ", new[]
            //{
            //    $"Name={Name}",
            //    $"AlbumArtist={AlbumArtist}",
            //    $"AlbumTitle={AlbumTitle}",
            //    $"Artists=[{string.Join(", ", Artists)}]",
            //    $"Genre={string.Join(", ", Genre)}",
            //    $"Year={Year}",
            //    $"TrackNumber={TrackNumber}",
            //    $"TrackCount={TrackCount}",
            //    $"DiscNumber={DiscNumber}",
            //    $"DiscCount={DiscCount}",
            //    $"DurationSeconds={DurationMilliSeconds}",
            //    $"Format={Format}",
            //    $"Channels={Channels}",
            //    $"IsVBR={IsVBR}",
            //    $"SampleRate={SampleRate}",
            //    $"Bitrate={Bitrate}",
            //    $"Imported={Imported}",
            //    $"Path={Path}",
            //    $"Modified={Modified}",
            //    $"Created={Created}",
            //    $"SizeBytes={SizeBytes}",
            //}) + ">";
            return string.Join(", ", new[]
            {
                $"{Name}:{AlbumTitle}",
                $"by={AlbumArtist}",
                $"track={TrackNumber}/{TrackCount}",
                $"disc={DiscNumber}/{DiscCount}"
            });
        }
    }
}
