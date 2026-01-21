using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Клуб_6.Models;
using Microsoft.EntityFrameworkCore;
using System.Windows.Media;

namespace Клуб_6.Окна
{
    public partial class Result_card : Window, INotifyPropertyChanged
    {
        private Клуб6Context _context;
        private int _eventId;
        private Event _event;

        public ObservableCollection<DogResultViewModel> DogResults { get; set; }
        public ObservableCollection<DisciplineTableRow> DisciplineTableRows { get; set; }
        public ObservableCollection<CriterionTableRow> CriterionTableRows { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Result_card(int eventId)
        {
            InitializeComponent();
            _context = new Клуб6Context();
            _eventId = eventId;
            DogResults = new ObservableCollection<DogResultViewModel>();
            DisciplineTableRows = new ObservableCollection<DisciplineTableRow>();
            CriterionTableRows = new ObservableCollection<CriterionTableRow>();

            LoadEventData();
            DataContext = this;
        }

        private void LoadEventData()
        {
            try
            {
                _event = _context.Events
                    .Include(e => e.Composition)
                    .Include(e => e.Status)
                    .FirstOrDefault(e => e.EventId == _eventId);

                if (_event == null)
                {
                    MessageBox.Show("Мероприятие не найдено", "Ошибка");
                    Close();
                    return;
                }

                txtTitle.Text = "КАРТОЧКА МЕРОПРИЯТИЯ";
                txtName.Text = _event.Composition?.Title ?? "";
                txtEventDate.Text = _event.EventDate.ToString("dd.MM.yyyy");
                txtEventVenue.Text = _event.EventVenue ?? "Не указано";
                txtComposition.Text = _event.Composition?.Title ?? "Не указано";
                txtOrganization.Text = _event.Organization ?? "Не указано";
                txtStatus.Text = _event.Status?.StatusName ?? "Неизвестно";
                txtHost.Text = _event.Host ?? "Неизвестно";

                var judges = new List<string>();
                if (!string.IsNullOrWhiteSpace(_event.Judge1))
                    judges.Add(_event.Judge1);
                if (!string.IsNullOrWhiteSpace(_event.Judge2))
                    judges.Add(_event.Judge2);
                txtJudges.Text = judges.Any() ? string.Join(", ", judges) : "Не указаны";

                LoadDogResults();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка");
            }
        }

        private void LoadDogResults()
        {
            try
            {
                DogResults.Clear();
                DisciplineTableRows.Clear();
                CriterionTableRows.Clear();

                // Загружаем собак
                var dogsInEvent = _context.DogLists
                    .Include(dl => dl.Dog)
                    .Include(dl => dl.DogDisciplines)
                    .Where(dl => dl.EventId == _eventId)
                    .OrderBy(dl => dl.DogName)
                    .ToList();

                if (!dogsInEvent.Any())
                {
                    txtNoDogs.Visibility = Visibility.Visible;
                    return;
                }

                txtNoDogs.Visibility = Visibility.Collapsed;

                // Загружаем дисциплины мероприятия
                var eventDisciplines = _context.Disciplines
                    .Where(d => d.CompositionID == _event.Composition.CompositionId)
                    .OrderBy(d => d.DisciplineId)
                    .ToList();

                // Загружаем критерии мероприятия
                var allCriteria = _context.Criteria
                    .Include(c => c.Options)
                    .Where(c => c.CompositionID == _event.Composition.CompositionId)
                    .OrderBy(c => c.CriterionID)
                    .ToList();

                // Загружаем все результаты по критериям
                var allCriteriaResults = _context.DogCriteriaResultsDogLists
                    .Where(dcr => dogsInEvent.Select(d => d.RecordId).Contains(dcr.RecordId))
                    .ToList();

                // Заполняем данные о собаках (старая логика)
                foreach (var dogList in dogsInEvent)
                {
                    var dogResult = new DogResultViewModel
                    {
                        DogName = dogList.DogName ?? "Без имени",
                        ChipNumber = dogList.Dog?.ChipNumber.ToString() ?? "Не указан",
                        Breed = dogList.Dog?.Breed ?? "Не указана",
                        Owner = dogList.Owner ?? "Не указан",
                        TrialPassedScore = dogList.TrialPassed,
                        TrialFailedReason = dogList.TrialFailed
                    };

                    bool hasPassed = dogList.TrialPassed.HasValue && dogList.TrialPassed > 0;
                    bool hasFailed = !string.IsNullOrWhiteSpace(dogList.TrialFailed);

                    if (hasPassed)
                    {
                        dogResult.ShowTrialPassed = Visibility.Visible;
                        dogResult.ShowTrialFailed = Visibility.Collapsed;
                        dogResult.ShowNoResults = Visibility.Collapsed;
                    }
                    else if (hasFailed)
                    {
                        dogResult.ShowTrialPassed = Visibility.Collapsed;
                        dogResult.ShowTrialFailed = Visibility.Visible;
                        dogResult.ShowNoResults = Visibility.Collapsed;
                    }
                    else
                    {
                        dogResult.ShowTrialPassed = Visibility.Collapsed;
                        dogResult.ShowTrialFailed = Visibility.Collapsed;
                        dogResult.ShowNoResults = Visibility.Visible;
                    }

                    // Загружаем результаты по дисциплинам
                    foreach (var discipline in eventDisciplines)
                    {
                        var dogDiscipline = dogList.DogDisciplines
                            .FirstOrDefault(dd => dd.DisciplineId == discipline.DisciplineId);

                        if (dogDiscipline != null)
                        {
                            dogResult.DisciplineResults.Add(new DisciplineResult
                            {
                                DisciplineName = discipline.DisciplineName ?? "Дисциплина",
                                Score = dogDiscipline.Score,
                                Coefficient = discipline.Coefficient ?? 1
                            });
                        }
                    }

                    // Загружаем результаты по критериям
                    var dogCriteriaResults = allCriteriaResults
                        .Where(dcr => dcr.RecordId == dogList.RecordId)
                        .ToList();

                    foreach (var criterion in allCriteria)
                    {
                        var criterionResult = dogCriteriaResults
                            .FirstOrDefault(dcr => dcr.CriterionId == criterion.CriterionID);

                        if (criterionResult != null)
                        {
                            var option = criterion.Options
                                .FirstOrDefault(o => o.OptionId == criterionResult.OptionId);

                            string selectedOption = option?.OptionValue ?? "Вариант";
                            string textAnswer = criterionResult.UserInput;

                            dogResult.CriterionResults.Add(new CriterionResult
                            {
                                CriterionName = criterion.CriterionName ?? "Критерий",
                                SelectedOption = selectedOption,
                                TextAnswer = textAnswer
                            });
                        }
                    }

                    DogResults.Add(dogResult);
                }

                // СОЗДАЕМ ТАБЛИЦУ ДИСЦИПЛИН
                // Заголовок таблицы - имена собак
                var headerRow = new DisciplineTableRow { IsHeader = true };
                headerRow.DogName = "Дисциплина";

                foreach (var dog in DogResults)
                {
                    headerRow.Scores.Add(dog.DogName); // Имена собак в заголовках
                }

                headerRow.Total = "Итог";
                headerRow.Status = "Статус";
                DisciplineTableRows.Add(headerRow);

                // Данные по дисциплинам
                foreach (var discipline in eventDisciplines)
                {
                    var row = new DisciplineTableRow
                    {
                        DogName = discipline.DisciplineName ?? "Дисциплина",
                        IsHeader = false
                    };

                    int rowTotal = 0;

                    foreach (var dog in DogResults)
                    {
                        var dogDiscipline = dog.DisciplineResults
                            .FirstOrDefault(dr => dr.DisciplineName == (discipline.DisciplineName ?? "Дисциплина"));

                        if (dogDiscipline != null && dogDiscipline.Score.HasValue)
                        {
                            int score = dogDiscipline.Score.Value * (discipline.Coefficient ?? 1);
                            row.Scores.Add(score.ToString());
                            rowTotal += score;
                        }
                        else
                        {
                            row.Scores.Add("-");
                        }
                    }

                    row.Total = rowTotal.ToString();
                    row.Status = "-";
                    DisciplineTableRows.Add(row);
                }

                // Итоговая строка
                var totalRow = new DisciplineTableRow
                {
                    DogName = "ИТОГО",
                    IsHeader = true,
                    IsTotalRow = true
                };

                foreach (var dog in DogResults)
                {
                    int dogTotal = dog.DisciplineResults.Sum(dr => dr.FinalScore ?? 0);
                    totalRow.Scores.Add(dogTotal.ToString());
                }

                // Общий итог
                int grandTotal = DogResults.Sum(d => d.DisciplineResults.Sum(dr => dr.FinalScore ?? 0));
                totalRow.Total = grandTotal.ToString();
                totalRow.Status = "-";
                DisciplineTableRows.Add(totalRow);

                // СОЗДАЕМ ТАБЛИЦУ КРИТЕРИЕВ
                // Заголовок таблицы
                var critHeaderRow = new CriterionTableRow { IsHeader = true };
                critHeaderRow.CriterionName = "Критерий";

                foreach (var dog in DogResults)
                {
                    critHeaderRow.Answers.Add(dog.DogName); // Имена собак в заголовках
                }

                CriterionTableRows.Add(critHeaderRow);

                // Данные по критериям
                foreach (var criterion in allCriteria)
                {
                    var row = new CriterionTableRow
                    {
                        CriterionName = criterion.CriterionName ?? "Критерий",
                        IsHeader = false
                    };

                    foreach (var dog in DogResults)
                    {
                        var dogCriterion = dog.CriterionResults
                            .FirstOrDefault(cr => cr.CriterionName == (criterion.CriterionName ?? "Критерий"));

                        if (dogCriterion != null)
                        {
                            string answer = !string.IsNullOrEmpty(dogCriterion.TextAnswer)
                                ? dogCriterion.TextAnswer
                                : dogCriterion.SelectedOption;

                            // Обрезаем длинные ответы
                            if (answer.Length > 30)
                                answer = answer.Substring(0, 27) + "...";

                            row.Answers.Add(answer);
                        }
                        else
                        {
                            row.Answers.Add("Нет ответа");
                        }
                    }

                    CriterionTableRows.Add(row);
                }

                // Привязываем таблицы
                icDisciplinesTable.ItemsSource = DisciplineTableRows;
                icCriteriaTable.ItemsSource = CriterionTableRows;
                icDogResults.ItemsSource = DogResults;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки результатов собак: {ex.Message}", "Ошибка");
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    // Классы для таблиц
    public class DisciplineTableRow : INotifyPropertyChanged
    {
        public string DogName { get; set; }
        public ObservableCollection<string> Scores { get; set; } = new ObservableCollection<string>();
        public string Total { get; set; }
        public string Status { get; set; }
        public bool IsHeader { get; set; }
        public bool IsTotalRow { get; set; }

        public Brush BackgroundColor
        {
            get
            {
                if (IsHeader) return Brushes.LightGray;
                if (IsTotalRow) return Brushes.LightGreen;
                return Brushes.White;
            }
        }

        public FontWeight FontWeight => IsHeader || IsTotalRow ? FontWeights.Bold : FontWeights.Normal;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class CriterionTableRow : INotifyPropertyChanged
    {
        public string CriterionName { get; set; }
        public ObservableCollection<string> Answers { get; set; } = new ObservableCollection<string>();
        public bool IsHeader { get; set; }

        public Brush BackgroundColor => IsHeader ? Brushes.LightGray : Brushes.White;
        public FontWeight FontWeight => IsHeader ? FontWeights.Bold : FontWeights.Normal;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    // Старые классы (оставляем как было)
    public class DogResultViewModel : INotifyPropertyChanged
    {
        public string DogName { get; set; }
        public string ChipNumber { get; set; }
        public string Breed { get; set; }
        public string Owner { get; set; }
        public int? TrialPassedScore { get; set; }
        public string TrialFailedReason { get; set; }

        private Visibility _showTrialPassed = Visibility.Collapsed;
        public Visibility ShowTrialPassed
        {
            get => _showTrialPassed;
            set
            {
                _showTrialPassed = value;
                OnPropertyChanged();
            }
        }

        private Visibility _showTrialFailed = Visibility.Collapsed;
        public Visibility ShowTrialFailed
        {
            get => _showTrialFailed;
            set
            {
                _showTrialFailed = value;
                OnPropertyChanged();
            }
        }

        private Visibility _showNoResults = Visibility.Visible;
        public Visibility ShowNoResults
        {
            get => _showNoResults;
            set
            {
                _showNoResults = value;
                OnPropertyChanged();
            }
        }

        public List<DisciplineResult> DisciplineResults { get; set; } = new();
        public List<CriterionResult> CriterionResults { get; set; } = new();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class DisciplineResult
    {
        public string DisciplineName { get; set; }
        public int? Score { get; set; }
        public int? Coefficient { get; set; }
        public int? FinalScore => Score.HasValue && Coefficient.HasValue
            ? Score.Value * Coefficient.Value
            : null;
    }

    public class CriterionResult
    {
        public string CriterionName { get; set; }
        public string SelectedOption { get; set; }
        public string TextAnswer { get; set; }
    }
}