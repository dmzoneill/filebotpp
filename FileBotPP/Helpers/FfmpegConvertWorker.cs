using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using FileBotPP.Tree;

namespace FileBotPP.Helpers
{
    public class FfmpegConvertWorker : ISupportsStop, IDisposable, IFfmpegConvertWorker
    {
        private readonly IDirectoryItem _directory;
        private readonly IFileItem _fileitem;
        private readonly ConcurrentQueue< IFileItem > _unconvertedFiles;
        private int _convertedItemsCount;
        private int _convertItemsCount;
        private bool _stop;
        private BackgroundWorker _worker;

        public FfmpegConvertWorker()
        {
            this._unconvertedFiles = new ConcurrentQueue< IFileItem >();
        }

        public FfmpegConvertWorker( IFileItem file )
        {
            this._unconvertedFiles = new ConcurrentQueue< IFileItem >();
            this._fileitem = file;
        }

        public FfmpegConvertWorker( IDirectoryItem directory )
        {
            this._unconvertedFiles = new ConcurrentQueue< IFileItem >();
            this._directory = directory;
        }

        public void Dispose()
        {
            this.Dispose( true );
            GC.SuppressFinalize( this );
        }

        public void stop_worker()
        {
            this._stop = true;
        }

        public void start_convert()
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
            Factory.Instance.WindowFileBotPp.set_status_text( "Converted file (" + this._convertedItemsCount + "/" + this._convertItemsCount + ")" );

            IFileItem item;
            while ( this._unconvertedFiles.TryDequeue( out item ) )
            {
                Factory.Instance.WindowFileBotPp.set_status_text( "Problem converting file (" + item.FullName );
            }
        }

        private void _worker_DoWork( object sender, DoWorkEventArgs e )
        {
            if ( this._directory == null && this._fileitem == null )
            {
                this._convertItemsCount = ItemProvider.Count();
                foreach ( var dir in ItemProvider.Items.OfType< IDirectoryItem >().ToArray() )
                {
                    if ( this._stop )
                    {
                        return;
                    }
                    this.convert_folder( dir );
                }
            }

            if ( this._directory != null )
            {
                if ( this._stop )
                {
                    return;
                }
                this._convertItemsCount = this._directory.Count;
                this.convert_folder( this._directory );
            }

            if ( this._fileitem != null )
            {
                if ( this._stop )
                {
                    return;
                }
                this._convertItemsCount = this._fileitem.Count;
                this.convert_file( this._fileitem );
            }
        }

        private void convert_folder( IItem directory )
        {
            foreach ( var item in directory.Items.OfType< IDirectoryItem >() )
            {
                if ( this._stop )
                {
                    return;
                }
                this.convert_folder( item );
            }

            foreach ( var item in directory.Items.OfType< IFileItem >().Where( item => item.Missing != true ) )
            {
                if ( this._stop )
                {
                    return;
                }
                this.convert_file( item );
            }
        }

        private void convert_file( IFileItem fitem )
        {
            this._convertedItemsCount += 1;

            var mi = Environment.CurrentDirectory + "\\Library\\ffmpeg.exe";
            var arguments = "-y -v info -i \"" + fitem.Path.Replace( "\\", "/" ) + "\" -c:a copy -c:s mov_text -c:v mpeg4 -f mp4 \"" + fitem.Parent.Path.Replace( "\\", "/" ) + "/" + fitem.ShortName + ".mp4\"";
            var objpath = Factory.Instance.AppDataFolder + "\\ffmpegconvert.bat";

            if (Factory.Instance.Utils.write_file( objpath, "@echo off" + Environment.NewLine + "\"" + mi + "\" " + arguments + Environment.NewLine + "EXIT /B %errorlevel%" ) == false )
            {
                return;
            }

            var returncode = Factory.Instance.Utils.run_process_foreground( objpath, "" );

            if ( returncode > 0 )
            {
                this._unconvertedFiles.Enqueue( fitem );
                Factory.Instance.LogLines.Enqueue( "FFmpeg convert problem : " + fitem.Path );
            }
            else
            {
                Factory.Instance.LogLines.Enqueue( "FFmpeg convert ok : " + fitem.Path );
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
    }
}