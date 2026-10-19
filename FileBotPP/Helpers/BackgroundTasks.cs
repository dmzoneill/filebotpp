using System;
using System.ComponentModel;

namespace FileBotPP.Helpers
{
    public static class BackgroundTasks
    {
        public static BackgroundWorker Start( DoWorkEventHandler work, ProgressChangedEventHandler progress, RunWorkerCompletedEventHandler completed, bool supportsCancellation )
        {
            var worker = new BackgroundWorker();
            worker.DoWork += work;
            worker.WorkerReportsProgress = true;
            worker.ProgressChanged += progress;
            worker.RunWorkerCompleted += completed;

            if ( supportsCancellation )
            {
                worker.WorkerSupportsCancellation = true;
            }

            worker.RunWorkerAsync();
            return worker;
        }

        public static void RunOnce( Action work, Action completed )
        {
            var worker = new BackgroundWorker();
            worker.DoWork += ( sender, args ) => work();
            worker.RunWorkerCompleted += ( sender, args ) => completed();
            worker.RunWorkerAsync();
        }
    }
}
