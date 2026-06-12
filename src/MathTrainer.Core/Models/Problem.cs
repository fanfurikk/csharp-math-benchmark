using System.Globalization;

namespace MathTrainer.Core.Models;

/// <summary>
/// Одно математическое задание: условие (текст) и правильный ответ.
/// Класс отвечает за проверку введённого пользователем ответа.
/// </summary>
public sealed class Problem
{
    /// <summary>Текст условия, например «12 + 7 = ?».</summary>
    public string Text { get; }

    /// <summary>Правильный ответ в каноничной строковой форме (например «19» или «3/4»).</summary>
    public string CorrectAnswer { get; }

    /// <summary>Тип задания, которому принадлежит этот пример.</summary>
    public ProblemType Type { get; }

    /// <summary>
    /// Создаёт новое задание.
    /// </summary>
    /// <param name="text">Текст условия.</param>
    /// <param name="correctAnswer">Правильный ответ.</param>
    /// <param name="type">Тип задания.</param>
    public Problem(string text, string correctAnswer, ProblemType type)
    {
        Text = text ?? throw new ArgumentNullException(nameof(text));
        CorrectAnswer = correctAnswer ?? throw new ArgumentNullException(nameof(correctAnswer));
        Type = type;
    }

    /// <summary>
    /// Проверяет ответ пользователя. Сравнение устойчиво к пробелам,
    /// к разделителю дроби (точка/запятая) и к лишним нулям в дробях.
    /// </summary>
    /// <param name="userAnswer">Ответ, введённый пользователем.</param>
    /// <returns><c>true</c>, если ответ верный.</returns>
    public bool CheckAnswer(string? userAnswer)
    {
        if (string.IsNullOrWhiteSpace(userAnswer))
            return false;

        var normalizedUser = Normalize(userAnswer);
        var normalizedCorrect = Normalize(CorrectAnswer);
        return string.Equals(normalizedUser, normalizedCorrect, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Приводит строку-ответ к единому виду: убирает пробелы,
    /// заменяет запятую на точку, пытается сравнить числа по значению.
    /// </summary>
    private static string Normalize(string value)
    {
        var trimmed = value.Trim().Replace(" ", string.Empty).Replace(',', '.');

        // Если это обычное число — округляем до 4 знаков, чтобы 0.5 == .50.
        if (double.TryParse(trimmed, NumberStyles.Any, CultureInfo.InvariantCulture, out var number))
            return Math.Round(number, 4).ToString(CultureInfo.InvariantCulture);

        return trimmed;
    }

    /// <summary>Удобное строковое представление для логов и отладки.</summary>
    public override string ToString() => $"{Text}  (ответ: {CorrectAnswer})";
}
