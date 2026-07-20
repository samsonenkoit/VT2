using CommunityToolkit.Mvvm.ComponentModel;

namespace VtApp.Models;

public partial class GoalEditItem : ObservableObject
{
    public int Id { get; set; }

    [ObservableProperty]
    private string _text = string.Empty;
}
