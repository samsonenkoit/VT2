# VT2 — Agent Context

## About the Project

**VT2** is a Windows desktop task management application.  


## Technology Stack

| Area | Stack |
|------|-------|
| Platform | .NET 10, Windows (WPF) |
| UI | Material Design In XAML (`MaterialDesignThemes`, `MaterialDesignColors`) |
| MVVM | CommunityToolkit.Mvvm (`ObservableObject`, `[ObservableProperty]`, `[RelayCommand]`) |
| Tests | xUnit (`Vt.Tests`) |
| Solution | `VT2.slnx` |

## Solution Structure

```
VT2/
├── VtApp/              # WPF application (main UI)
│   ├── Models/         # Domain models (TaskItem, TaskPriority, …)
│   ├── ViewModels/     # Presentation logic
│   ├── Views/          # XAML views (.xaml + code-behind)
│   ├── Converters/     # IValueConverter bindings
│   └── Services/       # Non-UI business logic (priority calculation, etc.)
├── Database/           # Data layer (stub only, not implemented yet)
└── Vt.Tests/           # ViewModel unit tests
```

## Architecture

The app follows the **MVVM** pattern:

- **Views** — markup and minimal code-behind only (`InitializeComponent`).
- **ViewModels** — state, commands, bindings; inherit from `ObservableObject`.
- **Models** — simple POCO classes and enums.

Navigation:
- `MainWindowViewModel` switches pages via `SelectedPage` / `CurrentView`.
- DataTemplates in `App.xaml` map ViewModel → View.

Task priority (`TaskPriority`) determines the board column and card color (see `TaskPriorityToBrushConverter`).

## Development Conventions

- Match the existing style: Material Design, `DeepPurple` / `Teal`, Russian labels in the UI.
- New ViewModels — `partial class` with CommunityToolkit source generators.
- Commands — `[RelayCommand]`; properties — `[ObservableProperty]`.
- XAML: `materialDesign:HintAssist`, outlined styles (`MaterialDesignOutlinedTextBox`, etc.).
- View code-behind — only when WPF requires it.
- Avoid unnecessary abstractions; keep diffs minimal and focused.
- Add tests for ViewModel logic when behavior is non-trivial.
- Do not commit secrets or local config (see `.gitignore`).

## Build and Run

```bash
dotnet build VT2.slnx
dotnet test VT2.slnx
dotnet run --project VtApp/VtApp.csproj
```

Requires .NET 10 SDK and Windows (WPF).

