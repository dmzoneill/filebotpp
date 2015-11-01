using System;
using System.Collections.Generic;
using System.Linq;

namespace FileBotPP.Helpers
{
    public static class Settings
    {
        public static string ProxyServerHost
        {
            get { return Properties.Settings.Default.ProxyServerHost; }
            set
            {
                Properties.Settings.Default.ProxyServerHost = value;
                Properties.Settings.Default.Save();
            }
        }

        public static int ProxyServerPort
        {
            get { return Properties.Settings.Default.ProxyServerPort; }
            set
            {
                Properties.Settings.Default.ProxyServerPort = value;
                Properties.Settings.Default.Save();
            }
        }

        public static List< string > AllowedTypes
        {
            get
            {
                if ( Properties.Settings.Default.AllowedTypes.Contains( ',' ) )
                {
                    return Properties.Settings.Default.AllowedTypes.Split( ',' ).ToList();
                }

                return Properties.Settings.Default.AllowedTypes.Length == 0 ? "mpg,avi,mkv,mp4".Split( ',' ).ToList() : new List< string > {Properties.Settings.Default.AllowedTypes};
            }
            set
            {
                Properties.Settings.Default.AllowedTypes = value.Count > 1 ? String.Join( ",", value ) : value[ 0 ];

                Properties.Settings.Default.Save();
            }
        }

        public static string TvdbApiKey
        {
            get { return Properties.Settings.Default.TvdbApiKey; }
            set
            {
                Properties.Settings.Default.TvdbApiKey = value;
                Properties.Settings.Default.Save();
            }
        }

        public static int CacheTimeout
        {
            get { return Properties.Settings.Default.CacheTimeout; }
            set
            {
                Properties.Settings.Default.CacheTimeout = value;
                Properties.Settings.Default.Save();
            }
        }

        public static int PoorQualityP
        {
            get { return Properties.Settings.Default.PoorQualityP; }
            set
            {
                Properties.Settings.Default.PoorQualityP = value;
                Properties.Settings.Default.Save();
            }
        }

        public static string TorrentPreferredQuality
        {
            get { return Properties.Settings.Default.TorrentPreferredQuality; }
            set
            {
                Properties.Settings.Default.TorrentPreferredQuality = value;
                Properties.Settings.Default.Save();
            }
        }

        public static string FFmpegConvert
        {
            get { return Properties.Settings.Default.FFmpegConvert; }
            set
            {
                Properties.Settings.Default.FFmpegConvert = value;
                Properties.Settings.Default.Save();
            }
        }

        public static int FilesSystemWatcherMinRefreshTime
        {
            get { return Properties.Settings.Default.FsWatcherMinRefresh; }
            set
            {
                Properties.Settings.Default.FsWatcherMinRefresh = value;
                Properties.Settings.Default.Save();
            }
        }

        public static int FilesSystemWatcherMaxRefreshTime
        {
            get { return Properties.Settings.Default.FsWatcherMaxRefresh; }
            set
            {
                Properties.Settings.Default.FsWatcherMaxRefresh = value;
                Properties.Settings.Default.Save();
            }
        }

        public static bool FilesSystemWatcherEnabled
        {
            get { return Properties.Settings.Default.FsWatcherEnabled; }
            set
            {
                Properties.Settings.Default.FsWatcherEnabled = value;
                Properties.Settings.Default.Save();
            }
        }
        
    }
}