using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MathTrainer.Core;
using MathTrainer.Core.Generators;
using MathTrainer.Core.Sessions;
using MathTrainer.Core.Settings;

namespace MathTrainer.Wpf;

/// <summary>
/// Логика главного окна. Связывает элементы интерфейса с тренировочной
/// сессией из библиотеки MathTrainer.Core и реагирует на её события.
/// </summary>
public partial class MainWindow : Window
{
    private AppSettings _settings;
    private TrainingSession? _session;

    // Таймер для режима «на время»: обновляет обратный отсчёт раз в секунду.
    private readonly DispatcherTimer _timer;
    private DateTime _deadline;

    public MainWindow()
    {
        InitializeComponent();

        _settings = SettingsStorage.Load();

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += Timer_Tick;

        PopulateComboBoxes();
        ApplySettingsToUi();
    }

    // ---------- Инициализация интерфейса ----------

    /// <summary>Заполняет выпадающие списки понятными русскими подписями.</summary>
    private void PopulateComboBoxes()
    {
        // Для типа заданий берём человекочитаемые названия прямо из генераторов.
        foreach (var type in GeneratorFactory.AllTypes)
        {
            var name = GeneratorFactory.Create(type, Difficulty.Easy).DisplayName;
            TypeBox.Items.Add(new ComboBoxItem { Content = name, Tag = type });
        }

        AddItem(DifficultyBox, "Лёгкий", Difficulty.Easy);
        AddItem(DifficultyBox, "Средний", Difficulty.Medium);
        AddItem(DifficultyBox, "Сложный", Difficulty.Hard);

        AddItem(ModeBox, "Тренировка", SessionMode.Practice);
        AddItem(ModeBox, "На время", SessionMode.Timed);
    }

    private static void AddItem(ComboBox box, string label, object tag) =>
        box.Items.Add(new ComboBoxItem { Content = label, Tag = tag });

    /// <summary>Выставляет элементы интерфейса в соответствии с настройками.</summary>
    private void ApplySettingsToUi()
    {
        SelectByTag(TypeBox, _settings.ProblemType);
        SelectByTag(DifficultyBox, _settings.Difficulty);
        SelectByTag(ModeBox, _settings.Mode);
        CountBox.Text = _settings.QuestionCount.ToString();
        TimeBox.Text = _settings.TimeLimitSeconds.ToString();
    }

    private static void SelectByTag(ComboBox box, object tag)
    {
        foreach (ComboBoxItem item in box.Items)
        {
            if (Equals(item.Tag, tag))
            {
                box.SelectedItem = item;
                return;
            }
        }
        if (box.Items.Count > 0)
            box.SelectedIndex = 0;
    }

    private static T GetTag<T>(ComboBox box) => (T)((ComboBoxItem)box.SelectedItem).Tag;

    /// <summary>Считывает выбранные в интерфейсе значения обратно в настройки.</summary>
    private bool ReadSettingsFromUi()
    {
        if (!int.TryParse(CountBox.Text, out var count) || count <= 0)
        {
            MessageBox.Show("«Заданий» должно быть положительным числом.", "Проверьте ввод",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
        if (!int.TryParse(TimeBox.Text, out var time) || time <= 0)
        {
            MessageBox.Show("«Лимит» должен быть положительным числом секунд.", "Проверьте ввод",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        _settings.ProblemType = GetTag<ProblemType>(TypeBox);
        _settings.Difficulty = GetTag<Difficulty>(DifficultyBox);
        _settings.Mode = GetTag<SessionMode>(ModeBox);
        _settings.QuestionCount = count;
        _settings.TimeLimitSeconds = time;
        return true;
    }

    // ---------- Управление сессией ----------

    private void Start_Click(object sender, RoutedEventArgs e) => StartSession();

    private void StartSession()
    {
        if (!ReadSettingsFromUi())
            return;

        SettingsStorage.Save(_settings);

        var generator = GeneratorFactory.Create(_settings.ProblemType, _settings.Difficulty);
        var timeLimit = TimeSpan.FromSeconds(_settings.TimeLimitSeconds);
        _session = new TrainingSession(generator, _settings.QuestionCount, _settings.Mode, timeLimit);

        // Подписываемся на события сессии — здесь обновляется интерфейс.
        _session.ProblemPresented += OnProblemPresented;
        _session.AnswerChecked += OnAnswerChecked;
        _session.SessionFinished += OnSessionFinished;

        SetRunningState(true);
        FeedbackText.Text = string.Empty;

        if (_settings.Mode == SessionMode.Timed)
        {
            _deadline = DateTime.Now + timeLimit;
            _timer.Start();
        }

        _session.Start();
        AnswerBox.Focus();
    }

    private void Stop_Click(object sender, RoutedEventArgs e) => _session?.Finish();

    private void Submit_Click(object sender, RoutedEventArgs e) => SubmitAnswer();

    private void SubmitAnswer()
    {
        if (_session is null || _session.IsFinished)
            return;

        _session.SubmitAnswer(AnswerBox.Text);
        AnswerBox.Clear();
        AnswerBox.Focus();
    }

    // ---------- Обработчики событий сессии ----------

    private void OnProblemPresented(object? sender, ProblemPresentedEventArgs e)
    {
        QuestionText.Text = e.Problem.Text;
        StatusText.Text = $"Задание {e.QuestionNumber} из {e.TotalQuestions}  •  счёт {_session!.Score}";
    }

    private void OnAnswerChecked(object? sender, AnswerCheckedEventArgs e)
    {
        if (e.Record.IsCorrect)
        {
            FeedbackText.Text = "Верно!";
            FeedbackText.Foreground = Brushes.LimeGreen;
        }
        else
        {
            FeedbackText.Text = $"Неверно. Правильно: {e.Record.Problem.CorrectAnswer}";
            FeedbackText.Foreground = Brushes.OrangeRed;
        }

        Progress.Value = e.QuestionNumber * 100.0 / e.TotalQuestions;
    }

    private void OnSessionFinished(object? sender, SessionFinishedEventArgs e)
    {
        _timer.Stop();
        SetRunningState(false);

        var s = e.Statistics;
        QuestionText.Text = "Сессия завершена";
        Progress.Value = 100;
        StatusText.Text = $"Верно {s.Correct}/{s.Total}  •  точность {s.AccuracyPercent:F0}%";

        MessageBox.Show(
            $"Отвечено заданий: {s.Total}\n" +
            $"Верно: {s.Correct}\n" +
            $"Неверно: {s.Wrong}\n" +
            $"Точность: {s.AccuracyPercent.ToString("F1", CultureInfo.InvariantCulture)}%\n" +
            $"Среднее время: {s.AverageTime.TotalSeconds.ToString("F1", CultureInfo.InvariantCulture)} c",
            "Итоги тренировки", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    /// <summary>Переключает интерфейс между состояниями «идёт сессия» и «ожидание».</summary>
    private void SetRunningState(bool running)
    {
        AnswerBox.IsEnabled = running;
        SubmitButton.IsEnabled = running;
        StopButton.IsEnabled = running;
        StartButton.IsEnabled = !running;
        SettingsPanel.IsEnabled = !running;
    }

    // ---------- Таймер режима «на время» ----------

    private void Timer_Tick(object? sender, EventArgs e)
    {
        var remaining = _deadline - DateTime.Now;
        if (remaining <= TimeSpan.Zero)
        {
            StatusText.Text = "Время вышло!";
            _session?.Finish();
            return;
        }
        StatusText.Text = $"Осталось {remaining.TotalSeconds:F0} c  •  счёт {_session?.Score ?? 0}";
    }

    // ---------- Горячие клавиши ----------

    /// <summary>
    /// Обработка горячих клавиш: Enter — старт/ответ, Esc — стоп, F1 — справка.
    /// </summary>
    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.F1:
                ShowHelp();
                e.Handled = true;
                break;

            case Key.Enter:
                if (_session is { IsFinished: false })
                    SubmitAnswer();
                else if (StartButton.IsEnabled)
                    StartSession();
                e.Handled = true;
                break;

            case Key.Escape:
                _session?.Finish();
                break;
        }
    }

    private void Help_Click(object sender, RoutedEventArgs e) => ShowHelp();

    private static void ShowHelp()
    {
        MessageBox.Show(
            "ТРЕНАЖЁР УСТНОГО СЧЁТА — справка\n\n" +
            "Управление:\n" +
            "  • Enter — начать сессию / подтвердить ответ\n" +
            "  • Esc   — остановить сессию\n" +
            "  • F1    — эта справка\n\n" +
            "Настройки (сверху):\n" +
            "  • Тип заданий: арифметика, дроби, проценты, уравнения\n" +
            "  • Сложность: лёгкий / средний / сложный\n" +
            "  • Режим: тренировка или на время\n" +
            "  • Заданий — сколько примеров в сессии\n" +
            "  • Лимит — секунды для режима «на время»\n\n" +
            "Командная строка (консольная версия):\n" +
            "  MathTrainer.Console --type Fractions --level Medium --count 15\n" +
            "  MathTrainer.Console --help — полный список параметров",
            "Справка (F1)", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    /// <summary>Сохраняем настройки при закрытии окна.</summary>
    protected override void OnClosed(EventArgs e)
    {
        ReadSettingsFromUi();
        SettingsStorage.Save(_settings);
        base.OnClosed(e);
    }
}
