using MathTrainer.Core.Models;

namespace MathTrainer.Core.Sessions;

/// <summary>
/// Запись об одном отвеченном задании: само задание, ответ пользователя,
/// верность и время, потраченное на ответ. Из таких записей складывается
/// история сессии (коллекция).
/// </summary>
public sealed class AnswerRecord
{
    public Problem Problem { get; }
    public string UserAnswer { get; }
    public bool IsCorrect { get; }
    public TimeSpan TimeSpent { get; }

    public AnswerRecord(Problem problem, string userAnswer, bool isCorrect, TimeSpan timeSpent)
    {
        Problem = problem;
        UserAnswer = userAnswer;
        IsCorrect = isCorrect;
        TimeSpent = timeSpent;
    }
}
