using System;
using System.Collections.Concurrent;
using FileBotPP.Helpers;
using FileBotPP.Tree;

namespace FileBotPP.Metadata
{
    public class MediaInfoWorker : QueuedFileWorker, IMediaInfoWorker
    {
        private readonly ConcurrentQueue<IFileItem> _brokenFiles = new ConcurrentQueue<IFileItem>();
        private int _scanItemsCount;
        private int _scannedItemsCount;

        public MediaInfoWorker()
            : base(null, null)
        {
        }

        public MediaInfoWorker(IFileItem file)
            : base(null, file)
        {
        }

        public MediaInfoWorker(IDirectoryItem directory)
            : base(directory, null)
        {
        }

        public void start_scan()
        {
            this.StartWorker(false);
        }

        public void scan_file_one_time(IFileItem fitem)
        {
            var mibin = Environment.CurrentDirectory + "\\Library\\MediaInfo.exe";

            var output = Factory.Instance.Utils.get_process_output(mibin, "\"" + fitem.Path + "\"", 5000);

            if (output.Contains("Duration"))
            {
                // ReSharper disable once ObjectCreationAsStatement
                new MediaInfo(fitem, output);
            }
        }

        protected override void SetItemCount(int count)
        {
            this._scanItemsCount = count;
        }

        protected override void ProcessFile(IFileItem fitem)
        {
            this._scannedItemsCount += 1;
            var mibin = Environment.CurrentDirectory + "\\Library\\MediaInfo.exe";

            var output = Factory.Instance.Utils.get_process_output(mibin, "\"" + fitem.Path + "\"");

            if (output.Contains("Duration"))
            {
                // ReSharper disable once ObjectCreationAsStatement
                new MediaInfo(fitem, output);
                Factory.Instance.LogLines.Enqueue("Scanned media info successfully : " + fitem.Path);
            }
            else
            {
                this._brokenFiles.Enqueue(fitem);
                this.Worker.ReportProgress(1);
                Factory.Instance.LogLines.Enqueue("Media metadata unreadable : " + fitem.Path);
            }

            this.Worker.ReportProgress(1);
        }

        protected override void DrainQueue()
        {
            QueueDrain.Drain(this._brokenFiles, "MetaData check (" + this._scannedItemsCount + "/" + this._scanItemsCount + ")", item => item.Corrupt = true);
        }
    }
}
