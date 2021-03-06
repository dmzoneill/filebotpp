﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using FileBotPP.Helpers;

namespace FileBotPP.Metadata
{
    public class Tvdb : ITvdb, ISupportsStop, IDisposable
    {
        private readonly string[] _dirs;
        private readonly List< ITvdbSeries > _series;
        private readonly List< ITvdbWorker > _workers;
        private BackgroundWorker _allSeriesWorker;
        private BackgroundWorker _artworkWorker;
        private string _seriesName;
        private BackgroundWorker _seriesWorker;
        private bool _stop;

        public Tvdb( string[] dirs )
        {
            this._dirs = dirs;
            this._series = new List< ITvdbSeries >();
            this._workers = new List< ITvdbWorker >();
            this.create_folders();
        }

        public void Dispose()
        {
            this.Dispose( true );
            GC.SuppressFinalize( this );
        }

        public void downloads_series_data()
        {
            try
            {
                this._allSeriesWorker = new BackgroundWorker();
                this._allSeriesWorker.DoWork += this.AllSeriesWorkerDoWork;
                this._allSeriesWorker.RunWorkerCompleted += this.AllSeriesWorkerRunWorkerCompleted;
                this._allSeriesWorker.ProgressChanged += this.AllSeriesWorkerProgressChanged;
                this._allSeriesWorker.WorkerReportsProgress = true;
                this._allSeriesWorker.RunWorkerAsync();
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
            }
        }

        public void get_series_from_workers()
        {
            try
            {
                foreach ( var worker in this._workers.Where( worker => worker.get_series() != null ) )
                {
                    this._series.Add( worker.get_series() );
                }

                this._workers.Clear();
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
            }
        }

        public List< ITvdbSeries > get_series()
        {
            return this._series;
        }

        public ITvdbSeries get_series_by_name( string name )
        {
            try
            {
                return this._series.FirstOrDefault( series => String.Compare( series.get_name(), name, StringComparison.Ordinal ) == 0 );
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
                return null;
            }
        }

        public void free_workers()
        {
            try
            {
                this._workers.Clear();
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
            }
        }

        public void stop_worker()
        {
            this._stop = true;
        }

        public void downloads_series_data( string name )
        {
            try
            {
                this._seriesName = name;
                this._seriesWorker = new BackgroundWorker();
                this._seriesWorker.DoWork += this.SeriesWorkerDoWork;
                this._seriesWorker.WorkerReportsProgress = true;
                this._seriesWorker.RunWorkerAsync();
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void create_folders()
        {
            if ( !Directory.Exists( Factory.Instance.AppDataFolder + "/tvdbartwork/" ) )
            {
                Directory.CreateDirectory( Factory.Instance.AppDataFolder + "/tvdbartwork" );
            }

            if ( !Directory.Exists( Factory.Instance.AppDataFolder + "/tvdbartwork/banner" ) )
            {
                Directory.CreateDirectory( Factory.Instance.AppDataFolder + "/tvdbartwork/banner" );
            }

            if ( !Directory.Exists( Factory.Instance.AppDataFolder + "/tvdbartwork/fanart" ) )
            {
                Directory.CreateDirectory( Factory.Instance.AppDataFolder + "/tvdbartwork/fanart" );
            }

            if ( !Directory.Exists( Factory.Instance.AppDataFolder + "/tvdbartwork/poster" ) )
            {
                Directory.CreateDirectory( Factory.Instance.AppDataFolder + "/tvdbartwork/poster" );
            }
        }

        private void SeriesWorkerDoWork( object sender, DoWorkEventArgs e )
        {
            try
            {
                Factory.Instance.LogLines.Enqueue( @"Fetching TVDB metadata for " + this._seriesName + "..." );

                var tvdbwoker = new TvdbWorker( this._seriesName );
                tvdbwoker.Run();

                Thread.Sleep( 50 );

                while ( tvdbwoker.is_working() )
                {
                    Thread.Sleep( 1000 );
                }

                this._series.Add( tvdbwoker.get_series() );
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void start_artwork_downloader()
        {
            try
            {
                this._artworkWorker = new BackgroundWorker();
                this._artworkWorker.DoWork += this.ArtWorkerDoWork;
                this._artworkWorker.RunWorkerAsync();
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void AllSeriesWorkerProgressChanged( object sender, ProgressChangedEventArgs e )
        {
            try
            {
                var percent = e.ProgressPercentage/100.0;
                Factory.Instance.WindowFileBotPp.set_tvdb_progress( percent + "%" );
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void AllSeriesWorkerRunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
        {
            try
            {
                Factory.Instance.WindowFileBotPp.set_status_text( "Tvdb done..." );
                Factory.Instance.MetaDataReady += 1;
                this.start_artwork_downloader();
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void AllSeriesWorkerDoWork( object sender, DoWorkEventArgs e )
        {
            try
            {
                Factory.Instance.LogLines.Enqueue( @"Fetching TVDB metadata..." );

                var wait = true;

                foreach ( var inode in this._dirs )
                {
                    if ( this._stop )
                    {
                        break;
                    }

                    this.wait_limit_workers( 5, wait );

                    var tvdbwoker = new TvdbWorker( inode );
                    this._workers.Add( tvdbwoker );
                    wait = !tvdbwoker.is_cached();
                    tvdbwoker.Run();

                    var percent = ( this._workers.Count/( double ) this._dirs.Length )*10000.0;
                    this._allSeriesWorker.ReportProgress( ( int ) percent );
                }

                this.wait_for_workers();

                if ( this._stop )
                {
                    return;
                }

                this.get_series_from_workers();
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void ArtWorkerDoWork( object sender, DoWorkEventArgs e )
        {
            try
            {
                Factory.Instance.LogLines.Enqueue( @"Fetching TVDB metadata..." );

                foreach ( var worker in this._workers )
                {
                    var aw = worker.get_artwork_links();
                    Factory.Instance.Utils.download_file( aw[ 0 ], aw[ 1 ] );
                    Factory.Instance.Utils.download_file( aw[ 2 ], aw[ 3 ] );
                    Factory.Instance.Utils.download_file( aw[ 4 ], aw[ 5 ] );
                }
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void wait_for_workers()
        {
            try
            {
                Thread.Sleep( 50 );

                Factory.Instance.LogLines.Enqueue( @"Waiting for threads" );

                var count = 2;

                while ( count > 1 )
                {
                    count = this._workers.Count( worker => worker.is_working() );

                    Thread.Sleep( 50 );
                }

                Thread.Sleep( 50 );
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void wait_limit_workers( int num, bool wait )
        {
            try
            {
                var count = num + 1;

                while ( count > num )
                {
                    count = this._workers.Count( worker => worker.is_working() );

                    Thread.Sleep( wait ? Factory.Instance.Random.Next( 10, 40 ) : 5 );
                }

                Thread.Sleep( wait ? Factory.Instance.Random.Next( 10, 40 ) : 5 );
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
            }
        }

        protected virtual void Dispose( bool disposing )
        {
            if ( !disposing )
            {
                return;
            }

            this._allSeriesWorker.Dispose();
            this._seriesWorker.Dispose();
            this._artworkWorker.Dispose();
        }
    }
}