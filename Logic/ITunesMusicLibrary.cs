using System;
using System.IO;
using System.Linq;
using System.Text;

namespace MusicManager.Logic {
    internal class ITLTrack {
        public int TrackID = 0;
        public ulong Size = 0;
        public ulong TotalTime = 0;
        public int TrackNumber;
        public int TrackCount;
        public int DiscNumber;
        public int DiscCount;
        public int Year = 0;
        public string DateModified = "";
        public string DateAdded = "";
        public int BitRate = 0;
        public int SampleRate = 0;
        public int ArtworkCount = 0;
        public string PersistentID = "";
        public string TrackType = "";
        public int FileFolderCount = 0;
        public int LibraryFolderCount = 0;
        public string Name = "";
        public string Artist = "";
        public string AlbumArtist = "";
        public string Album = "";
        public string Genre = "";
        public string Kind = "";
        public string Location = "";

        public static ITLTrack FromMusicMetadata(MusicMetadata m, int trackID) {
            var track = new ITLTrack() {
                TrackID = trackID,
                Size = m.SizeBytes,
                TotalTime = m.DurationMilliSeconds,
                TrackNumber = m.TrackNumber,
                TrackCount = m.TrackCount,
                DiscNumber = m.DiscNumber,
                DiscCount = m.DiscCount,
                Year = m.Year,
                DateModified = ITLUtil.ToUTCDatetimeString(m.Modified),
                DateAdded = ITLUtil.ToUTCDatetimeString(m.Imported),
                BitRate = (int)m.Bitrate,
                SampleRate = (int)m.SampleRate,
                ArtworkCount = 1,
                PersistentID = ITLUtil.CalculatePersistentID(m.Path), // Path はそうそう変わらないので、persistentID は Path から生成する
                Name = m.Name,
                Artist = string.Join(",", m.Artists),
                AlbumArtist = m.AlbumArtist,
                Album = m.AlbumTitle,
                Genre = string.Join(",", m.Genre),
                Location = ITLUtil.ConvertPathToLocation(m.Path),
            };

            return track;
        }

        // iTunes Music Library.xml の track 情報部分を書き出す
        public void WriteTo(StreamWriter w) {
            var addIntegerKeyIfNotZero = (ref string s, string key, int value) => {
                if (value == 0) {
                    return;
                }

                s += $"\t\t\t<key>{key}</key><integer>{value}</integer>\n";
            };

            var addStringKeyIfNotEmpty = (ref string s, string key, string value, string tag = "string") => {
                if (value == "") {
                    return;
                }

                s += $"\t\t\t<key>{key}</key><{tag}>{ITLUtil.EscapeXMLString(value)}</{tag}>\n";
            };

            var s = "";

            s += $"\t\t<key>{TrackID}</key>\n";
            s += "\t\t<dict>\n";

            addIntegerKeyIfNotZero(ref s, "Track ID", TrackID);
            s += $"\t\t\t<key>Size</key><integer>{Size}</integer>\n";
            s += $"\t\t\t<key>Total Time</key><integer>{TotalTime}</integer>\n";
            addIntegerKeyIfNotZero(ref s, "Disc Number", DiscNumber);
            addIntegerKeyIfNotZero(ref s, "Disc Count", DiscCount);
            addIntegerKeyIfNotZero(ref s, "Track Number", TrackNumber);
            addIntegerKeyIfNotZero(ref s, "Track Count", TrackCount);
            addIntegerKeyIfNotZero(ref s, "Year", Year);
            addStringKeyIfNotEmpty(ref s, "Date Modified", DateModified, "date");
            addStringKeyIfNotEmpty(ref s, "Date Added", DateAdded, "date");
            addIntegerKeyIfNotZero(ref s, "Bit Rate", BitRate);
            addIntegerKeyIfNotZero(ref s, "Sample Rate", SampleRate);
            addStringKeyIfNotEmpty(ref s, "Persistent ID", PersistentID);
            addStringKeyIfNotEmpty(ref s, "Name", Name);
            addStringKeyIfNotEmpty(ref s, "Artist", Artist);
            addStringKeyIfNotEmpty(ref s, "Album Artist", AlbumArtist);
            addStringKeyIfNotEmpty(ref s, "Album", Album);
            addStringKeyIfNotEmpty(ref s, "Genre", Genre);
            addStringKeyIfNotEmpty(ref s, "Location", Location);

            s += "\t\t</dict>\n";

            w.Write(s);
        }
    }

    internal class ITLUtil {
        public static long CalculateAppleEpoc(in DateTime dt) {
            var appleEpoc = dt.AddYears(66);
            var unixEpoc = ((DateTimeOffset)appleEpoc).ToUnixTimeSeconds();
            return unixEpoc;
        }

        public static string ConvertPathToLocation(string path) {
            var parts = path.Split("\\");
            var location = "file://localhost";

            for (var i = 0; i < parts.Count(); i++) {
                location += "/";
                // ドライブ文字はURLエンコードできない
                if (i == 0) {
                    location += parts[i];
                    continue;
                }

                location += System.Net.WebUtility.UrlEncode(parts[i]).Replace("+", "%20");
            }

            return location;
        }

        public static string ToUTCDatetimeString(in DateTime dt) {
            return dt.AddHours(-9).ToString("yyyy-MM-ddTHH:mm:ssZ");
        }

        public static string CalculatePersistentID(string seed) {
            var bytes = Encoding.UTF8.GetBytes(seed);
            var hash = System.Security.Cryptography.MD5.HashData(bytes);
            var hex = Convert.ToHexString(hash).ToUpper();
            return hex[..16];
        }

        public static void WriteLibraryXMLHeader(StreamWriter w) {
            var date = ToUTCDatetimeString(DateTime.Now);
            var libraryPersistentID = "38CAD4A721A4B4EB";

            var s = "";

            s += "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n";
            s += "<!DOCTYPE plist PUBLIC \"-//Apple Computer//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">\n";
            s += "<plist version=\"1.0\">\n";

            s += "<dict>\n";
            s += "\t<key>Major Version</key><integer>1</integer>\n";
            s += "\t<key>Minor Version</key><integer>1</integer>\n";
            s += "\t<key>Application Version</key><string>12.13.8.3</string>\n";
            s += $"\t<key>Date</key><date>{date}</date>\n";
            s += "\t<key>Features</key><integer>5</integer>\n";
            s += "\t<key>Show Content Ratings</key><true/>\n";
            s += $"\t<key>Library Persistent ID</key><string>{libraryPersistentID}</string>\n";
            s += "\t<key>Tracks</key>\n";
            s += "\t<dict>\n";

            w.Write(s);
        }

        public static void WriteLibraryXMLFooter(StreamWriter w, string musicFolder) {
            var s = "";

            s += "\t</dict>\n";

            // TODO: いったん Playlist 部分は省略
            s += "\t<key>Playlists</key>\n";
            s += "\t<array>\n";
            s += "\t</array>\n";

            s += $"\t<key>Music Folder</key><string>{EscapeXMLString(ConvertPathToLocation(musicFolder))}/</string>\n";
            s += "</dict>\n";
            s += "</plist>\n";

            w.Write(s);
        }

        public static string EscapeXMLString(string s) {
            return s.Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;");
        }
    }
}
