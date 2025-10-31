using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static MusicManager.Logic.NativePropertySystem;

namespace MusicManager.Logic {
    // IPropertyStore のラッパー
    class PropertyStore : IDisposable {
        private IPropertyStore? propertyStore;

        public static PropertyStore Open(string path) {
            Guid propertyStoreGUID = typeof(IPropertyStore).GUID;

            var hr = SHGetPropertyStoreFromParsingName(
                path,
                nint.Zero,
                GETPROPERTYSTOREFLAGS.GPS_DEFAULT,
                ref propertyStoreGUID,
                out IPropertyStore? ps
            );

            if (hr != HRESULT.S_OK) {
                throw new Exception($"HRESULT: {hr}");
            }

            if (ps == null) {
                throw new Exception("HRESULT=S_OKなのに、propertyStoreがnull");
            }

            var instance = new PropertyStore {
                propertyStore = ps
            };

            return instance;
        }

        public void Dispose() {
            Marshal.ReleaseComObject(propertyStore!);
            propertyStore = null;
        }

        public string GetString(in PROPERTYKEY key) {
            if (propertyStore == null) {
                throw new Exception("propertyStoreがnull");
            }

            var hr = propertyStore.GetValue(key, out PROPVARIANT pv);
            if (hr != HRESULT.S_OK) {
                throw new Exception($"HRESULT: {hr}");
            }

            return PropVariant.AsString(pv);
        }

        public bool GetBool(in PROPERTYKEY key) {
            if (propertyStore == null) {
                throw new Exception("propertyStoreがnull");
            }
            var hr = propertyStore.GetValue(key, out PROPVARIANT pv);
            if (hr != HRESULT.S_OK) {
                throw new Exception($"HRESULT: {hr}");
            }
            return PropVariant.AsBool(pv);
        }

        public uint GetUInt(in PROPERTYKEY key) {
            if (propertyStore == null) {
                throw new Exception("propertyStoreがnull");
            }
            var hr = propertyStore.GetValue(key, out PROPVARIANT pv);
            if (hr != HRESULT.S_OK) {
                throw new Exception($"HRESULT: {hr}");
            }
            return PropVariant.AsUint(pv);
        }

        public ulong GetUlong(in PROPERTYKEY key) {
            if (propertyStore == null) {
                throw new Exception("propertyStoreがnull");
            }
            var hr = propertyStore.GetValue(key, out PROPVARIANT pv);
            if (hr != HRESULT.S_OK) {
                throw new Exception($"HRESULT: {hr}");
            }
            return PropVariant.AsUlong(pv);
        }
        public List<string> GetStringList(in PROPERTYKEY key) {
            if (propertyStore == null) {
                throw new Exception("propertyStoreがnull");
            }
            var hr = propertyStore.GetValue(key, out PROPVARIANT pv);
            if (hr != HRESULT.S_OK) {
                throw new Exception($"HRESULT: {hr}");
            }
            return PropVariant.AsStringList(pv);
        }

        public DateTime GetDateTime(in PROPERTYKEY key) {
            if (propertyStore == null) {
                throw new Exception("propertyStoreがnull");
            }
            var hr = propertyStore.GetValue(key, out PROPVARIANT pv);
            if (hr != HRESULT.S_OK) {
                throw new Exception($"HRESULT: {hr}");
            }
            return PropVariant.AsDateTime(pv);
        }
    }

    // PROPVARIANT から C# の値に変換するヘルパー
    internal class PropVariant {
        public static bool IsEmpty(in PROPVARIANT pv) {
            return pv.varType == (ushort)VARENUM.VT_EMPTY;
        }

        public static string AsString(in PROPVARIANT pv) {
            if (IsEmpty(pv)) {
                return "";
            }

            if (pv.varType != (ushort)VARENUM.VT_LPWSTR) {
                throw new Exception("VT_LPWSTRではない" + pv.varType);
            }

            var ret = Marshal.PtrToStringUni(pv.pwszVal);
            if (ret == null) {
                throw new Exception("nullが返された");
            }

            return ret;
        }

        public static bool AsBool(in PROPVARIANT pv) {
            if (IsEmpty(pv)) {
                return false;
            }

            if (pv.varType != (ushort)VARENUM.VT_BOOL) {
                throw new Exception("VT_BOOLではない" + pv.varType);
            }
            return pv.boolVal != 0;
        }

        public static uint AsUint(in PROPVARIANT pv) {
            if (IsEmpty(pv)) {
                return 0;
            }

            if (pv.varType != (ushort)VARENUM.VT_UI4) {
                throw new Exception("VT_UI4ではない" + pv.varType);
            }
            return pv.ulVal;
        }
        public static ulong AsUlong(in PROPVARIANT pv) {
            if (IsEmpty(pv)) {
                return 0;
            }
            if (pv.varType != (ushort)VARENUM.VT_UI8) {
                throw new Exception("VT_UI8ではない" + pv.varType);
            }
            return pv.uhVal;
        }

        public static List<string> AsStringList(in PROPVARIANT pv) {
            if (IsEmpty(pv)) {
                return [];
            }

            if (pv.varType != (ushort)(VARENUM.VT_VECTOR | VARENUM.VT_LPWSTR)) {
                throw new Exception("VT_VECTOR|VT_LPWSTRではない" + pv.varType);
            }

            var list = new List<string>();

            var cnt = pv.calpwstr.count;
            for (int i = 0; i < cnt; i++) {
                var p = Marshal.ReadIntPtr(pv.calpwstr.p, i * nint.Size);
                var s = Marshal.PtrToStringUni(p);
                if (s == null) {
                    throw new Exception("nullが返された");
                }

                list.Add(s);
            }

            return list;
        }

        public static DateTime AsDateTime(in PROPVARIANT pv) {
            if (IsEmpty(pv)) {
                return DateTime.MinValue;
            }
            if (pv.varType != (ushort)VARENUM.VT_FILETIME) {
                throw new Exception("VT_FILETIMEではない" + pv.varType);
            }
            long fileTime = ((long)pv.filetime.dwHighDateTime << 32) + pv.filetime.dwLowDateTime;
            return DateTime.FromFileTime(fileTime);
        }
    }
}