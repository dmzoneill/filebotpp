using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace FileBotPP.Helpers
{
    public abstract class SeriesMetadataFetch< TWorker > : ISupportsStop, IDisposable
    {
        private BackgroundWorker _mainWorker;
        protected readonly List< TWorker > Workers;
        protected bool StopRequested;

        protected SeriesMetadataFetch()
        {
            this.Workers = new List< TWorker >();
        }

        public void downloads_series_data()
        {
            this._mainWorker = BackgroundTasks.Start( this.DoWork, this.OnProgress, this.OnCompleted, false );
        }

        public void free_workers()
        {
            this.Workers.Clear();
        }

        public void stop_worker()
        {
            this.StopRequested = true;
        }

        public void Dispose()
        {
            this.Dispose( true );
            GC.SuppressFinalize( this );
        }

        protected abstract string LogMessage { get; }
        protected abstract MatchCollection LoadMatches();
        protected abstract TWorker CreateWorker( Match option );
        protected abstract bool IsCached( TWorker worker );
        protected abstract void RunWorker( TWorker worker );
        protected abstract bool IsWorking( TWorker worker );
        protected abstract void ReportFetchProgress( double percent );
        protected abstract void ReportFinished();

        protected virtual void Dispose( bool disposing )
        {
            if ( disposing )
            {
                this._mainWorker.Dispose();
            }
        }

        private void DoWork( object sender, DoWorkEventArgs e )
        {
            Factory.Instance.LogLines.Enqueue( this.LogMessage );

            var optionMatches = this.LoadMatches();
            if ( optionMatches == null )
            {
                return;
            }

            var wait = true;

            foreach ( Match option in optionMatches )
            {
                if ( this.StopRequested )
                {
                    break;
                }

                if ( String.Compare( option.Groups[ 1 ].Value, "", StringComparison.Ordinal ) == 0 )
                {
                    continue;
                }

                WorkerThrottle.Limit( () => this.Workers.Count( this.IsWorking ), 5, wait );

                var worker = this.CreateWorker( option );
                this.Workers.Add( worker );
                wait = !this.IsCached( worker );
                this.RunWorker( worker );

                var percent = ( this.Workers.Count/( double ) optionMatches.Count )*10000.0;
                this._mainWorker.ReportProgress( ( int ) percent );
            }

            WorkerThrottle.WaitForIdle( () => this.Workers.Count( this.IsWorking ) );
        }

        private void OnProgress( object sender, ProgressChangedEventArgs e )
        {
            this.ReportFetchProgress( e.ProgressPercentage/100.0 );
        }

        private void OnCompleted( object sender, RunWorkerCompletedEventArgs e )
        {
            this.ReportFinished();
            Factory.Instance.MetaDataReady += 1;
        }
    }
}
