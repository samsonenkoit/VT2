using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace VtApp.Models;

public partial class SubtaskEditItem : ObservableObject
{
    public int Id { get; init; }

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDescription))]
    private string? _description;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DueDateDisplay))]
    [NotifyPropertyChangedFor(nameof(HasDueDate))]
    private DateTime? _dueDate;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDone))]
    private int _progressPercent;

    public bool HasDescription => !string.IsNullOrWhiteSpace(Description);

    public bool HasDueDate => DueDate is not null;

    public string DueDateDisplay => DueDate?.ToString("dd.MM") ?? "—";

    public bool IsDone
    {
        get => ProgressPercent >= 100;
        set => ProgressPercent = value ? 100 : 0;
    }

    partial void OnProgressPercentChanged(int value)
    {
        var normalized = SubtaskProgressOptions.Normalize(value);
        if (normalized != value)
            ProgressPercent = normalized;
    }

    partial void OnDueDateChanged(DateTime? value)
    {
        if (value is null)
            return;

        var date = value.Value.Date;
        if (date != value.Value)
            DueDate = date;
    }

    [RelayCommand]
    private void SetProgress(int progressPercent) =>
        ProgressPercent = SubtaskProgressOptions.Normalize(progressPercent);

    [RelayCommand]
    private void ClearDueDate() => DueDate = null;
}
