using System;
using System.Collections.Generic;
using System.IO;
using FileBotPP.Helpers.Interfaces;
using FileBotPP.Interfaces;
using FileBotPP.Metadata.Interfaces;
using FileBotPP.Tree;

namespace FileBotPP.Helpers
{
    public static class Common
    {
        private static int _metaDataReady;
        public static string AddSeriesName = null;
        private static string _scanLocation;
        private static string _appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/FileBotPP";

        static Common()
        {
            SeriesAnalyzer = new SeriesAnalyzer();
            Working = new List< ISupportsStop >();

            AppDataFolder = AppDataFolder.Replace( '\\', '/' );
            if ( Directory.Exists( AppDataFolder ) )
            {
                Directory.CreateDirectory( AppDataFolder );
            }
        }

        public static IFileBotPpWindow FileBotPp { get; set; }
        public static IEztv Eztv { get; set; }
        public static ITvdb Tvdb { get; set; }
        public static IFilebot Filebot { get; set; }
        public static ISeriesAnalyzer SeriesAnalyzer { get; set; }

        public static string ScanLocation
        {
            get { return _scanLocation; }
            set
            {
                LocationParts = value.Split( '\\' );
                _scanLocation = value;
            }
        }

        public static List< ISupportsStop > Working { get; set; }

        public static int MetaDataReady
        {
            get { return _metaDataReady; }
            set
            {
                _metaDataReady = value;

                if (_metaDataReady == 1 )
                {
                    FsPoller.stop_all();
                }

                if ( _metaDataReady == 3 )
                {
                    SeriesAnalyzer.analyze_all_series_folders();
                }

                if (_metaDataReady == 4)
                {
                    FsPoller.start_all();
                }
            }
        }

        public static string AppDataFolder
        {
            get { return _appDataFolder; }
            set { _appDataFolder = value; }
        }

        public static bool EztvAvailable { get; set; }
        public static bool TvdbAvailable { get; set; }
        public static string[] LocationParts { get; set; }
        public static Object TreeLock { get; } = new object();

        public static void stop_all_workers()
        {
            foreach ( var worker in Working )
            {
                worker.stop_worker();
            }

            Eztv?.stop_worker();
            Tvdb?.stop_worker();
            Filebot?.stop_worker();

            Working = new List< ISupportsStop >();
        }

        public static void dispose_all_workers()
        {
            foreach ( var worker in Working )
            {
                worker.stop_worker();
            }

            Eztv = null;
            Tvdb = null;
            Filebot = null;
            Working = null;
            FsPoller.stop_all();
        }
    }
}