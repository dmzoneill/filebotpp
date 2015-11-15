using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using FileBotPP.Helpers;
using FileBotPP.Tree;

namespace FileBotPP.Metadata
{
    public class MediaInfoWorker : ISupportsStop, IMediaInfoWorker, IDisposable
    {
        private readonly ConcurrentQueue< IFileItem > _brokenFiles;
        private readonly IDirectoryItem _directory;
        private readonly IFileItem _fileitem;
        private int _scanItemsCount;
        private int _scannedItemsCount;
        private bool _stop;
        private BackgroundWorker _worker;

        public MediaInfoWorker()
        {
            this._brokenFiles = new ConcurrentQueue< IFileItem >();
        }

        public MediaInfoWorker( IFileItem file )
        {
            this._brokenFiles = new ConcurrentQueue< IFileItem >();
            this._fileitem = file;
        }

        public MediaInfoWorker( IDirectoryItem directory )
        {
            this._brokenFiles = new ConcurrentQueue< IFileItem >();
            this._directory = directory;
        }

        public void start_scan()
        {
            this._worker = new BackgroundWorker();
            this._worker.DoWork += this._worker_DoWork;
            this._worker.WorkerReportsProgress = true;
            this._worker.ProgressChanged += this._worker_ProgressChanged;
            this._worker.RunWorkerCompleted += this._worker_RunWorkerCompleted;
            this._worker.RunWorkerAsync();
        }

        private void _worker_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
        {
            this.consume_queue();
        }

        private void _worker_ProgressChanged( object sender, ProgressChangedEventArgs e )
        {
            this.consume_queue();
        }

        private void consume_queue()
        {
            Factory.Instance.WindowFileBotPp.set_status_text( "MetaData check (" + this._scannedItemsCount + "/" + this._scanItemsCount + ")" );

            IFileItem item;
            while ( this._brokenFiles.TryDequeue( out item ) )
            {
                item.Corrupt = true;
            }
        }

        private void _worker_DoWork( object sender, DoWorkEventArgs e )
        {
            if ( this._directory == null && this._fileitem == null )
            {
                this._scanItemsCount = ItemProvider.Count();
                foreach ( var dir in ItemProvider.Items.OfType< IDirectoryItem >().ToArray() )
                {
                    if ( this._stop )
                    {
                        return;
                    }
                    this.scan_folder( dir );
                }
            }

            if ( this._directory != null )
            {
                if ( this._stop )
                {
                    return;
                }
                this._scanItemsCount = this._directory.Count;
                this.scan_folder( this._directory );
            }

            if ( this._fileitem != null )
            {
                if ( this._stop )
                {
                    return;
                }
                this._scanItemsCount = this._fileitem.Count;
                this.scan_file( this._fileitem );
            }
        }

        private void scan_folder( IItem directory )
        {
            foreach ( var item in directory.Items.OfType< IDirectoryItem >() )
            {
                if ( this._stop )
                {
                    return;
                }
                this.scan_folder( item );
            }

            foreach ( var item in directory.Items.OfType< IFileItem >().Where( item => item.Missing != true ) )
            {
                if ( this._stop )
                {
                    return;
                }
                this.scan_file( item );
            }
        }

        private void scan_file( IFileItem fitem )
        {
            this._scannedItemsCount += 1;
            var mibin = Environment.CurrentDirectory + "\\Library\\MediaInfo.exe";

            var output = Factory.Instance.Utils.get_process_output( mibin, "\"" + fitem.Path + "\"" );

            if ( output.Contains( "Duration" ) )
            {
                // ReSharper disable once ObjectCreationAsStatement
                new MediaInfo( fitem, output );
                Factory.Instance.LogLines.Enqueue( "Scanned media info successfuly : " + fitem.Path );
            }
            else
            {
                this._brokenFiles.Enqueue( fitem );
                this._worker.ReportProgress( 1 );
                Factory.Instance.LogLines.Enqueue( "Media metadata unreadable : " + fitem.Path );
            }
            this._worker.ReportProgress( 1 );
        }

        public static void scan_file_one_time( IFileItem fitem )
        {
            var mibin = Environment.CurrentDirectory + "\\Library\\MediaInfo.exe";

            var output = Factory.Instance.Utils.get_process_output( mibin, "\"" + fitem.Path + "\"", 5000 );

            if ( output.Contains( "Duration" ) )
            {
                // ReSharper disable once ObjectCreationAsStatement
                new MediaInfo( fitem, output );
            }
        }

        public void stop_worker()
        {
            this._stop = true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._worker.Dispose();
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}