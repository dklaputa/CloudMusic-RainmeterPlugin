using System;
using System.Runtime.InteropServices;
using Rainmeter;

namespace PluginCloudMusic
{
    internal class Measure
    {
        enum MeasureType
        {
            Title,
            Artist
        }

        private MeasureType _type = MeasureType.Title;

        private string _title;
        private string _artist;

        private API _api;

        internal void Reload(API api, ref double maxValue)
        {
            _api = api;
            string type = _api.ReadString("PlayerType", "");
            switch (type.ToLowerInvariant())
            {
                case "title":
                    _type = MeasureType.Title;
                    break;

                case "artist":
                    _type = MeasureType.Artist;
                    break;

                default:
                    _api?.Log(API.LogType.Error, "PluginCloudMusic.dll: Type=" + type + " not valid");
                    break;
            }
        }

        internal double Update()
        {
            _title = string.Empty;
            _artist = string.Empty;

            string nowMusic = null;
            try
            {
                nowMusic = CloudMusicUtils.GetNowMusic();
            }
            catch (Exception e)
            {
                _api?.Log(API.LogType.Error, e.ToString());
            }

            if (string.IsNullOrEmpty(nowMusic))
            {
                return 0.0;
            }

            var musicInfo = nowMusic.Split(new[] {" - "}, StringSplitOptions.None);
            if (musicInfo.Length > 0)
            {
                _title = musicInfo[0];
            }
            if (musicInfo.Length > 1)
            {
                _artist = musicInfo[1];
            }

            return 0.0;
        }

        internal string GetString()
        {
            switch (_type)
            {
                case MeasureType.Title:
                    return _title;
                
                case MeasureType.Artist:
                    return _artist;
            }

            return string.Empty;
        }

        internal void ExecuteBang(string args)
        {
            switch (args.ToLowerInvariant())
            {
                case "playpause":
                    CloudMusicUtils.PlayOrPause();
                    break;
                case "next":
                    CloudMusicUtils.PlayNext();
                    break;
                case "previous":
                    CloudMusicUtils.PlayPrevious();
                    break;
            }
        }
    }

    public static class PluginCloudMusic
    {
        static IntPtr StringBuffer = IntPtr.Zero;

        [DllExportAttribute]
        public static void Initialize(ref IntPtr data, IntPtr rm)
        {
            data = GCHandle.ToIntPtr(GCHandle.Alloc(new Measure()));
        }

        [DllExportAttribute]
        public static void Finalize(IntPtr data)
        {
            GCHandle.FromIntPtr(data).Free();

            if (StringBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(StringBuffer);
                StringBuffer = IntPtr.Zero;
            }
        }

        [DllExportAttribute]
        public static void Reload(IntPtr data, IntPtr rm, ref double maxValue)
        {
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            measure.Reload(new API(rm), ref maxValue);
        }

        [DllExportAttribute]
        public static double Update(IntPtr data)
        {
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            return measure.Update();
        }

        [DllExportAttribute]
        public static IntPtr GetString(IntPtr data)
        {
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            if (StringBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(StringBuffer);
                StringBuffer = IntPtr.Zero;
            }

            string stringValue = measure.GetString();
            if (stringValue != null)
            {
                StringBuffer = Marshal.StringToHGlobalUni(stringValue);
            }

            return StringBuffer;
        }

        [DllExportAttribute]
        public static void ExecuteBang(IntPtr data, [MarshalAs(UnmanagedType.LPWStr)] string args)
        {
            Measure measure = (Measure) GCHandle.FromIntPtr(data).Target;
            measure.ExecuteBang(args);
        }
    }
}
