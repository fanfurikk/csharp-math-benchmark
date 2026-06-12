using System.Globalization;
using MathTrainer.Core.Models;

namespace MathTrainer.Core.Generators;

/// <summary>
/// Генератор арифметических примеров: + − × ÷.
/// На лёгком уровне используется только сложение и вычитание,
/// на среднем добавляется умножение, на сложном — деление без остатка.
/// </summary>
public sealed class ArithmeticGenerator : ProblemGeneratorBase
{
    public ArithmeticGenerator(Difficulty difficulty, Random? random = null)
        : base(difficulty, random) { }

    public override ProblemType Type => ProblemType.Arithmetic;

    public override string DisplayName => "Арифметика";

    public override Problem Next()
    {
        // Набор доступных операций зависит от сложности.
        var operations = Difficulty switch
        {
            Difficulty.Easy => new[] { '+', '-' },
            Difficulty.Medium => new[] { '+', '-', '*' },
            _ => new[] { '+', '-', '*', '/' }
        };

        var op = operations[Random.Next(operations.Length)];
        int a, b, answer;

        switch (op)
        {
            case '+':
                a = Next(1, MaxOperand);
                b = Next(1, MaxOperand);
                answer = a + b;
                break;

            case '-':
                // Гарантируем неотрицательный результат: a >= b.
                a = Next(1, MaxOperand);
                b = Next(1, a);
                answer = a - b;
                break;

            case '*':
                // При умножении сужаем множители, чтобы числа не были огромными.
                a = Next(2, Math.Max(2, MaxOperand / 5));
                b = Next(2, Math.Max(2, MaxOperand / 5));
                answer = a * b;
                break;

            default: // '/'
                // Сначала придумываем ответ и делитель, потом считаем делимое —
                // так деление всегда получается нацело.
                b = Next(2, 9);
                answer = Next(2, Math.Max(2, MaxOperand / 5));
                a = b * answer;
                break;
        }

        var text = $"{a} {op} {b} = ?";
        return new Problem(text, answer.ToString(CultureInfo.InvariantCulture), Type);
    }
}
