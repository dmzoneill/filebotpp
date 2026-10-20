using System;
using System.Collections.Concurrent;
using FileBotPP.Tree;

namespace FileBotPP.Helpers
{
    public class FfmpegConvertWorker : QueuedFileWorker, IFfmpegConvertWorker
    {
        private readonly ConcurrentQueue<IFileItem> _unconvertedFiles = new ConcurrentQueue<IFileItem>();
        private int _convertedItemsCount;
        private int _convertItemsCount;

        public FfmpegConvertWorker()
            : base(null, null)
        {
        }

        public FfmpegConvertWorker(IFileItem file)
            : base(null, file)
        {
        }

        public FfmpegConvertWorker(IDirectoryItem directory)
            : base(directory, null)
        {
        }

        public void start_convert()
        {
            this.StartWorker(false);
        }

        protected override void SetItemCount(int count)
        {
            this._convertItemsCount = count;
        }

        protected override void ProcessFile(IFileItem fitem)
        {
            this._convertedItemsCount += 1;

            var mi = Environment.CurrentDirectory + "\\Library\\ffmpeg.exe";
            var arguments = "-y -v info -i \"" + fitem.Path.Replace("\\", "/") + "\" -c:a copy -c:s mov_text -c:v mpeg4 -f mp4 \"" + fitem.Parent.Path.Replace("\\", "/") + "/" + fitem.ShortName + ".mp4\"";
            var objpath = Factory.Instance.AppDataFolder + "\\ffmpegconvert.bat";

            if (Factory.Instance.Utils.write_file(objpath, "@echo off" + Environment.NewLine + "\"" + mi + "\" " + arguments + Environment.NewLine + "EXIT /B %errorlevel%") == false)
            {
                return;
            }

            var returncode = Factory.Instance.Utils.run_process_foreground(objpath, "");

            if (returncode > 0)
            {
                this._unconvertedFiles.Enqueue(fitem);
                Factory.Instance.LogLines.Enqueue("FFmpeg convert problem : " + fitem.Path);
            }
            else
            {
                Factory.Instance.LogLines.Enqueue("FFmpeg convert ok : " + fitem.Path);
            }

            this.Worker.ReportProgress(1);
        }

        protected override void DrainQueue()
        {
            QueueDrain.Drain(this._unconvertedFiles, "Converted file (" + this._convertedItemsCount + "/" + this._convertItemsCount + ")", item => Factory.Instance.WindowFileBotPp.set_status_text("Problem converting file (" + item.FullName));
        }
    }
}
