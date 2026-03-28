using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Клуб_6.Models;

namespace Клуб_6.Окна
{
    public partial class Result_card : Window, INotifyPropertyChanged
    {
        private КлубContext _context;
        private int _eventId;
        private Event _event;

        public ObservableCollection<DogResultViewModel> DogResults { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Result_card(int eventId)
        {
            InitializeComponent();
            _context = new КлубContext();
            _eventId = eventId;
            DogResults = new ObservableCollection<DogResultViewModel>();

            LoadEventData();
            DataContext = this;
        }

        private void LoadEventData()
        {
            try
            {
                _event = _context.Event
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

                // Загружаем собак мероприятия с их дисциплинами
                var dogsInEvent = _context.DogList
                    .Include(dl => dl.Dog)
                    .Include(dl => dl.DogDisciplines)
                    .Where(dl => dl.EventId == _eventId)
                    .OrderBy(dl => dl.DogName)
                    .ToList();

                if (!dogsInEvent.Any())
                {
                    txtNoDogs.Visibility = Visibility.Visible;
                    lvDogResults.Visibility = Visibility.Collapsed;
                    lvDisciplinesTable.Visibility = Visibility.Collapsed;
                    lvCriteriaTable.Visibility = Visibility.Collapsed;
                    return;
                }

                txtNoDogs.Visibility = Visibility.Collapsed;
                lvDogResults.Visibility = Visibility.Visible;
                lvDisciplinesTable.Visibility = Visibility.Visible;
                lvCriteriaTable.Visibility = Visibility.Visible;

                // Загружаем дисциплины мероприятия
                var eventDisciplines = _context.Discipline
                    .Where(d => d.CompositionID == _event.Composition.CompositionId)
                    .OrderBy(d => d.DisciplineId)
                    .ToList();

                // Загружаем критерии мероприятия
                var allCriteria = _context.Criterion
                    .Include(c => c.Options)
                    .Where(c => c.CompositionID == _event.Composition.CompositionId)
                    .OrderBy(c => c.CriterionID)
                    .ToList();

                // Загружаем все результаты по критериям для собак мероприятия
                var allCriteriaResults = _context.DogCriteriaResultsDogList
                    .Where(dcr => dogsInEvent.Select(d => d.RecordId).Contains(dcr.RecordId))
                    .ToList();

                // Заполняем DogResults
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

                    // Результаты по дисциплинам
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
                        else
                        {
                            // Если нет результата, добавляем запись с null Score
                            dogResult.DisciplineResults.Add(new DisciplineResult
                            {
                                DisciplineName = discipline.DisciplineName ?? "Дисциплина",
                                Score = null,
                                Coefficient = discipline.Coefficient ?? 1
                            });
                        }
                    }

                    // Результаты по критериям
                    var dogCriteriaResults = allCriteriaResults
                        .Where(dcr => dcr.RecordId == dogList.RecordId)
                        .ToList();

                    foreach (var criterion in allCriteria)
                    {
                        var criterionResult = dogCriteriaResults
                            .FirstOrDefault(dcr => dcr.CriterionId == criterion.CriterionID);

                        if (criterionResult != null)
                        {
                            string selectedOption = null;
                            if (criterionResult.OptionId.HasValue)
                            {
                                var option = criterion.Options.FirstOrDefault(o => o.OptionId == criterionResult.OptionId);
                                selectedOption = option?.OptionValue;
                            }
                            string textAnswer = criterionResult.UserInput;

                            dogResult.CriterionResults.Add(new CriterionResult
                            {
                                CriterionName = criterion.CriterionName ?? "Критерий",
                                SelectedOption = selectedOption,
                                TextAnswer = textAnswer
                            });
                        }
                        else
                        {
                            dogResult.CriterionResults.Add(new CriterionResult
                            {
                                CriterionName = criterion.CriterionName ?? "Критерий",
                                SelectedOption = null,
                                TextAnswer = null
                            });
                        }
                    }

                    DogResults.Add(dogResult);
                }

                // Привязываем список собак к ListView
                lvDogResults.ItemsSource = DogResults;

                // Строим таблицы
                BuildDisciplinesTable(DogResults.ToList(), eventDisciplines);
                BuildCriteriaTable(DogResults.ToList(), allCriteria);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки результатов собак: {ex.Message}", "Ошибка");
            }
        }

        private void BuildDisciplinesTable(List<DogResultViewModel> dogs, List<Discipline> disciplines)
        {
            var gridView = new GridView();

            // Колонка "Дисциплина"
            gridView.Columns.Add(new GridViewColumn
            {
                Header = "Дисциплина",
                Width = 180,
                DisplayMemberBinding = new Binding("DisciplineName")
            });

            // Колонки для каждой собаки
            for (int i = 0; i < dogs.Count; i++)
            {
                gridView.Columns.Add(new GridViewColumn
                {
                    Header = dogs[i].DogName,
                    Width = 100,
                    DisplayMemberBinding = new Binding($"Scores[{i}]")
                });
            }

            //// Колонка "Итог по дисциплине"
            //gridView.Columns.Add(new GridViewColumn
            //{
            //    Header = "Итог",
            //    Width = 80,
            //    DisplayMemberBinding = new Binding("Total")
            //});

            lvDisciplinesTable.View = gridView;

            // Формируем строки таблицы
            var rows = new List<DisciplineTableRow>();

            // Строки для каждой дисциплины
            foreach (var discipline in disciplines)
            {
                var row = new DisciplineTableRow
                {
                    DisciplineName = discipline.DisciplineName ?? "Дисциплина",
                    IsHeader = false
                };

                int rowTotal = 0;
                foreach (var dog in dogs)
                {
                    var dogDiscipline = dog.DisciplineResults
                        .FirstOrDefault(dr => dr.DisciplineName == discipline.DisciplineName);
                    if (dogDiscipline?.Score.HasValue == true)
                    {
                        int score = dogDiscipline.Score.Value * (dogDiscipline.Coefficient ?? 1);
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
                rows.Add(row);
            }

            // Итоговая строка
            var totalRow = new DisciplineTableRow
            {
                DisciplineName = "ИТОГО",
                IsHeader = true,
                IsTotalRow = true
            };

            for (int i = 0; i < dogs.Count; i++)
            {
                int dogTotal = dogs[i].DisciplineResults.Sum(dr => dr.FinalScore ?? 0);
                totalRow.Scores.Add(dogTotal.ToString());
            }
            int grandTotal = dogs.Sum(d => d.DisciplineResults.Sum(dr => dr.FinalScore ?? 0));
            totalRow.Total = grandTotal.ToString();
            totalRow.Status = "-";
            rows.Add(totalRow);

            lvDisciplinesTable.ItemsSource = rows;
        }

        private void BuildCriteriaTable(List<DogResultViewModel> dogs, List<Criterion> criteria)
        {
            var gridView = new GridView();

            // Колонка "Критерий"
            gridView.Columns.Add(new GridViewColumn
            {
                Header = "Критерий",
                Width = 200,
                DisplayMemberBinding = new Binding("CriterionName")
            });

            // Колонки для каждой собаки
            for (int i = 0; i < dogs.Count; i++)
            {
                gridView.Columns.Add(new GridViewColumn
                {
                    Header = dogs[i].DogName,
                    Width = 200,
                    DisplayMemberBinding = new Binding($"Answers[{i}]")
                });
            }

            lvCriteriaTable.View = gridView;

            // Формируем строки таблицы
            var rows = new List<CriterionTableRow>();
            foreach (var criterion in criteria)
            {
                var row = new CriterionTableRow
                {
                    CriterionName = criterion.CriterionName ?? "Критерий",
                    IsHeader = false
                };

                foreach (var dog in dogs)
                {
                    var dogCriterion = dog.CriterionResults
                        .FirstOrDefault(cr => cr.CriterionName == criterion.CriterionName);
                    string answer = "-";
                    if (dogCriterion != null)
                    {
                        answer = !string.IsNullOrEmpty(dogCriterion.TextAnswer)
                            ? dogCriterion.TextAnswer
                            : (!string.IsNullOrEmpty(dogCriterion.SelectedOption) ? dogCriterion.SelectedOption : "-");
                        if (answer.Length > 40) answer = answer.Substring(0, 37) + "...";
                    }
                    row.Answers.Add(answer);
                }
                rows.Add(row);
            }

            lvCriteriaTable.ItemsSource = rows;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    // Класс для отображения результата собаки
    public class DogResultViewModel : INotifyPropertyChanged
    {
        public string DogName { get; set; }
        public string ChipNumber { get; set; }
        public string Breed { get; set; }
        public string Owner { get; set; }
        public int? TrialPassedScore { get; set; }
        public string TrialFailedReason { get; set; }

        public string ResultText
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(TrialFailedReason))
                    return $"Испытание не пройдено по причине: {TrialFailedReason}";
                if (TrialPassedScore.HasValue)
                    return $"Испытание пройдено, баллы: {TrialPassedScore.Value}";
                return "Результаты отсутствуют";
            }
        }

        public Brush ResultColor
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(TrialFailedReason))
                    return Brushes.Red;
                if (TrialPassedScore.HasValue)
                    return Brushes.Green;
                return Brushes.Gray;
            }
        }

        public List<DisciplineResult> DisciplineResults { get; set; } = new();
        public List<CriterionResult> CriterionResults { get; set; } = new();

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Класс для дисциплинарных результатов
    public class DisciplineResult
    {
        public string DisciplineName { get; set; }
        public int? Score { get; set; }
        public int? Coefficient { get; set; }
        public int? FinalScore => Score.HasValue && Coefficient.HasValue ? Score.Value * Coefficient.Value : null;
    }

    // Класс для критериальных результатов
    public class CriterionResult
    {
        public string CriterionName { get; set; }
        public string SelectedOption { get; set; }
        public string TextAnswer { get; set; }
    }

    // Класс строки таблицы дисциплин
    public class DisciplineTableRow : INotifyPropertyChanged
    {
        public string DisciplineName { get; set; }
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

    // Класс строки таблицы критериев
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
}