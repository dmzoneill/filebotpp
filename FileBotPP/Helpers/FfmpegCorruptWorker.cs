using System;
using System.Collections.Concurrent;
using FileBotPP.Tree;

namespace FileBotPP.Helpers
{
    public class FfmpegCorruptWorker : QueuedFileWorker, IFfmpegCorruptWorker
    {
        private readonly ConcurrentQueue<IFileItem> _brokenFiles = new ConcurrentQueue<IFileItem>();
        private int _scanItemsCount;
        private int _scannedItemsCount;

        public FfmpegCorruptWorker()
            : base(null, null)
        {
        }

        public FfmpegCorruptWorker(IFileItem file)
            : base(null, file)
        {
        }

        public FfmpegCorruptWorker(IDirectoryItem directory)
            : base(directory, null)
        {
        }

        protected override bool StopDuringFolderWalk
        {
            get { return false; }
        }

        public void start_scan()
        {
            this.StartWorker(true);
        }

        protected override void SetItemCount(int count)
        {
            this._scanItemsCount = count;
        }

        protected override void ProcessFile(IFileItem fitem)
        {
            this._scannedItemsCount += 1;

            var mi = Environment.CurrentDirectory + "\\Library\\ffmpeg.exe";
            var arguments = "-y -v info -t 5 -i \"" + fitem.Path.Replace("\\", "/") + "\" -c:a copy -c:s mov_text -c:v mpeg4 -f mp4 test.mp4";

            if (Factory.Instance.Utils.write_file(Factory.Instance.AppDataFolder + "\\ffmpeg.bat", "@echo off" + Environment.NewLine + "\"" + mi + "\" " + arguments + Environment.NewLine + "EXIT /B %errorlevel%") == false)
            {
                return;
            }

            var output = Factory.Instance.Utils.run_process_background(Factory.Instance.AppDataFolder + "\\ffmpeg.bat", "");

            if (output > 0)
            {
                this._brokenFiles.Enqueue(fitem);
                Factory.Instance.LogLines.Enqueue("FFmpeg problem : " + fitem.Path);
            }
            else
            {
                Factory.Instance.LogLines.Enqueue("FFmpeg looks ok : " + fitem.Path);
            }

            this.Worker.ReportProgress(1);
        }

        protected override void DrainQueue()
        {
            QueueDrain.Drain(this._brokenFiles, "File video stream check (" + this._scannedItemsCount + "/" + this._scanItemsCount + ")", item => item.Corrupt = true);
        }
    }
}
