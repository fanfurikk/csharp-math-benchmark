namespace MathTrainer.Core.Generators;

/// <summary>
/// Фабрика генераторов. Скрывает от остального кода детали того,
/// какой конкретный класс создаётся для выбранного <see cref="ProblemType"/>.
/// </summary>
public static class GeneratorFactory
{
    /// <summary>
    /// Создать генератор нужного типа с заданной сложностью.
    /// </summary>
    /// <param name="type">Тип заданий.</param>
    /// <param name="difficulty">Уровень сложности.</param>
    /// <param name="random">Необязательный источник случайности (для тестов).</param>
    public static IProblemGenerator Create(ProblemType type, Difficulty difficulty, Random? random = null)
    {
        return type switch
        {
            ProblemType.Arithmetic => new ArithmeticGenerator(difficulty, random),
            ProblemType.Fractions => new FractionsGenerator(difficulty, random),
            ProblemType.Percentages => new PercentagesGenerator(difficulty, random),
            ProblemType.Equations => new EquationsGenerator(difficulty, random),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Неизвестный тип заданий.")
        };
    }

    /// <summary>Список всех поддерживаемых типов заданий (для меню/CLI).</summary>
    public static IReadOnlyList<ProblemType> AllTypes { get; } = Enum.GetValues<ProblemType>();
}
