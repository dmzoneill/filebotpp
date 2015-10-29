using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FileBotPP.Helpers;
using FileBotPP.Tree;

namespace FileBotPP
{
    /// <summary>
    ///     Interaction logic for AddSeriesWindow.xaml
    /// </summary>
    public partial class AddSeriesWindow
    {
        public AddSeriesWindow()
        {
            this.InitializeComponent();
        }

        private void Grid_Loaded( object sender, RoutedEventArgs e )
        {
            Common.AddSeriesName = null;
            this.PopulateComboBox();
        }

        private void PopulateComboBox()
        {
            if ( Common.Eztv == null )
            {
                return;
            }

            var got = ItemProvider.Items.Select( item => item.FullName ).ToList();
            var want = new List<string>();

            foreach ( var torrent in Common.Eztv.get_torrents().Where( torrent => got.Contains( torrent.Series ) == false ).Where( torrent => want.Contains( torrent.Series ) == false ) )
            {
                want.Add(torrent.Series);
            }

            foreach ( var gotitem in want)
            {
                this.ComboBox.Items.Add( new ComboBoxItem {Content = gotitem} );
            }
        }

        private void Button_Click( object sender, RoutedEventArgs e )
        {
            Common.AddSeriesName = this.ComboBox.Text;
            this.Close();
        }
    }
}