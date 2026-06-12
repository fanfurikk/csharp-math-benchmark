using MathTrainer.Core.Models;

namespace MathTrainer.Core.Generators;

/// <summary>
/// Контракт генератора заданий. Любой источник примеров (арифметика,
/// дроби, проценты, уравнения) реализует этот интерфейс — это позволяет
/// сессии работать с генераторами единообразно (полиморфизм).
/// </summary>
public interface IProblemGenerator
{
    /// <summary>Тип заданий, которые выдаёт генератор.</summary>
    ProblemType Type { get; }

    /// <summary>Текущий уровень сложности.</summary>
    Difficulty Difficulty { get; set; }

    /// <summary>Человекочитаемое название генератора (для меню и CLI).</summary>
    string DisplayName { get; }

    /// <summary>Сгенерировать очередное задание.</summary>
    Problem Next();
}
