using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicManager;
internal class UserPreference {
    public static string LibraryPath {
        get { 
            return StringGetter("LibraryPath", Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
        }

        set {
            StringSetter("LibraryPath", value);
        }
    }

    public static void ClearAll() {
               Windows.Storage.ApplicationData.Current.LocalSettings.Values.Clear();
    }

    private static string StringGetter(string key, string defaultValue) {
        var current = Windows.Storage.ApplicationData.Current.LocalSettings.Values[key];
        if (current == null) {
            return defaultValue;
        }

        return (string)current;
    }

    private static void StringSetter(string key, string value) {
        Windows.Storage.ApplicationData.Current.LocalSettings.Values[key] = value;
    }
}
