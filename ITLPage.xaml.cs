using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MusicManager.Logic;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace MusicManager;

public sealed partial class ITLPage : Page {
    public ITLPage() {
        InitializeComponent();
    }

    public ITLPageViewModel ViewModel { get; } = new ITLPageViewModel();

    protected override async void OnNavigatedTo(NavigationEventArgs e) {
        base.OnNavigatedFrom(e);

        var musicCount = await Task.Run(() => Music.CountMusicFiles(UserPreference.LibraryPath));
        targetFileCountText.Text = $"{musicCount} 件の音楽ファイル";

        ITLPageService.page = this;
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e) {
        base.OnNavigatedFrom(e);

        ITLPageService.page = null;
    }

    private void ExecuteButtonClick(object sender, RoutedEventArgs e) {
        ITLPageService.ExecuteButtonClick();
    }
}

public enum TaskStatus {
    Default,
    Running,
    Completed,
    Canceled,
}

public class ITLPageViewModel : INotifyPropertyChanged {
    private static double _progress = 0.0;
    private static TaskStatus _status = TaskStatus.Default;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName) =>
       PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    // ViewModel の参照用インスタンスプロパティ
    public double Progress {
        get { return _progress; }
        set {
            _progress = value;
            OnPropertyChanged(nameof(Progress));
            OnPropertyChanged(nameof(ProgressBarVisibility));
        }
    }
    public TaskStatus Status {
        get { return _status; }
        set {
            _status = value;
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(ExecuteButtonText));
            OnPropertyChanged(nameof(DoneNoticeText));
            OnPropertyChanged(nameof(ProgressBarVisibility));
            OnPropertyChanged(nameof(DoneNoticeTextVisibility));
        }
    }


    // UI の表示用 readonly プロパティ
    public string ExecuteButtonText {
        get {
            return Status == TaskStatus.Running ? "キャンセル" : "実行";
        }
    }
    public string DoneNoticeText {
        get {
            return Status switch {
                TaskStatus.Completed => "完了",
                TaskStatus.Canceled => "キャンセルしました",
                _ => "",
            };
        }
    }
    public Visibility ProgressBarVisibility {
        get {
            return Status == TaskStatus.Running ? Visibility.Visible : Visibility.Collapsed;
        }
    }
    public Visibility DoneNoticeTextVisibility {
        get {
            return Status switch {
                TaskStatus.Completed => Visibility.Visible,
                TaskStatus.Canceled => Visibility.Visible,
                _ => Visibility.Collapsed,
            };
        }
    }
}

internal class ITLPageService {
    private static CancellationTokenSource? _cancelTokenSource = null;

    public static async void ExecuteButtonClick() {
        if (_cancelTokenSource != null) {
            _cancelTokenSource.Cancel();
            return;
        }

        _cancelTokenSource = new();
        GetViewModel().Status = TaskStatus.Running;

        var completed = await Task.Run(() => Music.SearchMusicFilesAndExportITLFile(
            _cancelTokenSource.Token,
            UserPreference.LibraryPath,
            UserPreference.LibraryPath,
            (current, total, isIndeterminate) => {
                page?.DispatcherQueue.TryEnqueue(() => {
                    if (total > 0) {
                        GetViewModel().Progress = (double)current / total * 100.0;
                    }
                });
            }
        ));

        _cancelTokenSource.Dispose();
        _cancelTokenSource = null;

        if (completed) {
            GetViewModel().Status = TaskStatus.Completed;
        } else {
            GetViewModel().Status = TaskStatus.Canceled;
        }

        GetViewModel().Progress = 0;
    }

    public static ITLPage? page;
    private static ITLPageViewModel GetViewModel() {
        return page?.ViewModel ?? new ITLPageViewModel();
    }
}