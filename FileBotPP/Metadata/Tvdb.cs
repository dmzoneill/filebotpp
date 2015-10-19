using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using FileBotPP.Helpers;
using FileBotPP.Interfaces;
using FileBotPP.Metadata.tvdb;

namespace FileBotPP.Metadata
{
    public class Tvdb : ITvdb, ISupportsStop
    {
        private static readonly Random Random = new Random();
        private readonly string[] _dirs;
        private readonly List< Series > _series;
        private readonly List< TvdbWorker > _workers;
        private BackgroundWorker _mainWorker;
        private bool _stop;

        public Tvdb( string[] dirs )
        {
            this._dirs = dirs;
            this._series = new List< Series >();
            this._workers = new List< TvdbWorker >();
        }

        public void downloads_series_data()
        {
            this._mainWorker = new BackgroundWorker();
            this._mainWorker.DoWork += this._mainWorker_DoWork;
            this._mainWorker.RunWorkerCompleted += this._mainWorker_RunWorkerCompleted;
            this._mainWorker.ProgressChanged += this._mainWorker_ProgressChanged;
            this._mainWorker.WorkerReportsProgress = true;
            this._mainWorker.RunWorkerAsync();
        }

        private void _mainWorker_ProgressChanged( object sender, ProgressChangedEventArgs e )
        {
            var percent = e.ProgressPercentage/100.0;
            Common.FileBotPp.set_tvdb_progress( percent + "%" );
        }

        private void _mainWorker_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
        {
            Common.FileBotPp.set_status_text( "Tvdb done..." );
            Common.MetaDataReady += 1;
        }

        public void get_series_from_workers()
        {
            foreach ( var worker in this._workers.Where( worker => worker.get_series() != null ) )
            {
                this._series.Add( worker.get_series() );
            }

            this._workers.Clear();
        }

        private void _mainWorker_DoWork( object sender, DoWorkEventArgs e )
        {
            Utils.LogLines.Enqueue( @"Fetching TVDB metadata..." );

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
                this._mainWorker.ReportProgress( ( int ) percent );
            }

            this.wait_for_workers();
            if ( this._stop )
            {
                return;
            }
            this.get_series_from_workers();
        }

        private void wait_for_workers()
        {
            Thread.Sleep( 50 );

            Utils.LogLines.Enqueue( @"Waiting for threads" );

            var count = 2;

            while ( count > 1 )
            {
                count = this._workers.Count( worker => worker.is_working() );

                Thread.Sleep( 50 );
            }

            Thread.Sleep( 50 );
        }

        private void wait_limit_workers( int num, bool wait )
        {
            var count = num + 1;

            while ( count > num )
            {
                count = this._workers.Count( worker => worker.is_working() );

                if ( wait )
                {
                    Thread.Sleep( Random.Next( 10, 40 ) );
                }
            }

            if ( wait )
            {
                Thread.Sleep( Random.Next( 10, 40 ) );
            }
        }

        public List< Series > get_series()
        {
            return this._series;
        }

        public Series get_series_by_name( string name )
        {
            return this._series.FirstOrDefault( series => String.Compare( series.get_name(), name, StringComparison.Ordinal ) == 0 );
        }

        public void free_workers()
        {
            this._workers.Clear();
        }

        public void stop_worker()
        {
            this._stop = true;
        }
    }
}