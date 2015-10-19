using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using FileBotPP.Helpers;
using FileBotPP.Interfaces;
using FileBotPP.Tree;

namespace FileBotPP.Metadata
{
    public class Filebot : IFilebot
    {
        private readonly List< IBadNameUpdate > _renameList;
        private IDirectoryItem _checkSeasonDirectory;
        private BackgroundWorker _worker;

        public Filebot()
        {
            this._renameList = new List< IBadNameUpdate >();
        }

        public void check_series( IDirectoryItem directory )
        {
            Common.FileBotPp.set_status_text( "Checking names in series: " + directory.FullName );
            this._checkSeasonDirectory = directory;
            this._worker = new BackgroundWorker();
            this._worker.RunWorkerCompleted += this._worker_RunWorkerCompleted;
            this._worker.DoWork += this._worker_DoWork;
            this._worker.RunWorkerAsync();
        }

        private IItem get_file_rename( string oldpath )
        {
            foreach ( var subdirectory in this._checkSeasonDirectory.Items.OfType< IDirectoryItem >() )
            {
                foreach ( var fileitem in subdirectory.Items )
                {
                    if ( String.Compare( fileitem.Path, oldpath, StringComparison.Ordinal ) == 0 )
                    {
                        return fileitem;
                    }
                }
            }

            foreach ( var fileitem in this._checkSeasonDirectory.Items.OfType< IFileItem >() )
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
            var fbdirectory = this._checkSeasonDirectory.Path.Replace( '\\', '/' );
            var output = Utils.get_process_output( "filebot", "-r --db TheTVDB --action test -rename \"" + fbdirectory + "\" -non-strict 2> nul", 25000 );

            var renameMatches = Regex.Matches( output, @"\[TEST\] Rename \[(.*?)] to \[(.*)\]", RegexOptions.IgnoreCase );

            foreach ( Match match in renameMatches )
            {
                var correctPath = match.Groups[ 1 ].Value.Replace( "\\\\", "\\" );
                var filerename = this.get_file_rename( correctPath );

                if ( filerename != null )
                {
                    this._renameList.Add( new BadNameUpdate {Directory = ( IDirectoryItem ) filerename.Parent, File = ( IFileItem ) filerename, SuggestName = match.Groups[ 2 ].Value} );
                }
            }
        }

        private void _worker_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
        {
            foreach ( var renamefile in this._renameList )
            {
                renamefile.File.SuggestedName = renamefile.SuggestName;
                renamefile.File.BadName = true;
                renamefile.File.Update();
                renamefile.Directory.Update();
                renamefile.Directory.Parent?.Update();
            }

            Common.FileBotPp.set_status_text( "Completed name check: " + this._checkSeasonDirectory.FullName );
        }
    }
}