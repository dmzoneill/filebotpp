using System;
using System.ComponentModel;
using FileBotPP.Tree;

namespace FileBotPP.Helpers
{
    public abstract class QueuedFileWorker : ISupportsStop, IDisposable
    {
        private readonly IDirectoryItem _directory;
        private readonly IFileItem _fileitem;
        private BackgroundWorker _worker;
        protected bool StopRequested;

        protected QueuedFileWorker( IDirectoryItem directory, IFileItem file )
        {
            this._directory = directory;
            this._fileitem = file;
        }

        protected BackgroundWorker Worker
        {
            get { return this._worker; }
        }

        protected IDirectoryItem SelectedDirectory
        {
            get { return this._directory; }
        }

        protected IFileItem SelectedFile
        {
            get { return this._fileitem; }
        }

        protected virtual bool StopDuringFolderWalk
        {
            get { return true; }
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

        protected void StartWorker( bool supportsCancellation )
        {
            this._worker = BackgroundTasks.Start( this.OnDoWork, ( sender, args ) => this.DrainQueue(), ( sender, args ) => this.DrainQueue(), supportsCancellation );
        }

        protected abstract void SetItemCount( int count );
        protected abstract void ProcessFile( IFileItem file );
        protected abstract void DrainQueue();

        protected virtual void ProcessFolder( IItem directory )
        {
            ItemTreeScan.WalkChildren( directory, () => this.StopRequested, this.StopDuringFolderWalk, this.ProcessFolder, this.ProcessFile );
        }

        protected virtual void Dispose( bool disposing )
        {
            if ( disposing )
            {
                this._worker.Dispose();
            }
        }

        private void OnDoWork( object sender, DoWorkEventArgs e )
        {
            ItemTreeScan.Run( this.SelectedDirectory, this.SelectedFile, () => this.StopRequested, this.SetItemCount, this.ProcessFolder, this.ProcessFile );
        }
    }
}
