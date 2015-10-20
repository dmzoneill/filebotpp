namespace FileBotPP.Metadata.eztv.Interfaces
{
    public interface ITorrent
    {
        string Epname { get; set; }
        string Magnetlink { get; set; }
        string Series { get; set; }
        string Imbdid { get; set; }
    }
}