# VT2 — Agent Context

## About the Project

**Айфэллоу Трекер** (solution/repo: **VT2**) is a Windows desktop task management application: priority board, task edit (factors, subtasks, files), settings. UI display name is Russian; local artifacts use English `iFellowTracker`.

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
| Installer | WPF (`Installer`), self-contained single-file `Installer.exe`, скачивает `self-contained.zip` из Yandex Object Storage |

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
├── Installer/                # WPF installer / updater
├── Database/                 # EF Core data layer (fully implemented)
│   ├── Models/               # TaskDb, SubtaskDb, GoalDb + factor/priority enums
│   ├── Repositories/         # Task / Subtask / Goal repositories
│   ├── Services/             # PriorityCalculator
│   ├── Seed/                 # TaskSeedData
│   ├── VtDbContext.cs
│   ├── DatabaseInitializer.cs
│   └── AppDataPathProvider.cs
└── Vt.Tests/                 # ViewModels, repositories, services, DB initializer
```

## Installer and distribution

- Publish: `.\publish.ps1` → `publish/{major}_{minor}/framework-dependent.zip` and `self-contained.zip` (folder name from `VtApp/version.json`), plus `publish/version.json`.
- Upload `version.json` to the bucket root and the version folder to Yandex Object Storage under `vt2/{major}_{minor}/` (bucket `vt2`). Keep exactly one version folder.
- Installer reads `https://storage.yandexcloud.net/vt2/version.json`, downloads `self-contained.zip` from `vt2/{major}_{minor}/`, installs to `%LocalAppData%\iFellowTracker\App` (`iFellowTracker.exe`), creates Desktop shortcut `iFellowTracker.lnk` (description: «Айфэллоу Трекер»).
- Local data (`iFellowTracker.db`, `TasksFiles`) stays under `%LocalAppData%\iFellowTracker\` outside `App`. Old `%LocalAppData%\VT2` is not migrated.

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

- SQLite file: `%LocalAppData%\iFellowTracker\iFellowTracker.db`.
- Task attachments: `%LocalAppData%\iFellowTracker\TasksFiles\Task_{id}\` (filesystem only; no DB metadata). Add moves into the folder; delete removes the file.
- Soft-delete via `DeletedAtUtc` (filtered in repository queries).
- **No EF migrations** — schema via `EnsureCreated()`.
- On first launch: `EnsureCreated()` + seed (`TaskSeedData`). Subsequent launches keep the existing database.

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
| Files | Filesystem only (`TasksFiles/Task_{id}`) | Tab on edit page when task is saved; no DB rows |
| Goals | Yes (`GoalDb`) | До 3 слотов на задаче; в БД только непустой текст; порядок в UI по `Id` |
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
dotnet run --project Installer/Installer.csproj
dotnet publish Installer/Installer.csproj -c Release -o publish/Installer
.\publish.ps1
```

Requires .NET 10 SDK and Windows (WPF). Close a running `iFellowTracker` before rebuild if copy-to-output fails (`MSB3027`).
