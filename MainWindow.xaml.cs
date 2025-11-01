using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MusicManager {
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();

            ExtendsContentIntoTitleBar = true;

            AppWindow.ResizeClient(new Windows.Graphics.SizeInt32(800, 600));
            var pos = UserPreference.WindowPosition;
            if (pos != null) {
                AppWindow.Move(new Windows.Graphics.PointInt32(pos.Value.Item1, pos.Value.Item2));
            }

            AppWindow.Changed += (sender, args) => {
                if (args.DidPositionChange) {
                    var p = AppWindow.Position;
                    UserPreference.WindowPosition = (p.X, p.Y);
                }
            };

            contentFrame.Navigate(typeof(LibraryPage));
        }

        private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args) {
            if (args.IsSettingsInvoked) {
                contentFrame.Navigate(typeof(SettingsPage));
                return;
            }

            switch (args.InvokedItemContainer?.Tag as string) {
                case "Library":
                    contentFrame.Navigate(typeof(LibraryPage));
                    break;
                case "Sync":
                    contentFrame.Navigate(typeof(SyncPage));
                    break;
            }
        }

        private void TitleBar_PaneToggleRequested(TitleBar sender, object args) {
            navigationView.IsPaneOpen = !navigationView.IsPaneOpen;
        }
    }
}
