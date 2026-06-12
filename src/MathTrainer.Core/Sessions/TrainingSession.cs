using System.Diagnostics;
using MathTrainer.Core.Generators;
using MathTrainer.Core.Models;

namespace MathTrainer.Core.Sessions;

/// <summary>
/// Тренировочная сессия — центральный класс логики приложения.
/// Управляет последовательностью заданий, проверяет ответы, ведёт счёт
/// и уведомляет подписчиков через события. Не зависит от UI, поэтому
/// одинаково используется и в консоли, и в WPF.
/// </summary>
public sealed class TrainingSession
{
    private readonly IProblemGenerator _generator;
    private readonly List<AnswerRecord> _records = new();
    private readonly Stopwatch _questionTimer = new();

    private Problem? _current;

    /// <summary>Происходит, когда пользователю показывается новое задание.</summary>
    public event EventHandler<ProblemPresentedEventArgs>? ProblemPresented;

    /// <summary>Происходит после проверки очередного ответа.</summary>
    public event EventHandler<AnswerCheckedEventArgs>? AnswerChecked;

    /// <summary>Происходит, когда сессия завершена (все задания отвечены или вышло время).</summary>
    public event EventHandler<SessionFinishedEventArgs>? SessionFinished;

    /// <summary>
    /// Создаёт сессию.
    /// </summary>
    /// <param name="generator">Источник заданий.</param>
    /// <param name="questionCount">Сколько заданий в сессии.</param>
    /// <param name="mode">Режим: обычный или на время.</param>
    /// <param name="timeLimit">Лимит времени для режима <see cref="SessionMode.Timed"/>.</param>
    public TrainingSession(
        IProblemGenerator generator,
        int questionCount,
        SessionMode mode = SessionMode.Practice,
        TimeSpan? timeLimit = null)
    {
        _generator = generator ?? throw new ArgumentNullException(nameof(generator));
        if (questionCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(questionCount), "Заданий должно быть больше нуля.");

        QuestionCount = questionCount;
        Mode = mode;
        TimeLimit = timeLimit ?? TimeSpan.FromSeconds(60);
    }

    /// <summary>Всего заданий в сессии.</summary>
    public int QuestionCount { get; }

    /// <summary>Режим сессии.</summary>
    public SessionMode Mode { get; }

    /// <summary>Лимит времени (используется только в режиме на время).</summary>
    public TimeSpan TimeLimit { get; }

    /// <summary>Тип заданий этой сессии.</summary>
    public ProblemType ProblemType => _generator.Type;

    /// <summary>Сколько заданий уже отвечено.</summary>
    public int Answered => _records.Count;

    /// <summary>Текущий счёт (число верных ответов).</summary>
    public int Score => _records.Count(r => r.IsCorrect);

    /// <summary>Текущее задание (или <c>null</c>, если сессия не запущена/завершена).</summary>
    public Problem? Current => _current;

    /// <summary>Завершена ли сессия.</summary>
    public bool IsFinished { get; private set; }

    /// <summary>Запускает сессию и показывает первое задание.</summary>
    public void Start()
    {
        IsFinished = false;
        _records.Clear();
        PresentNext();
    }

    /// <summary>
    /// Принимает ответ пользователя на текущее задание, проверяет его,
    /// поднимает событие <see cref="AnswerChecked"/> и переходит к следующему.
    /// </summary>
    /// <param name="userAnswer">Ответ пользователя.</param>
    /// <returns><c>true</c>, если ответ оказался верным.</returns>
    public bool SubmitAnswer(string userAnswer)
    {
        if (IsFinished || _current is null)
            throw new InvalidOperationException("Сессия не активна. Сначала вызовите Start().");

        _questionTimer.Stop();
        bool isCorrect = _current.CheckAnswer(userAnswer);

        var record = new AnswerRecord(_current, userAnswer ?? string.Empty, isCorrect, _questionTimer.Elapsed);
        _records.Add(record);

        AnswerChecked?.Invoke(this, new AnswerCheckedEventArgs(record, Answered, QuestionCount));

        PresentNext();
        return isCorrect;
    }

    /// <summary>Показывает следующее задание либо завершает сессию.</summary>
    private void PresentNext()
    {
        if (_records.Count >= QuestionCount)
        {
            Finish();
            return;
        }

        _current = _generator.Next();
        _questionTimer.Restart();
        ProblemPresented?.Invoke(this, new ProblemPresentedEventArgs(_current, Answered + 1, QuestionCount));
    }

    /// <summary>Принудительно завершает сессию (например, по истечении времени).</summary>
    public void Finish()
    {
        if (IsFinished)
            return;

        IsFinished = true;
        _current = null;
        _questionTimer.Reset();

        var stats = new SessionStatistics(_records.AsReadOnly());
        SessionFinished?.Invoke(this, new SessionFinishedEventArgs(stats));
    }

    /// <summary>Возвращает текущую статистику (можно вызывать и до завершения).</summary>
    public SessionStatistics GetStatistics() => new(_records.AsReadOnly());
}
