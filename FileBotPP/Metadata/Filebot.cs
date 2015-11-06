using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using FileBotPP.Helpers;
using FileBotPP.Interfaces;
using FileBotPP.Metadata.Interfaces;
using FileBotPP.Payloads;
using FileBotPP.Payloads.Interfaces;
using FileBotPP.Tree;
using FileBotPP.Tree.Interfaces;

namespace FileBotPP.Metadata
{
    public class Filebot : IFilebot, ISupportsStop, IDisposable
    {
        private readonly List< IBadNameUpdate > _renameList;
        private int _checkedCount;
        private IDirectoryItem _checkSeasonDirectory;
        private IDirectoryItem _lastChecked;
        private bool _stop;
        private int _toCheckCount;
        private BackgroundWorker _worker;

        public Filebot()
        {
            this._renameList = new List< IBadNameUpdate >();
        }

        public void Dispose()
        {
            this.Dispose( true );
            GC.SuppressFinalize( this );
        }

        public void check_series( IDirectoryItem directory )
        {
            Common.FileBotPp.set_status_text( "Checking names in series: " + directory.FullName );
            this._checkSeasonDirectory = directory;
            this._worker = new BackgroundWorker();
            this._worker.RunWorkerCompleted += this._worker_RunWorkerCompleted;
            this._worker.ProgressChanged += this._worker_ProgressChanged;
            this._worker.DoWork += this._worker_DoWork;
            this._worker.WorkerReportsProgress = true;
            this._worker.RunWorkerAsync();
        }

        public void stop_worker()
        {
            this._stop = true;
        }

        public void check_series_all()
        {
            Common.FileBotPp.set_status_text( "Checking all series names" );
            this._worker = new BackgroundWorker();
            this._worker.RunWorkerCompleted += this._worker_RunWorkerCompleted;
            this._worker.ProgressChanged += this._worker_ProgressChanged;
            this._worker.DoWork += this._worker_DoWork;
            this._worker.WorkerReportsProgress = true;
            this._worker.RunWorkerAsync();
        }

        private void _worker_ProgressChanged( object sender, ProgressChangedEventArgs e )
        {
            Common.FileBotPp.set_status_text( "Checked (" + this._checkedCount + "/" + this._toCheckCount + ") " + this._lastChecked?.FullName );
            this.consume_queue();
        }

        private static IItem get_file_rename( IItem directory, string oldpath )
        {
            foreach ( var subdirectory in directory.Items.OfType< IDirectoryItem >() )
            {
                foreach ( var fileitem in subdirectory.Items )
                {
                    if ( String.Compare( fileitem.Path, oldpath, StringComparison.Ordinal ) == 0 )
                    {
                        return fileitem;
                    }
                }
            }

            foreach ( var fileitem in directory.Items.OfType< IFileItem >() )
            {
                if ( String.Compare( fileitem.Path, oldpath, StringComparison.Ordinal ) == 0 )
                {
                    return fileitem;
                }
            }

            return null;
        }

        private void _worker_DoWork( object sender, DoWorkEventArgs e )
        {
            if ( this._checkSeasonDirectory == null )
            {
                this._toCheckCount = ItemProvider.Items.Count;

                foreach ( var series in ItemProvider.Items )
                {
                    if ( this._stop )
                    {
                        break;
                    }

                    this.check_series_names( series );
                    this._checkedCount++;
                    this._lastChecked = ( IDirectoryItem ) series;
                    this._worker.ReportProgress( 1 );
                }
                return;
            }

            this._toCheckCount = 1;
            this._checkedCount++;
            this.check_series_names( this._checkSeasonDirectory );
            this._lastChecked = this._checkSeasonDirectory;
            this._worker.ReportProgress( 1 );
        }

        private void check_series_names( IItem directory )
        {
            var fbdirectory = directory.Path.Replace( '\\', '/' );
            var output = Utils.get_process_output( "filebot", "-r --db TheTVDB --action test -rename \"" + fbdirectory + "\" -non-strict 2> nul", 25000 );

            var renameMatches = Regex.Matches( output, @"\[TEST\] Rename \[(.*?)] to \[(.*)\]", RegexOptions.IgnoreCase );

            foreach ( Match match in renameMatches )
            {
                if ( this._stop )
                {
                    break;
                }

                var correctPath = match.Groups[ 1 ].Value.Replace( "\\\\", "\\" );
                var filerename = get_file_rename( directory, correctPath );

                if ( filerename != null )
                {
                    this._renameList.Add( new BadNameUpdate {Directory = ( IDirectoryItem ) filerename.Parent, File = ( IFileItem ) filerename, SuggestName = match.Groups[ 2 ].Value} );
                }
            }
        }

        private void _worker_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
        {
            this.consume_queue();

            Common.FileBotPp.set_status_text( "Series name check completed" );
        }

        private void consume_queue()
        {
            foreach ( var renamefile in this._renameList )
            {
                if ( this._stop )
                {
                    break;
                }

                renamefile.File.SuggestedName = renamefile.SuggestName;
                renamefile.File.BadName = true;
                renamefile.File.Update();
                renamefile.Directory.Update();
                renamefile.Directory.Parent?.Update();
            }
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