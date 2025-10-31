using System;
using System.Runtime.InteropServices;

namespace MusicManager.Logic {
    unsafe partial class NativePropertySystem {
        public enum HRESULT : long {
            S_OK = 0,
        }
        public enum GETPROPERTYSTOREFLAGS {
            GPS_DEFAULT = 0,
        }

        [DllImport("Shell32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern HRESULT SHGetPropertyStoreFromParsingName(
            string pszPath,
            nint pbc,
            GETPROPERTYSTOREFLAGS flags,
            ref Guid iid,
            [MarshalAs(UnmanagedType.Interface)] out IPropertyStore propertyStore
        );

        // propsys.h
        [ComImport, Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IPropertyStore {
            HRESULT GetCount([Out] out uint propertyCount);
            HRESULT GetAt([In] uint propertyIndex, [Out, MarshalAs(UnmanagedType.Struct)] out PROPERTYKEY key);
            HRESULT GetValue([In, MarshalAs(UnmanagedType.Struct)] ref PROPERTYKEY key, [Out, MarshalAs(UnmanagedType.Struct)] out PROPVARIANT pv);
            HRESULT SetValue([In, MarshalAs(UnmanagedType.Struct)] ref PROPERTYKEY key, [In, MarshalAs(UnmanagedType.Struct)] ref PROPVARIANT pv);
            HRESULT Commit();
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct PROPERTYKEY {
            public Guid fmtid;
            public uint pid;

            public PROPERTYKEY(Guid _fmtid, uint _pid) : this() {
                fmtid = _fmtid;
                pid = _pid;
            }
        }

        // propkey.h
        // VT_LPWSTR
        public static readonly PROPERTYKEY PKEY_Music_AlbumArtist = new PROPERTYKEY(new Guid("56A3372E-CE9C-11D2-9F0E-006097C686F6"), 13);
        // VT_LPWSTR
        public static readonly PROPERTYKEY PKEY_Music_AlbumTitle = new PROPERTYKEY(new Guid("56A3372E-CE9C-11D2-9F0E-006097C686F6"), 4);
        // VT_VECTOR | VT_LPWSTR
        public static readonly PROPERTYKEY PKEY_Music_Artist = new PROPERTYKEY(new Guid("56A3372E-CE9C-11D2-9F0E-006097C686F6"), 2);
        // VT_UI4
        public static readonly PROPERTYKEY PKEY_Music_DiscNumber = new PROPERTYKEY(new Guid("56A3372E-CE9C-11D2-9F0E-006097C686F6"), 104);
        // VT_VECTOR | VT_LPWSTR
        public static readonly PROPERTYKEY PKEY_Music_Genre = new PROPERTYKEY(new Guid("56A3372E-CE9C-11D2-9F0E-006097C686F6"), 11);
        // VT_BOOL
        public static readonly PROPERTYKEY PKEY_Music_IsCompilation = new PROPERTYKEY(new Guid("C449D5CB-9EA4-4809-82E8-AF9D59DED6D1"), 100);
        // VT_UI4
        public static readonly PROPERTYKEY PKEY_Music_TrackNumber = new PROPERTYKEY(new Guid("56A3372E-CE9C-11D2-9F0E-006097C686F6"), 7);
        // VT_UI4
        public static readonly PROPERTYKEY PKey_Media_Year = new PROPERTYKEY(new Guid("56A3372E-CE9C-11D2-9F0E-006097C686F6"), 5);
        // VT_UI8 (100ns units)
        public static readonly PROPERTYKEY PKey_Media_Duration = new PROPERTYKEY(new Guid("64440490-4C8B-11D1-8B70-080036B11A03"), 3);
        // VT_LPWSTR
        public static readonly PROPERTYKEY PKEY_Music_PartOfSet = new PROPERTYKEY(new Guid("56A3372E-CE9C-11D2-9F0E-006097C686F6"), 37);
        // VT_LPWSTR
        public static readonly PROPERTYKEY PKEY_Title = new PROPERTYKEY(new Guid("F29F85E0-4FF9-1068-AB91-08002B27B3D9"), 2);
        // VT_LPWSTR
        public static readonly PROPERTYKEY PKEY_Audio_Format = new PROPERTYKEY(new Guid("64440490-4C8B-11D1-8B70-080036B11A03"), 2);
        // VT_UI4
        public static readonly PROPERTYKEY PKEY_Audio_ChannelCount = new PROPERTYKEY(new Guid("64440490-4C8B-11D1-8B70-080036B11A03"), 7);
        // VT_BOOL
        public static readonly PROPERTYKEY PKEY_Audio_IsVariableBitRate = new PROPERTYKEY(new Guid("E6822FEE-8C17-4D62-823C-8E9CFCBD1D5C"), 100);
        // VT_UI4
        public static readonly PROPERTYKEY PKEY_Audio_SampleRate = new PROPERTYKEY(new Guid("64440490-4C8B-11D1-8B70-080036B11A03"), 5);
        // VT_UI4
        public static readonly PROPERTYKEY PKEY_Audio_EncodingBitrate = new PROPERTYKEY(new Guid("64440490-4C8B-11D1-8B70-080036B11A03"), 4);
        // VT_FILETIME
        public static readonly PROPERTYKEY PKEY_DateCreated = new PROPERTYKEY(new Guid("B725F130-47EF-101A-A5F1-02608C9EEBAC"), 15);
        // VT_FILETIME
        public static readonly PROPERTYKEY PKEY_DateModified = new PROPERTYKEY(new Guid("B725F130-47EF-101A-A5F1-02608C9EEBAC"), 14);
        // VT_UI8
        public static readonly PROPERTYKEY PKEY_Size = new PROPERTYKEY(new Guid("B725F130-47EF-101A-A5F1-02608C9EEBAC"), 12);
        // VT_FILETIME
        public static readonly PROPERTYKEY PKey_DateImported = new PROPERTYKEY(new Guid("14B81DA1-0135-4D31-96D9-6CBFC9671A99"), 18258);


        [StructLayout(LayoutKind.Explicit, Pack = 0)]
        public struct PROPVARIANT {
            [FieldOffset(0)]
            public ushort varType;
            [FieldOffset(2)]
            public ushort wReserved1;
            [FieldOffset(4)]
            public ushort wReserved2;
            [FieldOffset(6)]
            public ushort wReserved3;


            [FieldOffset(8)]
            public nint pwszVal; // VT_LPWSTR
            [FieldOffset(8)]
            public PROPVARIANT_VECTOR calpwstr; // VT_VECTOR | VT_LPWSTR
            [FieldOffset(8)]
            public ushort boolVal; // VT_BOOL
            [FieldOffset(8)]
            public uint ulVal; // VT_UI4
            [FieldOffset(8)]
            public ulong uhVal; // VT_UI8
            [FieldOffset(8)]
            public PROPVARIANT_FILETIME filetime; // VT_FILETIME
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct PROPVARIANT_VECTOR {
            public uint count;
            public nint p;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct PROPVARIANT_FILETIME {
            public uint dwLowDateTime;
            public uint dwHighDateTime;
        }

        public enum VARENUM : ushort {
            VT_EMPTY = 0,
            VT_LPWSTR = 31,
            VT_VECTOR = 0x1000,
            VT_BOOL = 11,
            VT_UI4 = 19,
            VT_UI8 = 21,
            VT_FILETIME = 64,
        };
    }
}
