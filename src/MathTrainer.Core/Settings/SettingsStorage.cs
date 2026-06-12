using System.Text.Json;
using System.Text.Json.Serialization;

namespace MathTrainer.Core.Settings;

/// <summary>
/// Отвечает за загрузку и сохранение <see cref="AppSettings"/> в JSON-файл
/// в папке профиля пользователя (%AppData%\MathTrainer\settings.json).
/// </summary>
public static class SettingsStorage
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        // Перечисления сохраняем строками — файл читаемый и устойчив к изменениям.
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>Полный путь к файлу настроек.</summary>
    public static string FilePath
    {
        get
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MathTrainer");
            return Path.Combine(dir, "settings.json");
        }
    }

    /// <summary>
    /// Загружает настройки. Если файла нет или он повреждён — возвращает
    /// настройки по умолчанию (приложение не должно падать из-за этого).
    /// </summary>
    public static AppSettings Load()
    {
        try
        {
            if (!File.Exists(FilePath))
                return new AppSettings();

            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
        }
        catch
        {
            // Любая ошибка чтения/разбора — откатываемся к значениям по умолчанию.
            return new AppSettings();
        }
    }

    /// <summary>Сохраняет настройки в файл, создавая папку при необходимости.</summary>
    public static void Save(AppSettings settings)
    {
        var directory = Path.GetDirectoryName(FilePath)!;
        Directory.CreateDirectory(directory);

        var json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(FilePath, json);
    }
}
