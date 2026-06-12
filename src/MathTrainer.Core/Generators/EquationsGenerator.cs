using System.Globalization;
using MathTrainer.Core.Models;

namespace MathTrainer.Core.Generators;

/// <summary>
/// Генератор простых линейных уравнений вида a·x + b = c.
/// Корень x — целое число, его и нужно найти.
/// </summary>
public sealed class EquationsGenerator : ProblemGeneratorBase
{
    public EquationsGenerator(Difficulty difficulty, Random? random = null)
        : base(difficulty, random) { }

    public override ProblemType Type => ProblemType.Equations;

    public override string DisplayName => "Уравнения";

    public override Problem Next()
    {
        // Коэффициент при x и сам корень растут вместе со сложностью.
        int maxCoefficient = Difficulty switch
        {
            Difficulty.Easy => 5,
            Difficulty.Medium => 9,
            _ => 12
        };

        int a = Next(2, maxCoefficient);          // коэффициент при x (не 0 и не 1)
        int x = Next(1, MaxOperand / 5 + 1);      // задуманный корень
        int b = Next(1, MaxOperand);              // свободный член
        int c = a * x + b;                        // правая часть, подобранная под корень

        var text = $"{a}x + {b} = {c};  x = ?";
        return new Problem(text, x.ToString(CultureInfo.InvariantCulture), Type);
    }
}
