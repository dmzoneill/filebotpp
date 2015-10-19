using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using FileBotPP.Helpers;
using FileBotPP.Interfaces;

namespace FileBotPP.Metadata
{
    public class Eztv : ISupportsStop, IEztv
    {
        private static readonly Random Random = new Random();
        private readonly List< ITorrent > _torrents;
        private readonly List< IEztvWorker > _workers;
        private BackgroundWorker _mainWorker;
        private bool _stop;

        public Eztv()
        {
            this._torrents = new List< ITorrent >();
            this._workers = new List< IEztvWorker >();
        }

        public void downloads_series_data()
        {
            this._mainWorker = new BackgroundWorker();
            this._mainWorker.RunWorkerCompleted += _mainWorker_RunWorkerCompleted;
            this._mainWorker.ProgressChanged += _mainWorker_ProgressChanged;
            this._mainWorker.DoWork += this._mainWorker_DoWork;
            this._mainWorker.WorkerReportsProgress = true;
            this._mainWorker.RunWorkerAsync();
        }

        public List< ITorrent > get_torrents()
        {
            return this._torrents;
        }

        public void get_series_from_workers()
        {
            foreach ( var worker in this._workers )
            {
                var torrents = worker.get_torrents();

                if ( torrents == null )
                {
                    continue;
                }

                foreach ( var torrent in torrents )
                {
                    this._torrents.Add( torrent );
                }
            }

            this._workers.Clear();
        }

        public void free_workers()
        {
            this._workers.Clear();
        }

        public void stop_worker()
        {
            this._stop = true;
        }

        private void _mainWorker_DoWork( object sender, DoWorkEventArgs e )
        {
            Utils.LogLines.Enqueue( @"Fetching EZTV metadata..." );

            var data = Utils.Fetch( "https://eztv.ag" );

            var selectMatch = Regex.Match( data, "<select.*?>(.*?)</select>", RegexOptions.IgnoreCase | RegexOptions.Singleline );
            if ( selectMatch.Success == false )
            {
                return;
            }

            var optionMatches = Regex.Matches( selectMatch.Value, "<option value=\"(.*?)\">(.*?)</option>", RegexOptions.IgnoreCase | RegexOptions.Singleline );

            var wait = true;

            foreach ( Match option in optionMatches )
            {
                if ( this._stop )
                {
                    break;
                }

                if ( String.Compare( option.Groups[ 1 ].Value, "", StringComparison.Ordinal ) == 0 )
                {
                    continue;
                }

                this.wait_limit_workers( 5, wait );

                var eztvwoker = new EztvWorker( int.Parse( option.Groups[ 1 ].Value ), option.Groups[ 2 ].Value );
                this._workers.Add( eztvwoker );
                wait = !eztvwoker.is_cached();
                eztvwoker.Run();

                var percent = ( this._workers.Count/( double ) optionMatches.Count )*10000.0;
                this._mainWorker.ReportProgress( ( int ) percent );
            }

            this.wait_for_workers();
            if ( this._stop )
            {
                return;
            }
            this.get_series_from_workers();
        }

        private static void _mainWorker_ProgressChanged( object sender, ProgressChangedEventArgs e )
        {
            var percent = e.ProgressPercentage/100.0;
            Common.FileBotPp.set_eztv_progress( percent + "%" );
        }

        private static void _mainWorker_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
        {
            Common.FileBotPp.set_eztv_progress( "100%" );
            Common.FileBotPp.set_status_text( "Eztv done..." );
            Common.MetaDataReady += 1;
        }

        private void wait_for_workers()
        {
            Thread.Sleep( Random.Next( 10, 40 ) );

            Utils.LogLines.Enqueue( "Waiting for threads" );

            var count = 2;

            while ( count > 1 )
            {
                count = this._workers.Count( worker => worker.is_working() );

                Thread.Sleep( Random.Next( 10, 40 ) );
            }
        }

        private void wait_limit_workers( int num, bool wait )
        {
            var count = num + 1;

            while ( count > num )
            {
                count = this._workers.Count( worker => worker.is_working() );

                Thread.Sleep( wait ? Random.Next( 10, 40 ) : 5 );
            }

            Thread.Sleep( wait ? Random.Next( 10, 40 ) : 5 );
        }
    }
}