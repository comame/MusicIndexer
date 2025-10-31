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

    public static (int, int)? WindowPosition {
        get {
            object x = Windows.Storage.ApplicationData.Current.LocalSettings.Values["WindowPositionX"];
            object y = Windows.Storage.ApplicationData.Current.LocalSettings.Values["WindowPositionY"];

            if (x == null || y == null) {
                return null;
            }
            return ((int)x, (int)y);
        }
        set {
            Windows.Storage.ApplicationData.Current.LocalSettings.Values["WindowPositionX"] = value!.Value.Item1;
            Windows.Storage.ApplicationData.Current.LocalSettings.Values["WindowPositionY"] = value!.Value.Item2;
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
