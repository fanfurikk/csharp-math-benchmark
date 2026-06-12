namespace MathTrainer.Core;

/// <summary>
/// Тип математических заданий. Определяет, какой генератор будет использован.
/// </summary>
public enum ProblemType
{
    /// <summary>Арифметика: сложение, вычитание, умножение, деление.</summary>
    Arithmetic,

    /// <summary>Действия с обыкновенными дробями.</summary>
    Fractions,

    /// <summary>Проценты: найти процент от числа.</summary>
    Percentages,

    /// <summary>Простые линейные уравнения вида a·x + b = c.</summary>
    Equations
}

/// <summary>
/// Уровень сложности. Влияет на диапазон чисел и набор операций.
/// </summary>
public enum Difficulty
{
    /// <summary>Лёгкий — небольшие числа.</summary>
    Easy,

    /// <summary>Средний — числа побольше, больше операций.</summary>
    Medium,

    /// <summary>Сложный — крупные числа, все операции.</summary>
    Hard
}

/// <summary>
/// Режим работы тренировочной сессии.
/// </summary>
public enum SessionMode
{
    /// <summary>Тренировка без ограничения по времени.</summary>
    Practice,

    /// <summary>На время — у сессии есть общий лимит в секундах.</summary>
    Timed
}
