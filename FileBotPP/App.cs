using System.Windows;
using MahApps.Metro;

namespace FileBotPP
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup( StartupEventArgs e )
        {
            // get the theme from the current application
            ThemeManager.DetectAppStyle( Current );

            // now set the Green accent and dark theme
            ThemeManager.ChangeAppStyle( Current, ThemeManager.GetAccent( "Steel" ), ThemeManager.GetAppTheme( "BaseLight" ) );

            base.OnStartup( e );
        }
    }
}