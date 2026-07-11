using CommunityToolkit.Mvvm.ComponentModel;

namespace VtApp.Models;

public partial class SubtaskEditItem : ObservableObject
{
    public int Id { get; init; }

    [ObservableProperty]
    private string _title = string.Empty;
}
