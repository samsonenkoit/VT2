# VT2 — Agent Context

## About the Project

**VT2** is a Windows desktop task management application: priority board, task edit (factors, subtasks, files), settings placeholder.

## Technology Stack

| Area | Stack |
|------|-------|
| Platform | .NET 10 (`net10.0-windows` for app/tests, `net10.0` for Database), WPF |
| UI | Material Design In XAML 5.3 (`MaterialDesignThemes`, `MaterialDesignColors`), MD3 defaults |
| Theme | Light, `PrimaryColor=Grey`, `SecondaryColor=Teal` |
| MVVM | CommunityToolkit.Mvvm 8.4 (`ObservableObject`, `[ObservableProperty]`, `[RelayCommand]`) |
| DI | `Microsoft.Extensions.DependencyInjection` (`VtApp/DependencyInjection/ServiceCollectionExtensions.cs`) |
| Data | EF Core 10 + SQLite (`Microsoft.EntityFrameworkCore.Sqlite`) |
| Tests | xUnit (`Vt.Tests`) |
| Solution | `VT2.slnx` |

## Solution Structure

```
VT2/
├── VtApp/                    # WPF application
│   ├── Assets/               # App icon (vt2.ico)
│   ├── Controls/             # LevelBar (factor level control)
│   ├── Converters/           # IValueConverter bindings
│   ├── DependencyInjection/  # AddVtAppServices()
│   ├── Models/               # UI models (TaskItem, SubtaskEditItem, GoalEditItem, …)
│   ├── Services/             # TaskMapper, TaskFileService, TaskFactorDisplay
│   ├── ViewModels/
│   └── Views/                # TasksView, TaskEditView, SettingsView
├── Database/                 # EF Core data layer (fully implemented)
│   ├── Models/               # TaskDb, SubtaskDb, TaskFileDb + factor/priority enums
│   ├── Repositories/         # Task / Subtask / TaskFile repositories
│   ├── Services/             # PriorityCalculator
│   ├── Seed/                 # TaskSeedData
│   ├── VtDbContext.cs
│   ├── DatabaseInitializer.cs
│   └── AppDataPathProvider.cs
└── Vt.Tests/                 # ViewModels, repositories, services, DB initializer
```

## Architecture

**MVVM**

- Views — markup and minimal code-behind (`InitializeComponent`, DI `DataContext` where needed).
- ViewModels — state/commands; `partial class` + CommunityToolkit generators.
- UI models in `VtApp/Models`; persistence entities and enums in `Database/Models`.

**DI (startup)**

- `App.OnStartup` → `DatabaseInitializer.Initialize()` → build `ServiceCollection` → one root scope → resolve `MainWindow`.
- Scoped: `VtDbContext`, repositories, `ITaskFileService`.
- Transient: ViewModels, `MainWindow`.

**Data**

- SQLite file: `%LocalAppData%\VT2\vt2.db`.
- Task attachments: `%LocalAppData%\VT2\TasksFiles\Task_{id}\`.
- Soft-delete via `DeletedAtUtc` (filtered in repository queries).
- **No EF migrations** — schema via `EnsureCreated()`.
- **Dev reset:** each app launch runs `EnsureDeleted()` + `EnsureCreated()` + seed (`TaskSeedData`). Data does not persist across runs.

**Priority**

- `Database.Services.PriorityCalculator` derives `TaskPriority` from Importance / Urgency (with DelayRisk / Difficulty bumps).
- Board columns and card colors follow `TaskPriority` (`TaskPriorityToBrushConverter`).

## Navigation

**Shell**

- `MainWindowViewModel`: `SelectedPage` (`Tasks` / `Settings`) → `CurrentView`.
- DataTemplates in `App.xaml`: `TasksViewModel` → `TasksView`, `SettingsViewModel` → `SettingsView`.
- Leaving Tasks calls `TasksViewModel.ResetToBoard()`.

**Within Tasks**

- `TasksView` hosts `<ContentControl Content="{Binding CurrentContent}" />`.
- Board: `CurrentContent = this` (`TasksViewModel`) — four priority columns.
- Edit/create: `CurrentContent = TaskEditViewModel` — DataTemplate inside `TasksView.xaml` → `TaskEditView`.
- Save/Cancel returns to the board (optionally reloads tasks).

## Domain Notes

| Concept | Persistence | Notes |
|---------|-------------|--------|
| Task | Yes (`TaskDb`) | Title, description, due date, progress, priority, four factors |
| Factors | Yes | Importance, Urgency, Difficulty, DelayRisk — edited with `LevelBar` |
| Subtasks | Yes (`SubtaskDb`) | Description, due date, progress; checklist UI on edit page |
| Files | Yes (`TaskFileDb` + filesystem) | Tab on edit page when task is saved |
| Goals | **No** | UI-only (`GoalEditItem`); three empty slots, not saved |
| Settings | — | Placeholder page |

## Development Conventions

- Match existing style: Material Design, Grey / Teal, **Russian** UI labels.
- New ViewModels — `partial class` with CommunityToolkit source generators.
- Commands — `[RelayCommand]`; properties — `[ObservableProperty]`; use `[NotifyPropertyChangedFor]` for computed props.
- XAML: `materialDesign:HintAssist`, outlined styles (`MaterialDesignOutlinedTextBox`, etc.).
- View code-behind — only when WPF requires it.
- Prefer repository interfaces over using `VtDbContext` from ViewModels.
- Avoid unnecessary abstractions; keep diffs minimal and focused.
- Add tests for non-trivial ViewModel / repository / service behavior.
- Do not commit secrets or local config (see `.gitignore`).
- Note: `Popup.IsOpen` binds **TwoWay by default** (`BindsTwoWayByDefault`); use explicit `Mode=OneWay` when binding to a `ToggleButton.IsChecked` if dismiss must not uncheck the toggle.

## Build and Run

```bash
dotnet build VT2.slnx
dotnet test VT2.slnx
dotnet run --project VtApp/VtApp.csproj
```

Requires .NET 10 SDK and Windows (WPF). Close a running `VtApp` before rebuild if copy-to-output fails (`MSB3027`).
