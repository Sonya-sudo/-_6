using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Клуб_6.Models;
using Microsoft.EntityFrameworkCore;
using System.Windows.Media;

namespace Клуб_6.Окна
{
    public partial class Registration_performance : Window
    {
        private Клуб6Context _context;
        private EventComposition _composition;
        private bool _isEditMode;

        public ObservableCollection<Criterion> CriteriaList { get; set; }
        public ObservableCollection<Discipline> DisciplinesList { get; set; }

        public Registration_performance(int? compositionId = null)
        {
            InitializeComponent();
            _context = new Клуб6Context();

            CriteriaList = new ObservableCollection<Criterion>();
            DisciplinesList = new ObservableCollection<Discipline>();

            if (compositionId.HasValue)
            {
                _isEditMode = true;
                LoadExistingComposition(compositionId.Value);
                txtTitle.Text = "РЕДАКТИРОВАНИЕ ШАБЛОНА";
            }
            else
            {
                _isEditMode = false;
                _composition = new EventComposition();
            }

            this.DataContext = this;
        }

        private void RefreshItemsControl(Button button)
        {
            // Находим ItemsControl
            var itemsControl = FindVisualParent<ItemsControl>(button);
            if (itemsControl != null)
            {
                itemsControl.Items.Refresh();
            }
        }

        private void LoadExistingComposition(int compositionId)
        {
            _composition = _context.EventCompositions
                .FirstOrDefault(c => c.CompositionId == compositionId);

            if (_composition != null)
            {
                txtEventName.Text = _composition.Title ?? "";
                LoadTemplateData(compositionId);
            }
        }

        private void LoadTemplateData(int compositionId)
        {
            DisciplinesList.Clear();
            var disciplines = _context.Disciplines
                .Where(d => d.CompositionID == compositionId)
                .ToList();

            foreach (var discipline in disciplines)
            {
                DisciplinesList.Add(discipline);
            }

            CriteriaList.Clear();
            var criteria = _context.Criteria
                .Where(c => c.CompositionID == compositionId)
                .Include(c => c.Options)
                .ToList();

            foreach (var criterion in criteria)
            {
                if (criterion.Options == null)
                {
                    criterion.Options = new ObservableCollection<Option>();
                }
                CriteriaList.Add(criterion);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtEventName.Text))
                {
                    MessageBox.Show("Введите название шаблона!");
                    return;
                }

                if (!DisciplinesList.Any())
                {
                    MessageBox.Show("Добавьте хотя бы одну дисциплину!");
                    return;
                }

                if (!CriteriaList.Any())
                {
                    MessageBox.Show("Добавьте хотя бы один критерий!");
                    return;
                }

                using (var saveContext = new Клуб6Context())
                {
                    if (!_isEditMode)
                    {
                        CreateNewTemplate(saveContext);
                    }
                    else
                    {
                        UpdateExistingTemplate(saveContext);
                    }

                    MessageBox.Show($"Шаблон '{txtEventName.Text}' успешно сохранен!");
                    var choice = new ChooseEvent();
                    choice.Show();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                // Получаем самую глубокую ошибку
                Exception deepestException = ex;
                while (deepestException.InnerException != null)
                {
                    deepestException = deepestException.InnerException;
                }

                string errorDetails = $"Ошибка: {deepestException.Message}\n\n";

                // Если это SQL ошибка, покажем подробности
                if (deepestException is Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
                {
                    errorDetails += $"DbUpdateException: {dbEx.Message}\n";
                }
                else if (deepestException is Microsoft.Data.SqlClient.SqlException sqlEx) // ← ИЗМЕНИТЕ ЗДЕСЬ
                {
                    errorDetails += $"SQL Error #{sqlEx.Number}: {sqlEx.Message}\n";
                    foreach (Microsoft.Data.SqlClient.SqlError err in sqlEx.Errors) // ← И ЗДЕСЬ
                    {
                        errorDetails += $"  Procedure: {err.Procedure}, Line: {err.LineNumber}\n";
                    }
                }

                MessageBox.Show(errorDetails, "Ошибка сохранения",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                // Также выведите в консоль для отладки
                System.Diagnostics.Debug.WriteLine($"FULL ERROR: {ex}");
            }
        }

        private void CreateNewTemplate(Клуб6Context context)
        {
            // 1. Сохраняем EventComposition
            _composition = new EventComposition
            {
                Title = txtEventName.Text.Trim()
            };
            context.EventCompositions.Add(_composition);
            context.SaveChanges();

            int compositionId = _composition.CompositionId;

            // 2. Сохраняем Discipline
            foreach (var discipline in DisciplinesList)
            {
                var newDiscipline = new Discipline
                {
                    DisciplineName = discipline.DisciplineName,
                    WorkingScore = discipline.WorkingScore,
                    Coefficient = discipline.Coefficient,
                    CompositionID = compositionId
                };
                context.Disciplines.Add(newDiscipline);
            }
            context.SaveChanges();

            // 3. Сохраняем Criteria и Options
            foreach (var criterion in CriteriaList)
            {
                var newCriterion = new Criterion
                {
                    CriterionName = criterion.CriterionName,
                    IsImportant = criterion.IsImportant,
                    CompositionID = compositionId
                };
                context.Criteria.Add(newCriterion);
                context.SaveChanges();

                int criterionId = newCriterion.CriterionID;

                foreach (var option in criterion.Options)
                {
                    var newOption = new Option
                    {
                        OptionType = option.OptionType,
                        OptionValue = option.OptionType == "radio" ? option.OptionValue : null,
                        TextInfo = option.OptionType == "text" ? option.TextInfo : null,
                        CompositionID = compositionId,
                        CriterionID = criterionId
                    };
                    context.Options.Add(newOption);
                }
                context.SaveChanges();
            }
        }

        private void UpdateExistingTemplate(Клуб6Context context)
        {
            var composition = context.EventCompositions
                .FirstOrDefault(c => c.CompositionId == _composition.CompositionId);

            if (composition == null) return;

            composition.Title = txtEventName.Text.Trim();
            context.SaveChanges();

            int compositionId = composition.CompositionId;

            // Удаляем старые записи
            var oldDisciplines = context.Disciplines
                .Where(d => d.CompositionID == compositionId)
                .ToList();
            context.Disciplines.RemoveRange(oldDisciplines);

            var oldCriteria = context.Criteria
                .Where(c => c.CompositionID == compositionId)
                .ToList();
            context.Criteria.RemoveRange(oldCriteria);

            var oldOptions = context.Options
                .Where(o => o.CompositionID == compositionId)
                .ToList();
            context.Options.RemoveRange(oldOptions);

            context.SaveChanges();

            // Добавляем новые Discipline
            foreach (var discipline in DisciplinesList)
            {
                var newDiscipline = new Discipline
                {
                    DisciplineName = discipline.DisciplineName,
                    WorkingScore = discipline.WorkingScore,
                    Coefficient = discipline.Coefficient,
                    CompositionID = compositionId
                };
                context.Disciplines.Add(newDiscipline);
            }
            context.SaveChanges();

            // Добавляем новые Criteria и Options
            foreach (var criterion in CriteriaList)
            {
                var newCriterion = new Criterion
                {
                    CriterionName = criterion.CriterionName,
                    IsImportant = criterion.IsImportant,
                    CompositionID = compositionId
                };
                context.Criteria.Add(newCriterion);
                context.SaveChanges();

                int criterionId = newCriterion.CriterionID;

                foreach (var option in criterion.Options)
                {
                    var newOption = new Option
                    {
                        OptionType = option.OptionType,
                        OptionValue = option.OptionType == "radio" ? option.OptionValue : null,
                        TextInfo = option.OptionType == "text" ? option.TextInfo : null,
                        CompositionID = compositionId,
                        CriterionID = criterionId
                    };
                    context.Options.Add(newOption);
                }
                context.SaveChanges();
            }
        }

        private void btnAddCriterion_Click(object sender, RoutedEventArgs e)
        {
            CriteriaList.Add(new Criterion
            {
                CriterionName = "Новый критерий",
                IsImportant = false,
                Options = new ObservableCollection<Option>()
            });
        }

        // Универсальный метод добавления варианта (работает для radio и text)
        private void AddOption_Click(object sender, RoutedEventArgs e, string optionType)
        {
            var button = sender as Button;
            if (button == null) return;

            // Ищем TextBox в родительском StackPanel
            var stackPanel = button.Parent as StackPanel;
            if (stackPanel == null) return;

            var textBox = stackPanel.Children.OfType<TextBox>().FirstOrDefault();
            if (textBox == null || string.IsNullOrWhiteSpace(textBox.Text))
            {
                MessageBox.Show(optionType == "radio" ? "Введите текст варианта!" : "Введите текст вопроса!");
                return;
            }

            // Получаем критерий из Tag кнопки
            var criterion = button.Tag as Criterion;
            if (criterion == null) return;

            if (criterion.Options == null)
            {
                criterion.Options = new ObservableCollection<Option>();
            }

            string textValue = textBox.Text.Trim();

            // Проверка на дубликаты
            if (optionType == "radio")
            {
                if (criterion.Options.Any(o => o.OptionValue?.Trim() == textValue && o.OptionType == "radio"))
                {
                    MessageBox.Show("Такой вариант уже существует!");
                    return;
                }
            }
            else if (optionType == "text")
            {
                if (criterion.Options.Any(o => o.TextInfo?.Trim() == textValue && o.OptionType == "text"))
                {
                    MessageBox.Show("Такой вопрос уже существует!");
                    return;
                }
            }

            // Добавляем вариант
            var newOption = new Option
            {
                OptionType = optionType
            };

            if (optionType == "radio")
            {
                newOption.OptionValue = textValue;
                newOption.TextInfo = null;
            }
            else if (optionType == "text")
            {
                newOption.TextInfo = textValue;
                newOption.OptionValue = null;
            }

            criterion.Options.Add(newOption);
            textBox.Text = "";
        }

        // Добавление радио-варианта
        private void btnAddRadioOption_Click(object sender, RoutedEventArgs e)
        {
            AddOption_Click(sender, e, "radio");
            RefreshItemsControl(sender as Button);
        }

        private static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(child);
            while (parent != null && !(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as T;
        }

        // Добавление текстового поля
        private void btnAddTextOption_Click(object sender, RoutedEventArgs e)
        {
            AddOption_Click(sender, e, "text");
            RefreshItemsControl(sender as Button);
        }

        // Удаление варианта (универсальный метод)
        private void btnDeleteOption_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            // Получаем вариант
            var option = button.DataContext as Option;
            if (option == null) return;

            // Ищем родительский ItemsControl
            var itemsControl = FindParent<ItemsControl>(button);
            if (itemsControl == null) return;

            // Получаем критерий из DataContext ItemsControl
            var criterion = itemsControl.DataContext as Criterion;
            if (criterion != null)
            {
                // Удаляем вариант
                criterion.Options.Remove(option);

                // Принудительно обновляем ItemsControl
                itemsControl.Items.Refresh();

                // Также обновляем родительский ItemsControl (если есть)
                var parentItemsControl = FindParent<ItemsControl>(itemsControl);
                if (parentItemsControl != null)
                {
                    parentItemsControl.Items.Refresh();
                }
            }
        }

        private void btnDeleteCriterion_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Criterion selectedCriterion)
            {
                CriteriaList.Remove(selectedCriterion);
            }
        }

        private void btnAddDiscipline_Click(object sender, RoutedEventArgs e)
        {
            DisciplinesList.Add(new Discipline
            {
                DisciplineName = "Новая дисциплина"
            });
        }

        private void btnDeleteDiscipline_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Discipline selectedDiscipline)
            {
                DisciplinesList.Remove(selectedDiscipline);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            var choice = new ChooseEvent();
            choice.Show();
            this.Close();
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(child);
            while (parent != null && !(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as T;
        }
    }
}