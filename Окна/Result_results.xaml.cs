using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Клуб_6.Models;
using Microsoft.EntityFrameworkCore;

namespace Клуб_6.Окна
{
    public partial class Result_results : Window, INotifyPropertyChanged
    {
        private Клуб6Context _context;
        private int _eventId;
        private Event _событие;
        private Dog _собака;
        private int _currentDogIndex = 0;
        private List<DogList> _allDogsInEvent;
        private const int STATUS_ID_FOR_SUCCESS = 2;

        public ObservableCollection<CriterionViewModel> Criteria { get; set; }
        public ObservableCollection<DisciplineViewModel> Disciplines { get; set; }
        public ObservableCollection<EventStatus> EventStatuses { get; set; }
        public ObservableCollection<DogList> AllDogsInEvent { get; set; }

        private string _dogInfoText;
        public string DogInfoText
        {
            get => _dogInfoText;
            set { _dogInfoText = value; OnPropertyChanged(); }
        }

        private string _eventNameText;
        public string EventNameText
        {
            get => _eventNameText;
            set { _eventNameText = value; OnPropertyChanged(); }
        }

        private string _eventDateText;
        public string EventDateText
        {
            get => _eventDateText;
            set { _eventDateText = value; OnPropertyChanged(); }
        }

        private string _dogCounterText;
        public string DogCounterText
        {
            get => _dogCounterText;
            set { _dogCounterText = value; OnPropertyChanged(); }
        }

        private bool _isPrevDogEnabled;
        public bool IsPrevDogEnabled
        {
            get => _isPrevDogEnabled;
            set { _isPrevDogEnabled = value; OnPropertyChanged(); }
        }

        private bool _isNextDogEnabled;
        public bool IsNextDogEnabled
        {
            get => _isNextDogEnabled;
            set { _isNextDogEnabled = value; OnPropertyChanged(); }
        }

        public int CurrentDogIndex
        {
            get => _currentDogIndex;
            set
            {
                if (_currentDogIndex != value && value >= 0 && value < (_allDogsInEvent?.Count ?? 0))
                {
                    SaveCurrentDogResults();
                    _currentDogIndex = value;
                    LoadCurrentDog();
                    UpdateDogCounter();
                    LoadExistingResults();
                    CalculateTrialPassed();
                    OnPropertyChanged();
                }
            }
        }

        private int? _selectedStatusId;
        public int? SelectedStatusId
        {
            get => _selectedStatusId;
            set
            {
                if (_selectedStatusId != value)
                {
                    _selectedStatusId = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _trialPassed;
        public int? TrialPassed
        {
            get => _trialPassed;
            set
            {
                if (_trialPassed != value)
                {
                    _trialPassed = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _trialFailed;
        public string TrialFailed
        {
            get => _trialFailed;
            set
            {
                if (_trialFailed != value)
                {
                    _trialFailed = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Result_results(int eventId)
        {
            InitializeComponent();
            _context = new Клуб6Context();
            _eventId = eventId;

            Criteria = new ObservableCollection<CriterionViewModel>();
            Disciplines = new ObservableCollection<DisciplineViewModel>();
            EventStatuses = new ObservableCollection<EventStatus>();
            AllDogsInEvent = new ObservableCollection<DogList>();

            LoadData();
            DataContext = this;
        }

        private void LoadData()
        {
            try
            {
                _событие = _context.Events
                    .Include(e => e.Composition)
                    .Include(e => e.Status)
                    .FirstOrDefault(e => e.EventId == _eventId);

                if (_событие == null)
                {
                    MessageBox.Show("Мероприятие не найдено", "Ошибка");
                    return;
                }

                EventDateText = _событие.EventDate.ToString("dd.MM.yyyy");

                _allDogsInEvent = _context.DogLists
                    .Include(dl => dl.Dog)
                    .Where(dl => dl.EventId == _eventId)
                    .OrderBy(dl => dl.DogName)
                    .ToList();

                AllDogsInEvent.Clear();
                foreach (var dog in _allDogsInEvent)
                {
                    AllDogsInEvent.Add(dog);
                }

                LoadEventStatuses();

                // Загружаем критерии, варианты и дисциплины по CompositionID мероприятия
                LoadCriteria();
                LoadDisciplines();

                if (_allDogsInEvent.Any())
                {
                    LoadCurrentDog();
                    UpdateDogCounter();
                    LoadExistingResults();
                    CalculateTrialPassed();
                }
                else
                {
                    MessageBox.Show("В мероприятии нет собак", "Информация");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка");
            }
        }


        private void CalculateTrialPassed()
        {
            if (Disciplines == null || !Disciplines.Any())
            {
                TrialPassed = null;
                return;
            }

            var total = Disciplines.Sum(d => d.FinalScore ?? 0);
            TrialPassed = total > 0 ? total : (int?)null;
        }

        private void LoadCurrentDog()
        {
            if (_currentDogIndex < 0 || _currentDogIndex >= _allDogsInEvent.Count)
                return;

            var currentDogList = _allDogsInEvent[_currentDogIndex];
            _собака = currentDogList.Dog;

            if (_собака != null)
            {
                DogInfoText = $"СОБАКА: {_собака.DogName} | Чип: {_собака.ChipNumber} | Порода: {_собака.Breed}";
            }
        }

        private void UpdateDogCounter()
        {
            DogCounterText = $"{_currentDogIndex + 1} из {_allDogsInEvent.Count}";
            IsPrevDogEnabled = _currentDogIndex > 0;
            IsNextDogEnabled = _currentDogIndex < _allDogsInEvent.Count - 1;
        }

        private void LoadDisciplines()
        {
            try
            {
                Disciplines.Clear();

                if (_событие?.Composition?.CompositionId == null)
                {
                    MessageBox.Show("У мероприятия не указан шаблон (Composition)", "Ошибка");
                    return;
                }

                // Загружаем дисциплины только для этого CompositionID
                var всеДисциплины = _context.Disciplines
                    .Where(d => d.CompositionID == _событие.Composition.CompositionId)
                    .ToList();

                if (!всеДисциплины.Any())
                {
                    MessageBox.Show("Для этого шаблона не найдены дисциплины", "Информация");
                    return;
                }

                foreach (var дисциплина in всеДисциплины)
                {
                    var дисциплинаVM = new DisciplineViewModel
                    {
                        DisciplineId = дисциплина.DisciplineId,
                        DisciplineName = дисциплина.DisciplineName ?? "Дисциплина",
                        Coefficient = дисциплина.Coefficient ?? 1
                    };

                    дисциплинаVM.OnWorkingScoreChanged += (sender, args) =>
                    {
                        CalculateTrialPassed();
                    };

                    Disciplines.Add(дисциплинаVM);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки дисциплин: {ex.Message}", "Ошибка");
            }
        }

        private void LoadCriteria()
        {
            try
            {
                Criteria.Clear();

                if (_событие?.Composition?.CompositionId == null)
                {
                    MessageBox.Show("У мероприятия не указан шаблон (Composition)", "Ошибка");
                    return;
                }

                // Также установите название мероприятия
                EventNameText = _событие.Composition?.Title ?? "Без названия";

                // Загружаем критерии только для этого CompositionID
                var allCriteria = _context.Criteria
                    .Where(c => c.CompositionID == _событие.Composition.CompositionId)
                    .Include(c => c.Options)
                    .ToList();

                if (!allCriteria.Any())
                {
                    MessageBox.Show("Для этого шаблона не найдены критерии", "Информация");
                    return;
                }

                foreach (var criterion in allCriteria)
                {
                    var criterionVM = new CriterionViewModel
                    {
                        CriterionId = criterion.CriterionID,
                        CriterionName = criterion.CriterionName ?? "Критерий"
                    };

                    // Загружаем варианты для этого критерия
                    foreach (var option in criterion.Options)
                    {
                        // Только радио-варианты
                        if (option.OptionType == "radio" || option.OptionType == "standard")
                        {
                            criterionVM.Options.Add(new OptionViewModel
                            {
                                OptionId = option.OptionId,
                                Value = option.OptionValue ?? "Вариант",
                                OptionType = "radio",
                                CriterionId = criterion.CriterionID,
                                ParentCriterion = criterionVM
                            });
                        }
                    }

                    // Добавляем текстовые поля отдельно (они не из Options таблицы)
                    foreach (var option in criterion.Options)
                    {
                        if (option.OptionType == "text" || option.OptionType == "text_input")
                        {
                            criterionVM.TextOptions.Add(new TextOptionViewModel
                            {
                                OptionId = option.OptionId, // Это ID текстового поля из Options
                                Text = option.TextInfo ?? "Введите ответ",
                                OptionType = "text",
                                CriterionId = criterion.CriterionID
                            });
                        }
                    }

                    // Добавляем критерий только если у него есть варианты
                    if (criterionVM.Options.Any() || criterionVM.TextOptions.Any())
                    {
                        Criteria.Add(criterionVM);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки критериев: {ex.Message}", "Ошибка");
            }
        }

        private void LoadExistingResults()
        {
            if (_собака == null) return;

            try
            {
                var currentRecordId = _allDogsInEvent[_currentDogIndex].RecordId;

                // Загружаем существующие ответы из таблицы DogCriteriaResultsDogLists
                var existingLinks = _context.DogCriteriaResultsDogLists
                    .Where(l => l.RecordId == currentRecordId)
                    .ToList();

                // Сбрасываем все выборы
                foreach (var criterionVM in Criteria)
                {
                    criterionVM.HasTextAnswer = false;
                    foreach (var option in criterionVM.Options)
                    {
                        option.IsSelected = false;
                    }
                    foreach (var textOption in criterionVM.TextOptions)
                    {
                        textOption.ResultText = "";
                    }
                }

                // Устанавливаем выбранные варианты
                foreach (var link in existingLinks)
                {
                    var criterionVM = Criteria.FirstOrDefault(c => c.CriterionId == link.CriterionId);
                    if (criterionVM != null)
                    {
                        // Ищем OptionId в обоих списках
                        var radioOption = criterionVM.Options.FirstOrDefault(o => o.OptionId == link.OptionId);
                        var textOption = criterionVM.TextOptions.FirstOrDefault(t => t.OptionId == link.OptionId);

                        if (radioOption != null)
                        {
                            // Это радио-вариант
                            radioOption.IsSelected = true;
                        }
                        else if (textOption != null)
                        {
                            // Это текстовый ответ
                            textOption.ResultText = link.UserInput ?? "";
                            criterionVM.HasTextAnswer = !string.IsNullOrWhiteSpace(link.UserInput);
                        }
                    }
                }

                // Загружаем результаты дисциплин (остается без изменений)
                var existingDisciplineResults = _context.DogDisciplines
                    .Where(dd => dd.RecordId == currentRecordId)
                    .ToList();

                foreach (var disciplineVM in Disciplines)
                {
                    var existingResult = existingDisciplineResults
                        .FirstOrDefault(dd => dd.DisciplineId == disciplineVM.DisciplineId);

                    if (existingResult != null)
                    {
                        disciplineVM.WorkingScore = existingResult.Score;
                    }
                    else
                    {
                        disciplineVM.WorkingScore = null;
                    }
                }

                // Загружаем результаты испытания из DogList
                var currentDogList = _allDogsInEvent[_currentDogIndex];
                TrialPassed = currentDogList.TrialPassed;
                TrialFailed = currentDogList.TrialFailed;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки существующих результатов: {ex.Message}");
            }
        }

        private void btnPrevDog_Click(object sender, RoutedEventArgs e)
        {
            if (_currentDogIndex > 0)
            {
                SaveCurrentDogResults();
                _currentDogIndex--;
                LoadCurrentDog();
                UpdateDogCounter();
                LoadExistingResults();
                CalculateTrialPassed();
                OnPropertyChanged(nameof(CurrentDogIndex));
            }
        }

        private void btnNextDog_Click(object sender, RoutedEventArgs e)
        {
            if (_currentDogIndex < _allDogsInEvent.Count - 1)
            {
                SaveCurrentDogResults();
                _currentDogIndex++;
                LoadCurrentDog();
                UpdateDogCounter();
                LoadExistingResults();
                CalculateTrialPassed();
                OnPropertyChanged(nameof(CurrentDogIndex));
            }
        }

        private void SaveCurrentDogResults()
        {
            try
            {
                if (_собака == null) return;
                SaveToDatabase(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка автосохранения: {ex.Message}");
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateAllFields())
                    return;

                SaveToDatabase(true);

                if (AreAllDogsCompleted())
                {
                    ChangeEventStatus(STATUS_ID_FOR_SUCCESS);
                }

                if (_currentDogIndex < _allDogsInEvent.Count - 1)
                {
                    _currentDogIndex++;
                    LoadCurrentDog();
                    UpdateDogCounter();
                    LoadExistingResults();
                    CalculateTrialPassed();
                    OnPropertyChanged(nameof(CurrentDogIndex));

                    MessageBox.Show("Результаты сохранены. Переход к следующей собаке.", "Успех");
                }
                else
                {
                    MessageBox.Show("Все результаты сохранены! Все собаки обработаны.", "Успех");

                    new Schedule().Show();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка");
            }
        }

        private bool ValidateAllFields()
        {
            foreach (var критерий in Criteria)
            {
                // Если есть текстовый ответ, пропускаем проверку радио-вариантов
                if (критерий.HasTextAnswer)
                {
                    // Проверяем, что текстовый ответ заполнен
                    if (критерий.TextOptions.Any() &&
                        string.IsNullOrWhiteSpace(критерий.TextOptions.First().ResultText))
                    {
                        MessageBox.Show($"Заполните текстовый ответ для критерия '{критерий.CriterionName}'", "Ошибка");
                        return false;
                    }
                    continue;
                }

                // Проверяем радио-варианты (только если нет текстового ответа)
                if (критерий.Options.Any())
                {
                    var selectedCount = критерий.Options.Count(o => o.IsSelected);
                    if (selectedCount == 0)
                    {
                        MessageBox.Show($"Выберите вариант для критерия '{критерий.CriterionName}' или ответьте текстом", "Ошибка");
                        return false;
                    }
                }
            }

            return true;
        }

        private bool AreAllDogsCompleted()
        {
            return _currentDogIndex == _allDogsInEvent.Count - 1;
        }

        private void ChangeEventStatus(int statusId)
        {
            try
            {
                var событие = _context.Events.Find(_eventId);
                if (событие != null)
                {
                    событие.StatusId = statusId;
                    _context.SaveChanges();

                    SelectedStatusId = statusId;
                    MessageBox.Show($"Статус мероприятия изменен на ID: {statusId}", "Информация");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка изменения статуса: {ex.Message}", "Ошибка");
            }
        }

        private void SaveToDatabase(bool showMessage)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    SaveResultsToBridgeTable();
                    SaveDisciplineResults();
                    SaveDogListResults();

                    _context.SaveChanges();
                    transaction.Commit();

                    if (showMessage)
                    {
                        // Сообщение показывается в btnSave_Click
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception($"Ошибка сохранения в БД: {ex.Message}", ex);
                }
            }
        }

        private void SaveResultsToBridgeTable()
        {
            var currentRecordId = _allDogsInEvent[_currentDogIndex].RecordId;

            // Удаляем старые результаты
            var oldLinks = _context.DogCriteriaResultsDogLists
                .Where(l => l.RecordId == currentRecordId)
                .ToList();
            _context.DogCriteriaResultsDogLists.RemoveRange(oldLinks);

            // Сохраняем ВЫБРАННЫЕ РАДИО-ВАРИАНТЫ
            foreach (var критерий in Criteria)
            {
                var selectedRadioOption = критерий.Options.FirstOrDefault(o => o.IsSelected);
                if (selectedRadioOption != null)
                {
                    var link = new DogCriteriaResults_DogList
                    {
                        RecordId = currentRecordId,
                        CriterionId = критерий.CriterionId,
                        OptionId = selectedRadioOption.OptionId, // ID из Options
                        UserInput = null
                    };
                    _context.DogCriteriaResultsDogLists.Add(link);
                }

                // Сохраняем ТЕКСТОВЫЕ ОТВЕТЫ (не привязаны к OptionId)
                foreach (var textOption in критерий.TextOptions)
                {
                    if (!string.IsNullOrWhiteSpace(textOption.ResultText))
                    {
                        var textLink = new DogCriteriaResults_DogList
                        {
                            RecordId = currentRecordId,
                            CriterionId = критерий.CriterionId,
                            OptionId = textOption.OptionId, // ← OptionId ТЕКСТОВОГО ПОЛЯ из Options
                            UserInput = textOption.ResultText
                        };
                        _context.DogCriteriaResultsDogLists.Add(textLink);
                    }
                }
            }
        }

        private void SaveDisciplineResults()
        {
            var currentRecordId = _allDogsInEvent[_currentDogIndex].RecordId;

            foreach (var disciplineVM in Disciplines)
            {
                var existingResult = _context.DogDisciplines
                    .FirstOrDefault(dd => dd.RecordId == currentRecordId
                        && dd.DisciplineId == disciplineVM.DisciplineId);

                if (existingResult != null)
                {
                    existingResult.Score = disciplineVM.WorkingScore; // Используем Score вместо WorkingScore
                    _context.DogDisciplines.Update(existingResult);
                }
                else
                {
                    var dogDiscipline = new DogDiscipline
                    {
                        RecordId = currentRecordId,
                        DisciplineId = disciplineVM.DisciplineId,
                        Score = disciplineVM.WorkingScore // Используем Score вместо WorkingScore
                    };
                    _context.DogDisciplines.Add(dogDiscipline);
                }
            }
        }

        private void SaveDogListResults()
        {
            var currentRecord = _allDogsInEvent[_currentDogIndex];

            currentRecord.TrialPassed = TrialPassed;
            currentRecord.TrialFailed = string.IsNullOrWhiteSpace(TrialFailed) ? null : TrialFailed;

            _context.DogLists.Update(currentRecord);
        }

        private void LoadEventStatuses()
        {
            try
            {
                EventStatuses.Clear();

                var statuses = _context.EventStatuses
                    .OrderBy(s => s.StatusId)
                    .ToList();

                foreach (var status in statuses)
                {
                    EventStatuses.Add(status);
                }

                if (_событие != null)
                {
                    SelectedStatusId = _событие.StatusId;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки статусов: {ex.Message}", "Ошибка");
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            new Schedule().Show();
            this.Close();
        }
    }

    public class TextOptionViewModel : INotifyPropertyChanged
    {
        public int OptionId { get; set; }
        public string Text { get; set; }
        public string OptionType { get; set; }
        public int CriterionId { get; set; }

        private string _resultText;
        public string ResultText
        {
            get => _resultText;
            set
            {
                if (_resultText != value)
                {
                    _resultText = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class CriterionViewModel : INotifyPropertyChanged
    {
        public int CriterionId { get; set; }
        public string CriterionName { get; set; }

        public bool ShowOptions => Options.Any();
        public bool ShowTextOptions => TextOptions.Any();

        public ObservableCollection<TextOptionViewModel> TextOptions { get; set; }
        public ObservableCollection<OptionViewModel> Options { get; set; }

        private bool _hasTextAnswer;
        public bool HasTextAnswer
        {
            get => _hasTextAnswer;
            set
            {
                if (_hasTextAnswer != value)
                {
                    _hasTextAnswer = value;
                    OnPropertyChanged();

                    // Если выбран текстовый ответ, снимаем выбор с радио-вариантов
                    if (_hasTextAnswer)
                    {
                        foreach (var option in Options)
                        {
                            option.IsSelected = false;
                        }
                    }
                }
            }
        }

        public CriterionViewModel()
        {
            Options = new ObservableCollection<OptionViewModel>();
            TextOptions = new ObservableCollection<TextOptionViewModel>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class OptionViewModel : INotifyPropertyChanged
    {
        public int OptionId { get; set; }
        public string Value { get; set; }
        public string OptionType { get; set; }
        public int CriterionId { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();

                    if (_isSelected && ParentCriterion != null)
                    {
                        // Снимаем текстовый ответ
                        ParentCriterion.HasTextAnswer = false;

                        // Для радио-вариантов снимаем выбор с других
                        if (OptionType == "radio")
                        {
                            foreach (var option in ParentCriterion.Options)
                            {
                                if (option != this)
                                {
                                    option.IsSelected = false;
                                }
                            }
                        }
                    }
                }
            }
        }

        public CriterionViewModel ParentCriterion { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class DisciplineViewModel : INotifyPropertyChanged
    {
        public int DisciplineId { get; set; }
        public string DisciplineName { get; set; }

        private int? _workingScore;
        public int? WorkingScore
        {
            get => _workingScore;
            set
            {
                if (_workingScore != value)
                {
                    _workingScore = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FinalScore));
                    OnWorkingScoreChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private int? _coefficient;
        public int? Coefficient
        {
            get => _coefficient;
            set
            {
                if (_coefficient != value)
                {
                    _coefficient = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FinalScore));
                    OnWorkingScoreChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public int? FinalScore => WorkingScore.HasValue && Coefficient.HasValue
            ? WorkingScore.Value * Coefficient.Value
            : null;

        public event EventHandler OnWorkingScoreChanged;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}