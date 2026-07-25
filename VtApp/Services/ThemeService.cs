using MaterialDesignThemes.Wpf;
using VtApp.Models;

namespace VtApp.Services;

public sealed class ThemeService : IThemeService
{
    public void Apply(AppTheme theme)
    {
        var helper = new PaletteHelper();
        var materialTheme = helper.GetTheme();
        materialTheme.SetBaseTheme(theme == AppTheme.Dark ? BaseTheme.Dark : BaseTheme.Light);
        helper.SetTheme(materialTheme);
    }
}
