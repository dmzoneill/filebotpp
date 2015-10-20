using FileBotPP.Metadata.eztv.Interfaces;

namespace FileBotPP.Metadata.eztv
{
    public class Torrent : ITorrent
    {
        public string Epname { get; set; }
        public string Magnetlink { get; set; }
        public string Series { get; set; }
        public string Imbdid { get; set; }
    }
}