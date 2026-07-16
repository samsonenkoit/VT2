using CommunityToolkit.Mvvm.ComponentModel;

namespace VtApp.Models;

public partial class GoalEditItem : ObservableObject
{
    [ObservableProperty]
    private string _text = string.Empty;
}
