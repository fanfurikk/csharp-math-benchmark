using System.Globalization;
using MathTrainer.Core;
using MathTrainer.Core.Generators;
using MathTrainer.Core.Sessions;
using MathTrainer.Core.Settings;

namespace MathTrainer.ConsoleApp;

/// <summary>
/// Точка входа консольного приложения. Разбирает параметры командной строки,
/// настраивает тренировочную сессию и проводит её в интерактивном режиме.
/// </summary>
internal static class Program
{
    private static int Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        // --help / -h / -? / /? — показать справку (аналог F1).
        if (HasFlag(args, "--help", "-h", "-?", "/?"))
        {
            PrintHelp();
            return 0;
        }

        // За основу берём сохранённые настройки, затем перекрываем их аргументами.
        var settings = SettingsStorage.Load();
        try
        {
            ApplyArguments(settings, args);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка в параметрах: {ex.Message}");
            Console.WriteLine("Запустите с --help, чтобы увидеть список параметров.");
            return 1;
        }

        // Сохраняем выбранные параметры, чтобы они подхватились при следующем запуске.
        SettingsStorage.Save(settings);

        // Если просили только сохранить настройки — выходим.
        if (HasFlag(args, "--save-only"))
        {
            Console.WriteLine($"Настройки сохранены в: {SettingsStorage.FilePath}");
            return 0;
        }

        RunSession(settings);
        return 0;
    }

    /// <summary>Создаёт сессию по настройкам и проводит её в консоли.</summary>
    private static void RunSession(AppSettings settings)
    {
        var generator = GeneratorFactory.Create(settings.ProblemType, settings.Difficulty);
        var timeLimit = TimeSpan.FromSeconds(settings.TimeLimitSeconds);
        var session = new TrainingSession(generator, settings.QuestionCount, settings.Mode, timeLimit);

        // Подписываемся на события сессии — вся реакция UI происходит здесь.
        session.ProblemPresented += (_, e) =>
            Console.Write($"\n[{e.QuestionNumber}/{e.TotalQuestions}] {e.Problem.Text} ");

        session.AnswerChecked += (_, e) =>
        {
            if (e.Record.IsCorrect)
                WriteColored("  верно!", ConsoleColor.Green);
            else
                WriteColored($"  неверно. Правильный ответ: {e.Record.Problem.CorrectAnswer}", ConsoleColor.Red);
        };

        session.SessionFinished += (_, e) => PrintSummary(e.Statistics);

        PrintHeader(generator, settings);

        var deadline = DateTime.UtcNow + timeLimit;
        session.Start();

        // Основной цикл: пока сессия активна, читаем ответы из консоли.
        while (!session.IsFinished && session.Current is not null)
        {
            // Контроль времени в режиме «на время».
            if (settings.Mode == SessionMode.Timed && DateTime.UtcNow > deadline)
            {
                WriteColored("\nВремя вышло!", ConsoleColor.Yellow);
                session.Finish();
                break;
            }

            var input = Console.ReadLine();

            // Ввод «q» или Ctrl+Z завершает тренировку досрочно.
            if (input is null || input.Trim().Equals("q", StringComparison.OrdinalIgnoreCase))
            {
                session.Finish();
                break;
            }

            session.SubmitAnswer(input);
        }
    }

    /// <summary>Применяет аргументы командной строки к настройкам.</summary>
    private static void ApplyArguments(AppSettings settings, string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i].ToLowerInvariant();
            switch (arg)
            {
                case "--type":
                case "-t":
                    settings.ProblemType = ParseEnum<ProblemType>(RequireValue(args, ref i, arg));
                    break;

                case "--level":
                case "-l":
                    settings.Difficulty = ParseEnum<Difficulty>(RequireValue(args, ref i, arg));
                    break;

                case "--count":
                case "-c":
                    settings.QuestionCount = ParsePositiveInt(RequireValue(args, ref i, arg), arg);
                    break;

                case "--mode":
                case "-m":
                    settings.Mode = ParseEnum<SessionMode>(RequireValue(args, ref i, arg));
                    break;

                case "--time":
                    settings.TimeLimitSeconds = ParsePositiveInt(RequireValue(args, ref i, arg), arg);
                    break;

                case "--save-only":
                    // Обрабатывается в Main, здесь просто игнорируем.
                    break;

                default:
                    throw new ArgumentException($"неизвестный параметр '{args[i]}'.");
            }
        }
    }

    // ----- Вспомогательные методы -----

    private static void PrintHeader(IProblemGenerator generator, AppSettings settings)
    {
        Console.WriteLine("==============================================");
        Console.WriteLine("        ТРЕНАЖЁР УСТНОГО СЧЁТА");
        Console.WriteLine("==============================================");
        Console.WriteLine($"Тип заданий : {generator.DisplayName}");
        Console.WriteLine($"Сложность   : {settings.Difficulty}");
        Console.WriteLine($"Заданий     : {settings.QuestionCount}");
        Console.WriteLine($"Режим       : {(settings.Mode == SessionMode.Timed ? $"на время ({settings.TimeLimitSeconds} c)" : "тренировка")}");
        Console.WriteLine("Введите ответ и нажмите Enter. Для выхода — 'q'.");
    }

    private static void PrintSummary(SessionStatistics stats)
    {
        Console.WriteLine();
        Console.WriteLine("----------------- ИТОГИ ----------------------");
        Console.WriteLine($"Отвечено заданий : {stats.Total}");
        Console.WriteLine($"Верно            : {stats.Correct}");
        Console.WriteLine($"Неверно          : {stats.Wrong}");
        Console.WriteLine($"Точность         : {stats.AccuracyPercent.ToString("F1", CultureInfo.InvariantCulture)}%");
        Console.WriteLine($"Среднее время    : {stats.AverageTime.TotalSeconds.ToString("F1", CultureInfo.InvariantCulture)} c");
        Console.WriteLine("==============================================");
    }

    private static void PrintHelp()
    {
        Console.WriteLine("""
        Тренажёр устного счёта — справка по командной строке
        =====================================================
        Запуск:  MathTrainer.Console [параметры]

        Параметры:
          -t, --type   <Arithmetic|Fractions|Percentages|Equations>
                          тип заданий (по умолчанию Arithmetic)
          -l, --level  <Easy|Medium|Hard>
                          уровень сложности (по умолчанию Easy)
          -c, --count  <число>
                          количество заданий в сессии (по умолчанию 10)
          -m, --mode   <Practice|Timed>
                          режим: обычный или на время (по умолчанию Practice)
              --time   <секунды>
                          лимит времени для режима Timed (по умолчанию 60)
              --save-only
                          только сохранить параметры в файл настроек и выйти
          -h, --help   показать эту справку

        Примеры:
          MathTrainer.Console --type Fractions --level Medium --count 15
          MathTrainer.Console -t Percentages -l Hard -m Timed --time 90
          MathTrainer.Console --type Equations --save-only

        Во время сессии: введите ответ и нажмите Enter; 'q' — досрочный выход.
        """);
    }

    private static void WriteColored(string text, ConsoleColor color)
    {
        var previous = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ForegroundColor = previous;
    }

    private static bool HasFlag(string[] args, params string[] flags) =>
        args.Any(a => flags.Contains(a, StringComparer.OrdinalIgnoreCase));

    private static string RequireValue(string[] args, ref int index, string flag)
    {
        if (index + 1 >= args.Length)
            throw new ArgumentException($"для параметра '{flag}' не указано значение.");
        return args[++index];
    }

    private static TEnum ParseEnum<TEnum>(string value) where TEnum : struct, Enum
    {
        if (Enum.TryParse<TEnum>(value, ignoreCase: true, out var result))
            return result;
        var allowed = string.Join(", ", Enum.GetNames<TEnum>());
        throw new ArgumentException($"недопустимое значение '{value}'. Допустимо: {allowed}.");
    }

    private static int ParsePositiveInt(string value, string flag)
    {
        if (int.TryParse(value, out var result) && result > 0)
            return result;
        throw new ArgumentException($"для параметра '{flag}' нужно положительное целое число.");
    }
}
