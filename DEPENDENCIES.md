# Зависимости VT2 (Айфэллоу Трекер)

Список NuGet-пакетов решения `VT2.slnx` и лицензий по метаданным пакетов (NuGet `license` / `licenseUrl`).  
Версии — resolved на момент составления файла. Транзитивные пакеты помечены в колонке «Тип».

## Прямые зависимости

| Пакет | Версия | Лицензия | Проекты |
|-------|--------|----------|---------|
| CommunityToolkit.Mvvm | 8.4.2 | MIT | VtApp, Installer |
| MaterialDesignColors | 5.3.2 | MIT | VtApp |
| MaterialDesignThemes | 5.3.2 | MIT | VtApp |
| Microsoft.EntityFrameworkCore.Sqlite | 10.0.0 | MIT | Database |
| Microsoft.Extensions.DependencyInjection | 10.0.0 | MIT | VtApp |
| Microsoft.NET.Test.Sdk | 17.14.0 | MIT | Vt.Tests |
| xunit | 2.9.3 | Apache-2.0 | Vt.Tests |
| xunit.runner.visualstudio | 3.0.2 | Apache-2.0 | Vt.Tests |

Автоматически подключаемый при publish:

| Пакет | Версия | Лицензия | Проекты |
|-------|--------|----------|---------|
| Microsoft.NET.ILLink.Tasks | 10.0.10 | MIT | Installer (auto-referenced) |

## Транзитивные зависимости

| Пакет | Версия | Лицензия | Приходит через |
|-------|--------|----------|----------------|
| Microsoft.CodeCoverage | 17.14.0 | MIT | Microsoft.NET.Test.Sdk |
| Microsoft.Data.Sqlite.Core | 10.0.0 | MIT | Microsoft.EntityFrameworkCore.Sqlite |
| Microsoft.EntityFrameworkCore | 10.0.0 | MIT | Microsoft.EntityFrameworkCore.Sqlite |
| Microsoft.EntityFrameworkCore.Abstractions | 10.0.0 | MIT | Microsoft.EntityFrameworkCore |
| Microsoft.EntityFrameworkCore.Analyzers | 10.0.0 | MIT | Microsoft.EntityFrameworkCore |
| Microsoft.EntityFrameworkCore.Relational | 10.0.0 | MIT | Microsoft.EntityFrameworkCore.Sqlite |
| Microsoft.EntityFrameworkCore.Sqlite.Core | 10.0.0 | MIT | Microsoft.EntityFrameworkCore.Sqlite |
| Microsoft.Extensions.Caching.Abstractions | 10.0.0 | MIT | Microsoft.EntityFrameworkCore / Extensions.* |
| Microsoft.Extensions.Caching.Memory | 10.0.0 | MIT | Microsoft.EntityFrameworkCore |
| Microsoft.Extensions.Configuration.Abstractions | 10.0.0 | MIT | Microsoft.Extensions.* |
| Microsoft.Extensions.DependencyInjection | 10.0.0 | MIT | Microsoft.EntityFrameworkCore (также прямая в VtApp) |
| Microsoft.Extensions.DependencyInjection.Abstractions | 10.0.0 | MIT | Microsoft.Extensions.DependencyInjection |
| Microsoft.Extensions.DependencyModel | 10.0.0 | MIT | Microsoft.EntityFrameworkCore.Sqlite |
| Microsoft.Extensions.Logging | 10.0.0 | MIT | Microsoft.EntityFrameworkCore |
| Microsoft.Extensions.Logging.Abstractions | 10.0.0 | MIT | Microsoft.Extensions.Logging |
| Microsoft.Extensions.Options | 10.0.0 | MIT | Microsoft.Extensions.* |
| Microsoft.Extensions.Primitives | 10.0.0 | MIT | Microsoft.Extensions.* |
| Microsoft.TestPlatform.ObjectModel | 17.14.0 | MIT | Microsoft.NET.Test.Sdk |
| Microsoft.TestPlatform.TestHost | 17.14.0 | MIT | Microsoft.NET.Test.Sdk |
| Microsoft.Xaml.Behaviors.Wpf | 1.1.77 | MIT | MaterialDesignThemes |
| Newtonsoft.Json | 13.0.3 | MIT | Microsoft.TestPlatform.* |
| SQLitePCLRaw.bundle_e_sqlite3 | 2.1.11 | Apache-2.0 | Microsoft.EntityFrameworkCore.Sqlite |
| SQLitePCLRaw.core | 2.1.11 | Apache-2.0 | SQLitePCLRaw.bundle_e_sqlite3 |
| SQLitePCLRaw.lib.e_sqlite3 | 2.1.11 | Apache-2.0 | SQLitePCLRaw.bundle_e_sqlite3 |
| SQLitePCLRaw.provider.e_sqlite3 | 2.1.11 | Apache-2.0 | SQLitePCLRaw.bundle_e_sqlite3 |
| xunit.abstractions | 2.0.3 | Apache-2.0 | xunit / xunit.runner.visualstudio |
| xunit.analyzers | 1.18.0 | Apache-2.0 | xunit |
| xunit.assert | 2.9.3 | Apache-2.0 | xunit |
| xunit.core | 2.9.3 | Apache-2.0 | xunit |
| xunit.extensibility.core | 2.9.3 | Apache-2.0 | xunit.core |
| xunit.extensibility.execution | 2.9.3 | Apache-2.0 | xunit.core |

## Сводка по лицензиям

| Лицензия | Пакеты (кратко) |
|----------|-----------------|
| MIT | CommunityToolkit.Mvvm, MaterialDesign*, Microsoft.EntityFrameworkCore*, Microsoft.Extensions*, Microsoft.Xaml.Behaviors.Wpf, Microsoft.NET.*, Microsoft.TestPlatform.*, Newtonsoft.Json |
| Apache-2.0 | SQLitePCLRaw.*, xunit* |

## Примечания

- Лицензии взяты из метаданных NuGet-пакетов в локальном кэше (`%USERPROFILE%\.nuget\packages`).
- Пакет `SQLitePCLRaw.lib.e_sqlite3` распространяется под Apache-2.0; встроенная нативная библиотека **SQLite** исторически распространяется как Public Domain ([SQLite copyright](https://www.sqlite.org/copyright.html)).
- Для `xunit.abstractions` 2.0.3 в nuspec указан `licenseUrl` на репозиторий xUnit (Apache-2.0).
- Runtime .NET / Windows SDK в эту таблицу не включены: они являются частью платформы при self-contained publish или установленного Desktop Runtime.
- Чтобы обновить список: `dotnet list package --include-transitive`.
