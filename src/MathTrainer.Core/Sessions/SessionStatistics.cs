namespace MathTrainer.Core.Sessions;

/// <summary>
/// Итоговая статистика тренировочной сессии. Вычисляется на основе
/// коллекции <see cref="AnswerRecord"/>.
/// </summary>
public sealed class SessionStatistics
{
    /// <summary>Всего отвеченных заданий.</summary>
    public int Total { get; }

    /// <summary>Количество верных ответов.</summary>
    public int Correct { get; }

    /// <summary>Количество неверных ответов.</summary>
    public int Wrong => Total - Correct;

    /// <summary>Точность в процентах (0..100).</summary>
    public double AccuracyPercent => Total == 0 ? 0 : Correct * 100.0 / Total;

    /// <summary>Среднее время на ответ.</summary>
    public TimeSpan AverageTime { get; }

    /// <summary>Общее затраченное время.</summary>
    public TimeSpan TotalTime { get; }

    /// <summary>Полная история ответов (только для чтения).</summary>
    public IReadOnlyList<AnswerRecord> Records { get; }

    public SessionStatistics(IReadOnlyList<AnswerRecord> records)
    {
        Records = records;
        Total = records.Count;
        Correct = records.Count(r => r.IsCorrect);
        TotalTime = records.Aggregate(TimeSpan.Zero, (sum, r) => sum + r.TimeSpent);
        AverageTime = Total == 0 ? TimeSpan.Zero : TimeSpan.FromTicks(TotalTime.Ticks / Total);
    }
}
