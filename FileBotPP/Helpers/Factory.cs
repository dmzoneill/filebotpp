using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileBotPP.Metadata;
using FileBotPP.Tree;

namespace FileBotPP.Helpers
{
    public class Factory : IFactory
    {
        private static IFactory _instance;
        private int _metaDataReady;
        private string _scanLocation;

        private Factory()
        {
            this.SeriesAnalyzer = new SeriesAnalyzer();
            this.Working = new List< ISupportsStop >();
            this.Utils = new Utils();
            this.Settings = new Settings();
            this.LogLines = new ConcurrentQueue< string >();
            this.Random = new Random();
            this.Torrents = new BlockingCollection< ITorrent >();

            this.AppDataFolder = this.AppDataFolder.Replace( '\\', '/' );
            if ( Directory.Exists( this.AppDataFolder ) )
            {
                Directory.CreateDirectory( this.AppDataFolder );
            }
        }

        public static IFactory Instance => _instance ?? ( _instance = new Factory() );
        public ConcurrentQueue< string > LogLines { get; }
        public string[] LocationParts { get; set; }
        public string AddSeriesName { get; set; }
        public IWindowFileBotPp WindowFileBotPp { get; set; }
        public IEztv Eztv { get; set; }
        public IItemProvider ItemProvider { get; set; }
        public ITvdb Tvdb { get; set; }
        public IKat Kat { get; set; }
        public IFilebot Filebot { get; set; }
        public ISeriesAnalyzer SeriesAnalyzer { get; set; }
        public IUtils Utils { get; set; }
        public ISettings Settings { get; set; }
        public Random Random { get; set; }
        public BlockingCollection<ITorrent> Torrents { get; set; }

        public string ScanLocation
        {
            get { return this._scanLocation; }
            set
            {
                this.LocationParts = value.Split( '\\' );
                this._scanLocation = value;
            }
        }

        public List< ISupportsStop > Working { get; set; }

        public int MetaDataReady
        {
            get { return this._metaDataReady; }
            set
            {
                this._metaDataReady = value;

                if ( this._metaDataReady == 1 )
                {
                    FsPoller.stop_all();
                }

                if ( this._metaDataReady == 4 )
                {
                    this.SeriesAnalyzer.analyze_all_series_folders();
                    Console.WriteLine( Torrents.Count );
                }

                if ( this._metaDataReady == 5 )
                {
                    FsPoller.start_all();
                }
            }
        }

        public string AppDataFolder { get; set; } = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ) + "/FileBotPP";
        public bool EztvAvailable { get; set; }
        public bool TvdbAvailable { get; set; }

        public void fetch_tvdb_metadata()
        {
            var dirs = Directory.GetDirectories( this.ScanLocation );
            this.Tvdb = new Tvdb( dirs.Select( dir => dir.Split( '\\' ) ).Select( nameparts => nameparts[ nameparts.Length - 1 ] ).ToArray() );
            this.Tvdb.downloads_series_data();
        }

        public void fetch_eztv_metadata()
        {
            this.Eztv = new Eztv();
            this.Eztv.downloads_series_data();
        }

        public void fetch_kat_metadata()
        {
            this.Kat = new Kat();
            this.Kat.downloads_series_data();
        }

        public void stop_all_workers()
        {
            foreach ( var worker in this.Working )
            {
                worker.stop_worker();
            }

            this.Eztv?.stop_worker();
            this.Tvdb?.stop_worker();
            this.Filebot?.stop_worker();

            this.Working = new List< ISupportsStop >();
        }

        public void dispose_all_workers()
        {
            foreach ( var worker in this.Working )
            {
                worker.stop_worker();
            }

            this.Eztv = null;
            this.Tvdb = null;
            this.Filebot = null;
            this.Working = null;
            FsPoller.stop_all();
        }
    }
}