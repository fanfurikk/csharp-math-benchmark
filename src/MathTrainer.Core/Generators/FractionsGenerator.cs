using MathTrainer.Core.Models;

namespace MathTrainer.Core.Generators;

/// <summary>
/// Генератор примеров с обыкновенными дробями: сложение и вычитание.
/// Ответ приводится к несократимому виду, например «1/2» или «3».
/// </summary>
public sealed class FractionsGenerator : ProblemGeneratorBase
{
    public FractionsGenerator(Difficulty difficulty, Random? random = null)
        : base(difficulty, random) { }

    public override ProblemType Type => ProblemType.Fractions;

    public override string DisplayName => "Дроби";

    public override Problem Next()
    {
        // Чем выше сложность, тем больше знаменатели.
        var maxDenominator = Difficulty switch
        {
            Difficulty.Easy => 5,
            Difficulty.Medium => 9,
            _ => 12
        };

        int d1 = Next(2, maxDenominator);
        int d2 = Next(2, maxDenominator);
        int n1 = Next(1, d1 - 1);
        int n2 = Next(1, d2 - 1);

        bool isAddition = Random.Next(2) == 0;

        // Приводим к общему знаменателю d1*d2.
        int numerator = isAddition
            ? n1 * d2 + n2 * d1
            : n1 * d2 - n2 * d1;
        int denominator = d1 * d2;

        var op = isAddition ? '+' : '-';
        var text = $"{n1}/{d1} {op} {n2}/{d2} = ?";
        var answer = FormatFraction(numerator, denominator);

        return new Problem(text, answer, Type);
    }

    /// <summary>
    /// Сокращает дробь и форматирует результат:
    /// целое число — без знаменателя, ноль — как «0», иначе «n/d».
    /// </summary>
    private static string FormatFraction(int numerator, int denominator)
    {
        if (numerator == 0)
            return "0";

        int sign = (numerator < 0) ? -1 : 1;
        numerator = Math.Abs(numerator);

        int gcd = GreatestCommonDivisor(numerator, denominator);
        numerator /= gcd;
        denominator /= gcd;

        if (denominator == 1)
            return (sign * numerator).ToString();

        return $"{sign * numerator}/{denominator}";
    }

    /// <summary>Наибольший общий делитель (алгоритм Евклида).</summary>
    private static int GreatestCommonDivisor(int a, int b)
    {
        while (b != 0)
            (a, b) = (b, a % b);
        return a == 0 ? 1 : a;
    }
}
