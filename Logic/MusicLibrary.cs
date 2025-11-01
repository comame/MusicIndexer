using System.Collections.Generic;

namespace MusicManager.Logic;

internal class MusicLibrary {
    private List<MusicTrack> tracks = [];

    public List<MusicTrack> Tracks {
        get => tracks;
    }

    public void FillTrackCount() {
        // アルバムキー -> tracks のインデックス
        Dictionary<string, List<int>> albums = [];

        // 楽曲をアルバムごとにまとめる
        // アルバムアーティストとアルバム名が一致したら、同一アルバムとみなす
        for (var i = 0; i < tracks.Count; i++) {
            var m = tracks[i];
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
                var m = tracks[index];
                var tn = m.DiscNumber;
                if (!trackCountOfDisc.ContainsKey(tn)) {
                    trackCountOfDisc[tn] = 1;
                    continue;
                }
                trackCountOfDisc[tn]++;
            }

            foreach (var index in albums[key]) {
                tracks[index].TrackCount = trackCountOfDisc[tracks[index].DiscNumber];
            }
        }
    }

    public void SortByImportedDate() {
        tracks.Sort((a, b) => a.Imported.CompareTo(b.Imported));
    }
}