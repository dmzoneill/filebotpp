using System;
using System.Collections.Generic;
using System.IO;
using FileBotPP.Interfaces;

namespace FileBotPP.Helpers
{
    public static class Common
    {
        private static int _metaDataReady;

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
        public static ISeriesAnalyzer SeriesAnalyzer { get; set; }
        public static string ScanLocation { get; set; }
        public static List< ISupportsStop > Working { get; set; }

        public static int MetaDataReady
        {
            get { return _metaDataReady; }
            set
            {
                _metaDataReady = value;
                if ( _metaDataReady > 2 )
                {
                    SeriesAnalyzer.analyze_series_folder();
                }
            }
        }

        public static string AppDataFolder { get; set; } = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ) + "/FileBotPP";
        public static bool EztvAvailable { get; set; }
        public static bool TvdbAvailable { get; set; }

        public static void stop_all_workers()
        {
            foreach ( var worker in Working )
            {
                worker.stop_worker();
            }

            Eztv.stop_worker();
            Tvdb.stop_worker();

            Working = new List< ISupportsStop >();
        }
    }
}