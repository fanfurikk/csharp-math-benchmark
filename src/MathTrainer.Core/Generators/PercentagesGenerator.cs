using System.Globalization;
using MathTrainer.Core.Models;

namespace MathTrainer.Core.Generators;

/// <summary>
/// Генератор заданий на проценты: «найти P% от числа N».
/// Числа подбираются так, чтобы ответ получался «круглым».
/// </summary>
public sealed class PercentagesGenerator : ProblemGeneratorBase
{
    public PercentagesGenerator(Difficulty difficulty, Random? random = null)
        : base(difficulty, random) { }

    public override ProblemType Type => ProblemType.Percentages;

    public override string DisplayName => "Проценты";

    public override Problem Next()
    {
        // Набор «удобных» процентов расширяется с ростом сложности.
        int[] percents = Difficulty switch
        {
            Difficulty.Easy => new[] { 10, 25, 50, 100 },
            Difficulty.Medium => new[] { 5, 10, 20, 25, 50, 75 },
            _ => new[] { 5, 12, 15, 20, 30, 40, 60, 80 }
        };

        int percent = percents[Random.Next(percents.Length)];

        // База кратна 10, чтобы пример считался в уме.
        int baseValue = Next(1, MaxOperand) * 10;

        double answer = baseValue * percent / 100.0;
        var text = $"{percent}% от {baseValue} = ?";

        // Округляем до 2 знаков и убираем лишние нули в конце.
        var answerText = Math.Round(answer, 2).ToString(CultureInfo.InvariantCulture);
        return new Problem(text, answerText, Type);
    }
}
