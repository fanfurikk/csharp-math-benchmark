namespace MathTrainer.Core.Settings;

/// <summary>
/// Пользовательские настройки приложения: что тренируем, как сложно,
/// сколько заданий, в каком режиме и с какой темой оформления.
/// Сохраняются между запусками (см. <see cref="SettingsStorage"/>).
/// </summary>
public sealed class AppSettings
{
    /// <summary>Тип заданий по умолчанию.</summary>
    public ProblemType ProblemType { get; set; } = ProblemType.Arithmetic;

    /// <summary>Уровень сложности.</summary>
    public Difficulty Difficulty { get; set; } = Difficulty.Easy;

    /// <summary>Режим: обычная тренировка или на время.</summary>
    public SessionMode Mode { get; set; } = SessionMode.Practice;

    /// <summary>Количество заданий в сессии.</summary>
    public int QuestionCount { get; set; } = 10;

    /// <summary>Лимит времени в секундах (для режима на время).</summary>
    public int TimeLimitSeconds { get; set; } = 60;

    /// <summary>Создаёт копию настроек (чтобы не менять оригинал «на лету»).</summary>
    public AppSettings Clone() => (AppSettings)MemberwiseClone();
}
