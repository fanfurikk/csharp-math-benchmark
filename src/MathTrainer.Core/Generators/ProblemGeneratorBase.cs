using MathTrainer.Core.Models;

namespace MathTrainer.Core.Generators;

/// <summary>
/// Базовый абстрактный класс для всех генераторов заданий.
/// Содержит общую инфраструктуру: генератор случайных чисел и
/// вспомогательные методы выбора диапазона по уровню сложности.
/// Наследники переопределяют только <see cref="Next"/> и <see cref="Type"/>.
/// </summary>
public abstract class ProblemGeneratorBase : IProblemGenerator
{
    /// <summary>Источник случайности. Защищён, чтобы им пользовались наследники.</summary>
    protected readonly Random Random;

    /// <summary>
    /// Создаёт генератор. Можно передать собственный <see cref="Random"/>
    /// (например, с фиксированным seed для воспроизводимых тестов).
    /// </summary>
    protected ProblemGeneratorBase(Difficulty difficulty, Random? random = null)
    {
        Difficulty = difficulty;
        Random = random ?? new Random();
    }

    /// <inheritdoc/>
    public Difficulty Difficulty { get; set; }

    /// <inheritdoc/>
    public abstract ProblemType Type { get; }

    /// <inheritdoc/>
    public abstract string DisplayName { get; }

    /// <inheritdoc/>
    public abstract Problem Next();

    /// <summary>
    /// Возвращает верхнюю границу диапазона чисел в зависимости от сложности.
    /// Общая логика для всех наследников, чтобы не дублировать код.
    /// </summary>
    protected int MaxOperand => Difficulty switch
    {
        Difficulty.Easy => 10,
        Difficulty.Medium => 50,
        Difficulty.Hard => 100,
        _ => 10
    };

    /// <summary>Случайное целое в диапазоне [min, max] включительно.</summary>
    protected int Next(int min, int max) => Random.Next(min, max + 1);
}
