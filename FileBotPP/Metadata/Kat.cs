using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using FileBotPP.Helpers;

namespace FileBotPP.Metadata
{
    public class Kat : ISupportsStop, IKat, IDisposable
    {
        private readonly List< IKatWorker > _workers;
        private BackgroundWorker _mainWorker;
        private bool _stop;

        public Kat()
        {
            this._workers = new List< IKatWorker >();
        }

        public void Dispose()
        {
            this.Dispose( true );
            GC.SuppressFinalize( this );
        }

        public void downloads_series_data()
        {
            this._mainWorker = new BackgroundWorker();
            this._mainWorker.RunWorkerCompleted += this._mainWorker_RunWorkerCompleted;
            this._mainWorker.ProgressChanged += this._mainWorker_ProgressChanged;
            this._mainWorker.DoWork += this._mainWorker_DoWork;
            this._mainWorker.WorkerReportsProgress = true;
            this._mainWorker.RunWorkerAsync();
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
            Factory.Instance.LogLines.Enqueue( @"Fetching Kat metadata..." );

            var data = Factory.Instance.Utils.FetchDeCompressed( "https://kat.cr/tv/show/" );

            if ( data == null )
            {
                return;
            }

            var optionMatches = Regex.Matches( data, "<a class=\"plain\" href=\"(.*?)\">(.*?)</a>", RegexOptions.IgnoreCase | RegexOptions.Singleline );

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

                var katwoker = new KatWorker( option.Groups[ 1 ].Value, option.Groups[ 2 ].Value );
                this._workers.Add( katwoker );
                wait = !katwoker.is_cached();
                katwoker.Run();

                var percent = ( this._workers.Count/( double ) optionMatches.Count )*10000.0;
                this._mainWorker.ReportProgress( ( int ) percent );
            }

            this.wait_for_workers();

            if ( this._stop )
            {
                return;
            }
        }

        private void _mainWorker_ProgressChanged( object sender, ProgressChangedEventArgs e )
        {
            var percent = e.ProgressPercentage/100.0;
            Factory.Instance.WindowFileBotPp.set_kat_progress( percent + "%" );
        }

        private void _mainWorker_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
        {
            Factory.Instance.WindowFileBotPp.set_kat_progress( "100%" );
            Factory.Instance.WindowFileBotPp.set_status_text( "Kat done..." );
            Factory.Instance.MetaDataReady += 1;
        }

        private void wait_for_workers()
        {
            Thread.Sleep( Factory.Instance.Random.Next( 10, 40 ) );

            Factory.Instance.LogLines.Enqueue( "Waiting for threads" );

            var count = 2;

            while ( count > 1 )
            {
                count = this._workers.Count( worker => worker.is_working() );

                Thread.Sleep( Factory.Instance.Random.Next( 10, 40 ) );
            }
        }

        private void wait_limit_workers( int num, bool wait )
        {
            var count = num + 1;

            while ( count > num )
            {
                count = this._workers.Count( worker => worker.is_working() );

                Thread.Sleep( wait ? Factory.Instance.Random.Next( 10, 40 ) : 5 );
            }

            Thread.Sleep( wait ? Factory.Instance.Random.Next( 10, 40 ) : 5 );
        }

        protected virtual void Dispose( bool disposing )
        {
            if ( disposing )
            {
                this._mainWorker.Dispose();
            }
        }
    }
}