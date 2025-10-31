using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Storage.Pickers;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MusicManager {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page {
        public SettingsPage() {
            InitializeComponent();

            libraryPlacePickerText.Text = UserPreference.LibraryPath;
        }

        private async void LibraryPlacePickerButtonClick(object sender, RoutedEventArgs e) {
            libraryPlacePickerButton.IsEnabled = false;

            var picker = new FolderPicker(libraryPlacePickerButton.XamlRoot.ContentIslandEnvironment.AppWindowId);
            var folder = await picker.PickSingleFolderAsync();
            if (folder == null) {
                libraryPlacePickerButton.IsEnabled = true;
                return;
            }

            libraryPlacePickerText.Text = folder.Path;
            UserPreference.LibraryPath = folder.Path;

            libraryPlacePickerButton.IsEnabled = true;
        }

        private async void PreferenceResetButtonClick(object sender, RoutedEventArgs e) {
            var dialog = new ContentDialog() {
                XamlRoot = XamlRoot,
                PrimaryButtonText = "リセットする",
                CloseButtonText = "キャンセル",
                DefaultButton = ContentDialogButton.Primary,
            };

            var result = await dialog.ShowAsync();
            if (result != ContentDialogResult.Primary) {
                return;
            }


            UserPreference.ClearAll();
            Frame.Navigate(typeof(SettingsPage));
        }
    }
}
