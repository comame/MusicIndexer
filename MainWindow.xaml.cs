using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

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

            AppWindow.ResizeClient(new Windows.Graphics.SizeInt32(1000, 600));
            contentFrame.Navigate(typeof(ITLPage));
        }

        private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args) {
            if (args.IsSettingsInvoked) {
                contentFrame.Navigate(typeof(SettingsPage));
                return;
            }

            switch (args.InvokedItemContainer?.Tag as string) {
                case "ITL":
                    contentFrame.Navigate(typeof(ITLPage));
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
