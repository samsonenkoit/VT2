using Database.Models;
using Database.Helpers;

namespace Database.Seed;

public static class TaskSeedData
{
    public static (IReadOnlyList<TaskDb> Tasks, IReadOnlyList<SubtaskDb> Subtasks, IReadOnlyList<GoalDb> Goals) GetSeedData()
    {
        var tasks = new List<TaskDb>
        {
            CreateTask(
                1,
                "Согласовать срочный договор с поставщиком",
                "Нужно согласовать условия поставки и подписать договор до начала следующего квартала. Юридический и финансовый отделы уже уведомили о приоритете.",
                new DateTime(2026, 3, 18),
                15,
                TaskImportance.High,
                TaskDelayRisk.Low,
                TaskDifficulty.Low,
                TaskUrgency.High),
            CreateTask(
                2,
                "Устранить критическую ошибку в отчёте",
                "В сводном отчёте неверно считается итоговая строка при фильтрации по подразделению. Ошибка воспроизводится на тестовых и боевых данных.",
                new DateTime(2026, 3, 20),
                40,
                TaskImportance.Critical,
                TaskDelayRisk.Medium,
                TaskDifficulty.Medium,
                TaskUrgency.Medium),
            CreateTask(
                3,
                "Подготовить ответ регулятору",
                "Собрать пояснения по запросу надзорного органа: приложить выписки, описания процессов и сроки устранения замечаний.",
                new DateTime(2026, 3, 22),
                5,
                TaskImportance.Critical,
                TaskDelayRisk.Low,
                TaskDifficulty.Low,
                TaskUrgency.High),
            CreateTask(
                4,
                "Создать первую задачу в новом проекте",
                "Зафиксировать стартовую задачу проекта: цель, критерии готовности и ответственных. Это точка входа для команды.",
                new DateTime(2026, 3, 25),
                30,
                TaskImportance.Medium,
                TaskDelayRisk.Low,
                TaskDifficulty.Low,
                TaskUrgency.High),
            CreateTask(
                5,
                "Согласовать бюджет на второй квартал",
                "Свести заявки подразделений, проверить лимиты и согласовать итоговую версию бюджета на Q2 с руководством.",
                new DateTime(2026, 3, 28),
                55,
                TaskImportance.High,
                TaskDelayRisk.Low,
                TaskDifficulty.Low,
                TaskUrgency.Medium),
            CreateTask(
                6,
                "Обновить презентацию для совещания",
                "Актуализировать слайды по статусу проектов, метрикам и рискам к еженедельному совещанию руководства.",
                new DateTime(2026, 4, 1),
                70,
                TaskImportance.High,
                TaskDelayRisk.Medium,
                TaskDifficulty.Low,
                TaskUrgency.Medium),
            CreateTask(
                7,
                "Проверить входящие заявки от клиентов",
                "Разобрать очередь входящих обращений: приоритезировать критичные, назначить исполнителей и закрыть дубликаты.",
                new DateTime(2026, 4, 3),
                20,
                TaskImportance.Critical,
                TaskDelayRisk.Low,
                TaskDifficulty.Low,
                TaskUrgency.Low),
            CreateTask(
                8,
                "Разобрать архив переписки за месяц",
                "Пройти почтовый архив за прошедший месяц, выделить открытые вопросы и перенести важные договорённости в задачи.",
                new DateTime(2026, 4, 10),
                45,
                TaskImportance.Medium,
                TaskDelayRisk.Low,
                TaskDifficulty.Low,
                TaskUrgency.Medium),
            CreateTask(
                9,
                "Актуализировать список контактов",
                "Обновить справочник контактов партнёров и подрядчиков: телефоны, роли и актуальные адреса электронной почты.",
                new DateTime(2026, 4, 12),
                60,
                TaskImportance.High,
                TaskDelayRisk.Low,
                TaskDifficulty.Low,
                TaskUrgency.Low),
            CreateTask(
                10,
                "Подготовить шаблоны ответов для поддержки",
                "Собрать типовые ответы на частые вопросы клиентов и оформить их в удобные шаблоны для первой линии поддержки.",
                new DateTime(2026, 4, 15),
                35,
                TaskImportance.Low,
                TaskDelayRisk.Low,
                TaskDifficulty.Low,
                TaskUrgency.High),
            CreateTask(
                11,
                "Изучить новые возможности Material Design",
                "Ознакомиться с обновлениями Material Design In XAML и оценить, что можно применить в интерфейсе приложения.",
                new DateTime(2026, 5, 5),
                10,
                TaskImportance.Low,
                TaskDelayRisk.Low,
                TaskDifficulty.Low,
                TaskUrgency.Low),
            CreateTask(
                12,
                "Навести порядок в папках проекта",
                "Упорядочить структуру каталогов репозитория и артефактов: убрать устаревшие файлы и согласовать naming conventions.",
                new DateTime(2026, 5, 12),
                0,
                TaskImportance.Low,
                TaskDelayRisk.Low,
                TaskDifficulty.Medium,
                TaskUrgency.Medium),
            CreateTask(
                13,
                "Составить план обучения команды",
                "Подготовить план обучения на квартал: темы, форматы, расписание и ожидаемые результаты для участников.",
                new DateTime(2026, 5, 20),
                25,
                TaskImportance.Medium,
                TaskDelayRisk.Low,
                TaskDifficulty.Low,
                TaskUrgency.Low),
            CreateTask(
                14,
                "Обновить документацию по процессам",
                "Актуализировать описания рабочих процессов: согласования, релизы и эскалации. Убрать устаревшие шаги.",
                new DateTime(2026, 6, 1),
                80,
                TaskImportance.Low,
                TaskDelayRisk.Medium,
                TaskDifficulty.Low,
                TaskUrgency.Low),
            CreateTask(
                15,
                "Провести ретроспективу квартала",
                "Организовать ретроспективу: собрать обратную связь, выделить улучшения и зафиксировать план действий на следующий квартал.",
                new DateTime(2026, 6, 15),
                50,
                TaskImportance.Medium,
                TaskDelayRisk.Medium,
                TaskDifficulty.Low,
                TaskUrgency.Low),
        };

        var subtasks = new List<SubtaskDb>
        {
            // Task 1 — 3
            new() { Description = "Запросить правки у юридического отдела", TaskId = 1, ProgressPercent = 100 },
            new() { Description = "Согласовать сумму с финансовым директором", TaskId = 1, ProgressPercent = 50 },
            new() { Description = "Отправить финальную версию поставщику", TaskId = 1 },

            // Task 2 — 4
            new() { Description = "Воспроизвести ошибку на тестовых данных", TaskId = 2, ProgressPercent = 100 },
            new() { Description = "Исправить расчёт итоговой строки", TaskId = 2, ProgressPercent = 60 },
            new() { Description = "Добавить регрессионный тест", TaskId = 2 },
            new() { Description = "Выкатить исправление на прод", TaskId = 2 },

            // Task 3 — 6
            new() { Description = "Собрать материалы по замечаниям", TaskId = 3 },
            new() { Description = "Подготовить черновик ответа", TaskId = 3 },
            new() { Description = "Согласовать формулировки с compliance", TaskId = 3 },
            new() { Description = "Приложить подтверждающие документы", TaskId = 3 },
            new() { Description = "Проверить даты и реквизиты", TaskId = 3 },
            new() { Description = "Отправить пакет регулятору", TaskId = 3 },

            // Task 4 — 2
            new() { Description = "Описать цель и критерии готовности", TaskId = 4, ProgressPercent = 80 },
            new() { Description = "Назначить ответственного и срок", TaskId = 4 },

            // Task 5 — 4
            new() { Description = "Собрать заявки от подразделений", TaskId = 5, ProgressPercent = 100 },
            new() { Description = "Сверить лимиты с прошлым кварталом", TaskId = 5, ProgressPercent = 70 },
            new() { Description = "Подготовить сводную таблицу", TaskId = 5, ProgressPercent = 40 },
            new() { Description = "Получить утверждение руководства", TaskId = 5 },

            // Task 6 — 3
            new() { Description = "Обновить блок со статусами проектов", TaskId = 6, ProgressPercent = 100 },
            new() { Description = "Добавить актуальные метрики", TaskId = 6, ProgressPercent = 80 },
            new() { Description = "Проверить слайды с докладчиком", TaskId = 6 },

            // Task 7 — 5
            new() { Description = "Отфильтровать критичные заявки", TaskId = 7, ProgressPercent = 50 },
            new() { Description = "Закрыть дубликаты", TaskId = 7 },
            new() { Description = "Назначить исполнителей", TaskId = 7 },
            new() { Description = "Обновить статусы в трекере", TaskId = 7 },
            new() { Description = "Ответить клиентам по приоритетным", TaskId = 7 },

            // Task 8 — 2
            new() { Description = "Выгрузить архив писем за месяц", TaskId = 8, ProgressPercent = 100 },
            new() { Description = "Перенести открытые вопросы в задачи", TaskId = 8, ProgressPercent = 20 },

            // Task 9 — 3
            new() { Description = "Проверить телефоны и e-mail", TaskId = 9, ProgressPercent = 100 },
            new() { Description = "Уточнить роли у партнёров", TaskId = 9, ProgressPercent = 50 },
            new() { Description = "Опубликовать обновлённый справочник", TaskId = 9 },

            // Task 10 — 6
            new() { Description = "Собрать топ частых вопросов", TaskId = 10, ProgressPercent = 70 },
            new() { Description = "Написать черновики ответов", TaskId = 10, ProgressPercent = 30 },
            new() { Description = "Согласовать тон с маркетингом", TaskId = 10 },
            new() { Description = "Проверить факты с продуктовой командой", TaskId = 10 },
            new() { Description = "Оформить шаблоны в едином стиле", TaskId = 10 },
            new() { Description = "Загрузить шаблоны в базу знаний", TaskId = 10 },

            // Task 11 — 1
            new() { Description = "Просмотреть changelog и примеры компонентов", TaskId = 11, ProgressPercent = 40 },

            // Task 12 — 3
            new() { Description = "Составить карту текущих папок", TaskId = 12 },
            new() { Description = "Удалить или архивировать устаревшее", TaskId = 12 },
            new() { Description = "Зафиксировать правила именования", TaskId = 12 },

            // Task 13 — 5
            new() { Description = "Собрать потребности команды", TaskId = 13, ProgressPercent = 60 },
            new() { Description = "Выбрать темы и форматы", TaskId = 13, ProgressPercent = 20 },
            new() { Description = "Согласовать бюджет на обучение", TaskId = 13 },
            new() { Description = "Составить календарь занятий", TaskId = 13 },
            new() { Description = "Разослать план участникам", TaskId = 13 },

            // Task 14 — 2
            new() { Description = "Сверить документ с фактическим процессом", TaskId = 14, ProgressPercent = 100 },
            new() { Description = "Обновить схемы и чек-листы", TaskId = 14, ProgressPercent = 60 },

            // Task 15 — 4
            new() { Description = "Собрать обратную связь участников", TaskId = 15, ProgressPercent = 80 },
            new() { Description = "Подготовить фасилитацию и тайминг", TaskId = 15, ProgressPercent = 40 },
            new() { Description = "Провести встречу ретроспективы", TaskId = 15 },
            new() { Description = "Оформить action items", TaskId = 15 },
        };

        var goals = new List<GoalDb>
        {
            // Task 1 — 2
            new() { TaskId = 1, Text = "Подписать договор до конца недели" },
            new() { TaskId = 1, Text = "Сохранить условия поставки" },

            // Task 2 — 2
            new() { TaskId = 2, Text = "Вернуть корректный отчёт пользователям" },
            new() { TaskId = 2, Text = "Исключить регрессию в итогах" },

            // Task 3 — 3
            new() { TaskId = 3, Text = "Отправить полный ответ в срок" },
            new() { TaskId = 3, Text = "Закрыть все пункты замечаний" },
            new() { TaskId = 3, Text = "Согласовать текст с compliance" },

            // Task 4 — 1
            new() { TaskId = 4, Text = "Запустить проект с понятной первой задачей" },

            // Task 5 — 3
            new() { TaskId = 5, Text = "Утвердить бюджет Q2" },
            new() { TaskId = 5, Text = "Уложиться в лимиты компании" },
            new() { TaskId = 5, Text = "Согласовать приоритеты подразделений" },

            // Task 6 — 2
            new() { TaskId = 6, Text = "Готовая презентация к совещанию" },
            new() { TaskId = 6, Text = "Актуальные цифры без расхождений" },

            // Task 7 — 1
            new() { TaskId = 7, Text = "Нулевая очередь критичных заявок" },

            // Task 8 — 2
            new() { TaskId = 8, Text = "Не потерять открытые договорённости" },
            new() { TaskId = 8, Text = "Все важные письма превратить в задачи" },

            // Task 9 — 3
            new() { TaskId = 9, Text = "Актуальный справочник контактов" },
            new() { TaskId = 9, Text = "Без устаревших записей" },
            new() { TaskId = 9, Text = "Единый формат данных" },

            // Task 10 — 2
            new() { TaskId = 10, Text = "Сократить время ответа поддержки" },
            new() { TaskId = 10, Text = "Единый тон коммуникации" },

            // Task 11 — 1
            new() { TaskId = 11, Text = "Выбрать 2–3 улучшения для UI" },

            // Task 12 — 2
            new() { TaskId = 12, Text = "Понятная структура проекта" },
            new() { TaskId = 12, Text = "Меньше времени на поиск файлов" },

            // Task 13 — 3
            new() { TaskId = 13, Text = "План обучения на квартал" },
            new() { TaskId = 13, Text = "Закрыть ключевые пробелы навыков" },
            new() { TaskId = 13, Text = "Согласованное расписание" },

            // Task 14 — 1
            new() { TaskId = 14, Text = "Документация соответствует практике" },

            // Task 15 — 3
            new() { TaskId = 15, Text = "Список улучшений на следующий квартал" },
            new() { TaskId = 15, Text = "Вовлечённость всей команды" },
            new() { TaskId = 15, Text = "Измеримые action items" },
        };

        return (tasks, subtasks, goals);
    }

    private static TaskDb CreateTask(
        int id,
        string title,
        string description,
        DateTime dueDateUtc,
        int progressPercent,
        TaskImportance importance,
        TaskDelayRisk delayRisk,
        TaskDifficulty difficulty,
        TaskUrgency urgency) =>
        new()
        {
            Id = id,
            Title = title,
            Description = description,
            DueDateUtc = dueDateUtc,
            ProgressPercent = progressPercent,
            Importance = importance,
            DelayRisk = delayRisk,
            Difficulty = difficulty,
            Urgency = urgency,
            Priority = PriorityCalculator.Calculate(importance, delayRisk, difficulty, urgency),
        };
}
