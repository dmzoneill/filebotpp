using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FileBotPP.Helpers;

namespace FileBotPP.Metadata
{
    internal class KatWorker : IKatWorker
    {
        private readonly string _serieslink;
        private readonly string _seriesname;
        private readonly string _seriesnameClean;
        private string _imdbid;
        private bool _working;

        public KatWorker(string link, string seriesname)
        {
            this._serieslink = link;
            this._seriesname = seriesname;

            var invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            this._seriesnameClean = invalid.Aggregate(this._seriesname, (current, c) => current.Replace(c.ToString(), ""));
        }

        public void Run()
        {
            this._working = true;
            BackgroundTasks.RunOnce(this.get_series_data, () => this._working = false);
        }

        public bool is_working()
        {
            return this._working;
        }

        public string get_series_name()
        {
            return this._seriesname;
        }

        public bool is_cached()
        {
            try
            {
                return CacheFiles.IsFresh(Factory.Instance.AppDataFolder + "/kat/" + this._seriesnameClean);
            }
            catch (Exception ex)
            {
                AppLog.Exception(ex);
                return false;
            }
        }

        private void get_series_data()
        {
            var folder = Factory.Instance.AppDataFolder + "/kat/";
            CacheFiles.EnsureDirectory(folder);

            var tempFile = folder + this._seriesnameClean + ".tmp";
            var cached = CacheFiles.ReadIfFresh(tempFile, true);
            if (cached != null)
            {
                this.parse_imdb_id(cached);
                this.parse_episodes_pages();
                return;
            }

            Factory.Instance.LogLines.Enqueue(@"Downloading " + this._seriesname + @" torrents...");

            var temp = Factory.Instance.Utils.FetchDeCompressed("https://kat.cr" + this._serieslink);

            if (String.Compare(temp, "", StringComparison.Ordinal) == 0 || temp == null)
            {
                return;
            }

            if (Factory.Instance.Utils.write_file(tempFile, temp) == false)
            {
                return;
            }

            this.parse_imdb_id(temp);
            this.parse_episodes_pages();
        }

        private void parse_imdb_id(string html)
        {
            var imdbmatch = Regex.Match(html, "http://www.imdb.com/title/(tt.*?)/", RegexOptions.IgnoreCase);

            this._imdbid = imdbmatch.Success ? imdbmatch.Groups[1].Value : null;
        }

        private void parse_episodes_pages()
        {
            if (this._imdbid == null)
            {
                Factory.Instance.LogLines.Enqueue(@"skipping " + this._seriesname + @", no imdb id...");
                return;
            }

            Factory.Instance.LogLines.Enqueue(@"Parsing " + this._seriesnameClean + @" metadata...");

            CacheFiles.EnsureDirectory(Factory.Instance.AppDataFolder + "/kat/" + this._seriesnameClean);

            for (var x = 1; x < this.get_series_torrents_pages_count(); x++)
            {
                var torrentPage = this.get_series_torrent_page(x);
                if (String.Compare(torrentPage, "", StringComparison.Ordinal) == 0)
                {
                    continue;
                }
                this.parse_page_torrents(torrentPage);
            }
        }

        private int get_series_torrents_pages_count()
        {
            var torrentPage = this.get_series_torrent_page(1);

            if (String.Compare(torrentPage, "", StringComparison.Ordinal) == 0)
            {
                return 1;
            }

            var pagesButtons = Regex.Matches(torrentPage, "<a rel=\"nofollow\" href=\".*?\" class=\"turnoverButton siteButton bigButton\">(.*?)</a>", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (pagesButtons.Count <= 0)
            {
                return 1;
            }

            var lastPage = pagesButtons[pagesButtons.Count - 1];
            return int.Parse(lastPage.Groups[1].Value);
        }

        private string get_series_torrent_page(int page)
        {
            var tempFile = Factory.Instance.AppDataFolder + "/kat/" + this._seriesnameClean + "/page" + page;
            string torrentPage;

            if (File.Exists(tempFile) == false)
            {
                torrentPage = Factory.Instance.Utils.FetchDeCompressed("https://kat.cr" + this._serieslink + "torrents/");

                if (torrentPage == null)
                {
                    return "";
                }

                Factory.Instance.Utils.write_file(tempFile, torrentPage);
                return torrentPage;
            }

            if (CacheFiles.IsWithinTimeout(tempFile))
            {
                return File.ReadAllText(tempFile);
            }

            torrentPage = Factory.Instance.Utils.FetchDeCompressed("https://kat.cr" + this._serieslink + "torrents/");

            if (torrentPage == null)
            {
                return "";
            }

            File.Delete(tempFile);

            return torrentPage;
        }

        private void parse_page_torrents(string page)
        {
            TorrentRows.AddFromHtml(page, "<a href=\".*?\" class=\"cellMainLink\">(.*?)</a>", this._seriesname, this._imdbid);
        }
    }
}