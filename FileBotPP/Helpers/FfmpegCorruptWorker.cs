using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using FileBotPP.Tree;

namespace FileBotPP.Helpers
{
    public class FfmpegCorruptWorker : ISupportsStop, IDisposable, IFfmpegCorruptWorker
    {
        private readonly ConcurrentQueue< IFileItem > _brokenFiles;
        private readonly IDirectoryItem _directory;
        private readonly IFileItem _fileitem;
        private int _scanItemsCount;
        private int _scannedItemsCount;
        private bool _stop;
        private BackgroundWorker _worker;

        public FfmpegCorruptWorker()
        {
            this._brokenFiles = new ConcurrentQueue< IFileItem >();
        }

        public FfmpegCorruptWorker( IFileItem file )
        {
            this._brokenFiles = new ConcurrentQueue< IFileItem >();
            this._fileitem = file;
        }

        public FfmpegCorruptWorker( IDirectoryItem directory )
        {
            this._brokenFiles = new ConcurrentQueue< IFileItem >();
            this._directory = directory;
        }

        public void stop_worker()
        {
            this._stop = true;
        }

        public void start_scan()
        {
            this._worker = new BackgroundWorker();
            this._worker.DoWork += this._worker_DoWork;
            this._worker.WorkerReportsProgress = true;
            this._worker.ProgressChanged += this._worker_ProgressChanged;
            this._worker.RunWorkerCompleted += this._worker_RunWorkerCompleted;
            this._worker.WorkerSupportsCancellation = true;
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
            Factory.Instance.WindowFileBotPp.set_status_text( "File video stream check (" + this._scannedItemsCount + "/" + this._scanItemsCount + ")" );

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

        private void scan_folder( IDirectoryItem directory )
        {
            foreach ( var item in directory.Items.OfType< IDirectoryItem >() )
            {
                this.scan_folder( item );
            }

            foreach ( var item in directory.Items.OfType< IFileItem >().Where( item => item.Missing != true ) )
            {
                this.scan_file( item );
            }
        }

        private void scan_file( IFileItem fitem )
        {
            this._scannedItemsCount += 1;

            var mi = Environment.CurrentDirectory + "\\Library\\ffmpeg.exe";
            var arguments = "-y -v info -t 5 -i \"" + fitem.Path.Replace( "\\", "/" ) + "\" -c:a copy -c:s mov_text -c:v mpeg4 -f mp4 test.mp4";

            if (Factory.Instance.Utils.write_file(Factory.Instance.AppDataFolder + "\\ffmpeg.bat", "@echo off" + Environment.NewLine + "\"" + mi + "\" " + arguments + Environment.NewLine + "EXIT /B %errorlevel%" ) == false )
            {
                return;
            }

            var output = Factory.Instance.Utils.run_process_background(Factory.Instance.AppDataFolder + "\\ffmpeg.bat", "" );

            if ( output > 0 )
            {
                this._brokenFiles.Enqueue( fitem );
                Factory.Instance.LogLines.Enqueue( "FFmpeg problem : " + fitem.Path );
            }
            else
            {
                Factory.Instance.LogLines.Enqueue( "FFmpeg looks ok : " + fitem.Path );
            }
            this._worker.ReportProgress( 1 );
        }

        protected virtual void Dispose( bool disposing )
        {
            if ( disposing )
            {
                this._worker.Dispose();
            }
        }

        public void Dispose()
        {
            this.Dispose( true );
            GC.SuppressFinalize( this );
        }
    }
}