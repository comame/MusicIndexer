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

internal sealed partial class LibraryPage : Page {
    public LibraryPage() {
        InitializeComponent();
    }

    public IndexViewModel IndexViewModel { get; } = new IndexViewModel();

    protected override void OnNavigatedTo(NavigationEventArgs e) {
        base.OnNavigatedFrom(e);

        IndexService.page = this;

        IndexViewModel.Library = MusicIndexer.LoadFromIndexFile();

        var musicCount = MusicIndexer.CountMusicFiles(UserPreference.LibraryPath);
        targetFileCountText.Text = $"{musicCount} ���̉��y�t�@�C��";
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e) {
        base.OnNavigatedFrom(e);

        IndexService.page = null;
    }

    private void ITLExecuteButtonClick(object sender, RoutedEventArgs e) {
        var l = IndexViewModel.Library;
        if (l == null) {
            ShowITLDoneNoticeText("�C���f�b�N�X������܂���");
            return;
        }
        MusicIndexer.GenerateITLFile(in l);
        ShowITLDoneNoticeText("����");
    }

    private void IndexExecuteButton_Click(object sender, RoutedEventArgs e) {
        IndexService.ExecuteButtonClick();
    }
    private async void ShowITLDoneNoticeText(string content) {
        itlDoneNoticeText.Text = content;
        itlDoneNoticeText.Visibility = Visibility.Visible;
        await Task.Delay(5 * 1000);
        itlDoneNoticeText.Visibility = Visibility.Collapsed;
    }
}

public enum TaskStatus {
    Default,
    Running,
    Completed,
    Canceled,
}

internal partial class IndexViewModel : INotifyPropertyChanged {
    private static double _progress = 0.0;
    private static TaskStatus _status = TaskStatus.Default;
    private static MusicLibrary? _library = null;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName) =>
       PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    // ViewModel �̎Q�Ɨp�C���X�^���X�v���p�e�B
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
    public MusicLibrary? Library {
        get { return _library; }
        set {
            _library = value;
            OnPropertyChanged(nameof(Library));
            OnPropertyChanged(nameof(IndexedFileCountText));
        }
    }


    // UI �̕\���p readonly �v���p�e�B
    public string ExecuteButtonText {
        get {
            return Status == TaskStatus.Running ? "�L�����Z��" : "���s";
        }
    }
    public string DoneNoticeText {
        get {
            return Status switch {
                TaskStatus.Completed => "����",
                TaskStatus.Canceled => "�L�����Z�����܂���",
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
    public string IndexedFileCountText {
        get {
            if (Library == null) {
                return "���C���f�b�N�X";
            }
            return $"{Library.Tracks.Count} �����C���f�b�N�X�ς�";
        }
    }
}

internal class IndexService {
    private static CancellationTokenSource? _cancelTokenSource = null;

    public static async void ExecuteButtonClick() {
        if (_cancelTokenSource != null) {
            _cancelTokenSource.Cancel();
            return;
        }

        _cancelTokenSource = new();
        GetViewModel().Status = TaskStatus.Running;

        var newLibrary = await Task.Run(() => MusicIndexer.UpdateIndex(
            (progress) => {
                page?.DispatcherQueue.TryEnqueue(() => {
                    GetViewModel().Progress = progress;
                });
            },
            _cancelTokenSource.Token
        ));

        _cancelTokenSource.Dispose();
        _cancelTokenSource = null;

        if (newLibrary != null) {
            GetViewModel().Status = TaskStatus.Completed;
            GetViewModel().Library = newLibrary;
        } else {
            GetViewModel().Status = TaskStatus.Canceled;
        }

        GetViewModel().Progress = 0;

        await Task.Delay(5 * 1000);
        GetViewModel().Status = TaskStatus.Default;
    }

    public static LibraryPage? page;
    private static IndexViewModel GetViewModel() {
        return page?.IndexViewModel ?? new IndexViewModel();
    }
}