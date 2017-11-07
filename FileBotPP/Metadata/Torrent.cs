namespace FileBotPP.Metadata
{
    public class Torrent : ITorrent
    {
        private string _epname;

        public string Epname
        {
            get { return this._epname; }
            set
            {
                this._epname = value;
                this.EpnameLower = this._epname.ToLower();
            }
        }

        public string EpnameLower { get; set; }
        public string Magnetlink { get; set; }
        public string Series { get; set; }
        public string Imbdid { get; set; }
    }
}