using CommunityToolkit.Mvvm.ComponentModel;

namespace VtApp.Models;

public partial class SubtaskEditItem : ObservableObject
{
    public int Id { get; init; }

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private DateTime _dueDate = DateTime.Today.AddDays(3);

    [ObservableProperty]
    private int _progressPercent;
}
