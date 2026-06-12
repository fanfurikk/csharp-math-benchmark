using MathTrainer.Core.Models;

namespace MathTrainer.Core.Sessions;

/// <summary>
/// Аргументы события «ответ проверен». Передаются подписчикам
/// (GUI или консоли), чтобы те обновили интерфейс.
/// </summary>
public sealed class AnswerCheckedEventArgs : EventArgs
{
    public AnswerRecord Record { get; }

    /// <summary>Номер текущего задания (1-based).</summary>
    public int QuestionNumber { get; }

    /// <summary>Всего заданий в сессии.</summary>
    public int TotalQuestions { get; }

    public AnswerCheckedEventArgs(AnswerRecord record, int questionNumber, int totalQuestions)
    {
        Record = record;
        QuestionNumber = questionNumber;
        TotalQuestions = totalQuestions;
    }
}

/// <summary>
/// Аргументы события «новое задание показано».
/// </summary>
public sealed class ProblemPresentedEventArgs : EventArgs
{
    public Problem Problem { get; }
    public int QuestionNumber { get; }
    public int TotalQuestions { get; }

    public ProblemPresentedEventArgs(Problem problem, int questionNumber, int totalQuestions)
    {
        Problem = problem;
        QuestionNumber = questionNumber;
        TotalQuestions = totalQuestions;
    }
}

/// <summary>
/// Аргументы события «сессия завершена». Содержат итоговую статистику.
/// </summary>
public sealed class SessionFinishedEventArgs : EventArgs
{
    public SessionStatistics Statistics { get; }

    public SessionFinishedEventArgs(SessionStatistics statistics)
    {
        Statistics = statistics;
    }
}
