using System;
using System.Windows.Media;

namespace FileBotPP.Tree
{
    [Flags]
    public enum ItemStatusMask : short
    {
        None = 0,
        Empty = 1,
        Corrupted = 2,
        BadLocation = 4,
        Missing = 8,
        Quality = 16,
        DisallowedType = 32,
        BadName = 64,
        Extra = 128
    };

    public static class ItemStatus
    {
        static ItemStatus()
        {
            EmptyColour = Brushes.Black;
            EmptyTooltip = "Empty folder";

            CorruptedColour = Brushes.Red;
            CorruptedTooltip = "Corrupted";

            BadLocationColour = Brushes.Blue;
            BadLocationTooltip = "Bad location";

            MissingColour = Brushes.Green;
            MissingTooltip = "Missing";

            QualityColour = Brushes.DarkOrange;
            QualityTooltip = "Poor quality";

            DisallowedTypeColour = Brushes.Yellow;
            DisallowedTypeTooltip = "Disallowed file type";

            BadNameColour = Brushes.DeepSkyBlue;
            BadNameTooltip = "Bad name";

            ExtraColour = Brushes.LawnGreen;
            ExtraTooltip = "Extra files/seasons not in TVDB";

            TorrentColour = Brushes.LightCoral;
            TorrentTooltip = "Torrent available";

            OkColour = Brushes.White;
            OkTooltip = "Ok";
        }

        public static Brush EmptyColour { get; set; }
        public static string EmptyTooltip { get; set; }
        public static Brush CorruptedColour { get; set; }
        public static string CorruptedTooltip { get; set; }
        public static Brush QualityColour { get; set; }
        public static string QualityTooltip { get; set; }
        public static Brush MissingColour { get; set; }
        public static string MissingTooltip { get; set; }
        public static Brush BadLocationColour { get; set; }
        public static string BadLocationTooltip { get; set; }
        public static Brush BadNameColour { get; set; }
        public static string BadNameTooltip { get; set; }
        public static Brush OkColour { get; set; }
        public static string OkTooltip { get; set; }
        public static Brush DisallowedTypeColour { get; set; }
        public static string DisallowedTypeTooltip { get; set; }
        public static Brush ExtraColour { get; set; }
        public static string ExtraTooltip { get; set; }
        public static Brush TorrentColour { get; set; }
        public static string TorrentTooltip { get; set; }
    }
}