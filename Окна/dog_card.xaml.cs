using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Клуб_6.Models;
using Microsoft.EntityFrameworkCore;

namespace Клуб_6.Окна
{
    public partial class dog_card : Window
    {
        private Dog _currentDog;
        private Клуб6Context _context;
        private bool _isNewDog;

        // Вспомогательный класс для отображения критериев
        public class ImportantCriterionDisplay
        {
            public string CriterionName { get; set; }
            public string SelectedOption { get; set; }
            public string EventName { get; set; }
            public string EventDate { get; set; }
        }

        // Коллекция для привязки данных
        public ObservableCollection<ImportantCriterionDisplay> ImportantCriteria { get; set; }

        public class OwnerDisplay
        {
            public Owner Owner { get; set; }
            public string DisplayText => $"{Owner.LastName} {Owner.FirstName} {Owner.MiddleName}";
        }

        public class KennelDisplay
        {
            public Kennel Kennel { get; set; }
            public string DisplayText => $"{Kennel.KennelName}";
        }

        public dog_card(Dog dog = null, Клуб6Context context = null)
        {
            InitializeComponent();
            _context = context ?? new Клуб6Context();

            // Инициализируем коллекцию
            ImportantCriteria = new ObservableCollection<ImportantCriterionDisplay>();
            this.DataContext = this; // Устанавливаем DataContext для привязки

            if (dog == null)
            {
                _currentDog = new Dog
                {
                    ChipNumber = 0,
                    BirthDate = DateOnly.FromDateTime(DateTime.Now),
                    IsAlive = true
                };
                _isNewDog = true;
                Title = "Создание новой собаки";
            }
            else
            {
                _currentDog = _context.Dogs
                    .Include(d => d.Kennel)
                    .FirstOrDefault(d => d.ChipNumber == dog.ChipNumber);

                if (_currentDog == null)
                {
                    _currentDog = dog;
                }

                _isNewDog = false;
                Title = "Редактирование карточки собаки";

                // Загружаем важные критерии ТОЛЬКО для существующей собаки
                LoadImportantCriteria();
            }

            LoadOwners();
            LoadData();
            LoadKennels();
        }

        // Метод для загрузки важных критериев из DogCriteriaResults_DogList
        private void LoadImportantCriteria()
        {
            try
            {
                ImportantCriteria.Clear();

                if (_currentDog == null || _currentDog.ChipNumber <= 0)
                {
                    txtNoImportantCriteria.Visibility = Visibility.Visible;
                    return;
                }

                // 1. Находим все записи DogList для этой собаки и включаем Event с Composition
                var dogListRecords = _context.DogLists
                    .Where(dl => dl.DogId == _currentDog.ChipNumber)
                    .Include(dl => dl.Event)
                            .ThenInclude(e => e.Composition) // Добавляем загрузку Composition
                    .ToList();

                if (!dogListRecords.Any())
                {
                    txtNoImportantCriteria.Visibility = Visibility.Visible;
                    return;
                }

                // 2. Получаем ID всех записей DogList
                var recordIds = dogListRecords.Select(dl => dl.RecordId).ToList();

                // 3. Получаем все результаты критериев для этих записей
                var allResults = new List<DogCriteriaResults_DogList>();

                foreach (var recordId in recordIds)
                {
                    var results = _context.DogCriteriaResultsDogLists
                        .Where(r => r.RecordId == recordId)
                        .ToList();
                    allResults.AddRange(results);
                }

                if (!allResults.Any())
                {
                    txtNoImportantCriteria.Visibility = Visibility.Visible;
                    return;
                }

                // 4. Получаем все критерии (важные)
                var criterionIds = allResults.Select(r => r.CriterionId).Distinct().ToList();
                var importantCriteria = _context.Criteria
                    .Include(c => c.Options)
                    .Where(c => criterionIds.Contains(c.CriterionID) && c.IsImportant == true)
                    .ToList();

                if (!importantCriteria.Any())
                {
                    txtNoImportantCriteria.Visibility = Visibility.Visible;
                    return;
                }

                // 5. Связываем результаты с критериями и записями DogList
                var detailedResults = new List<CriterionResultDetail>();

                foreach (var result in allResults)
                {
                    var criterion = importantCriteria.FirstOrDefault(c => c.CriterionID == result.CriterionId);
                    if (criterion == null) continue;

                    var dogListRecord = dogListRecords.FirstOrDefault(dl => dl.RecordId == result.RecordId);
                    if (dogListRecord == null) continue;

                    // Находим выбранную опцию
                    Option selectedOption = null;
                    if (result.OptionId > 0)
                    {
                        selectedOption = criterion.Options.FirstOrDefault(o => o.OptionId == result.OptionId);
                    }

                    detailedResults.Add(new CriterionResultDetail
                    {
                        Result = result,
                        Criterion = criterion,
                        SelectedOption = selectedOption,
                        DogListRecord = dogListRecord,
                        Event = dogListRecord.Event,
                        EventComposition = dogListRecord.Event?.Composition // Добавляем Composition
                    });
                }

                // 6. Группируем по критериям (берем последнюю оценку по дате мероприятия)
                var groupedResults = detailedResults
                    .GroupBy(r => r.Criterion.CriterionID)
                    .Select(g => g.OrderByDescending(r =>
                        r.Event?.EventDate.ToDateTime(TimeOnly.MinValue) ?? DateTime.MinValue)
                    .First())
                    .ToList();

                // 7. Формируем данные для отображения
                foreach (var detail in groupedResults)
                {
                    string selectedOptionText = GetSelectedOptionText(detail);

                    // Информация о мероприятии - берем название из EventComposition
                    string eventName = detail.EventComposition?.Title ?? "Неизвестное мероприятие";
                    string eventDate = detail.Event?.EventDate.ToString("dd.MM.yyyy") ?? "";

                    ImportantCriteria.Add(new ImportantCriterionDisplay
                    {
                        CriterionName = detail.Criterion.CriterionName,
                        SelectedOption = selectedOptionText,
                        EventName = eventName,
                        EventDate = eventDate
                    });
                }

                // 8. Сортируем по названию критерия
                ImportantCriteria = new ObservableCollection<ImportantCriterionDisplay>(
                    ImportantCriteria.OrderBy(ic => ic.CriterionName));

                // 9. Обновляем ItemsControl
                importantCriteriaControl.ItemsSource = ImportantCriteria;
                importantCriteriaControl.Items.Refresh();

                // 10. Показываем/скрываем сообщение
                txtNoImportantCriteria.Visibility = ImportantCriteria.Any()
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке важных критериев: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                txtNoImportantCriteria.Visibility = Visibility.Visible;
            }
        }

        // Обновленный вспомогательный класс
        private class CriterionResultDetail
        {
            public DogCriteriaResults_DogList Result { get; set; }
            public Criterion Criterion { get; set; }
            public Option SelectedOption { get; set; }
            public DogList DogListRecord { get; set; }
            public Event Event { get; set; }
            public EventComposition EventComposition { get; set; } // Добавляем свойство для Composition
        }

        // Метод для получения текста выбранного варианта остается без изменений
        private string GetSelectedOptionText(CriterionResultDetail detail)
        {
            // Если есть выбранная опция
            if (detail.SelectedOption != null)
            {
                if (detail.SelectedOption.OptionType == "radio")
                {
                    return detail.SelectedOption.OptionValue ?? "Не указано";
                }
                else if (detail.SelectedOption.OptionType == "text")
                {
                    // Для текстовых вариантов показываем либо пользовательский ввод, либо текст варианта
                    return !string.IsNullOrEmpty(detail.Result.UserInput)
                        ? detail.Result.UserInput
                        : detail.SelectedOption.TextInfo ?? "Текстовый ответ";
                }
            }
            // Если нет выбранной опции, но есть UserInput
            else if (!string.IsNullOrEmpty(detail.Result.UserInput))
            {
                return detail.Result.UserInput;
            }

            return "Не выбран";
        }


        private void LoadOwners()
        {
            try
            {
                var owners = _context.Owners
                    .OrderBy(o => o.LastName)
                    .ThenBy(o => o.FirstName)
                    .Select(o => new OwnerDisplay { Owner = o })
                    .ToList();

                cmbOwner.ItemsSource = owners;
                cmbOwner.DisplayMemberPath = "DisplayText";
                cmbOwner.SelectedValuePath = "Owner";

                var ownerRelation = _context.DogOwners
                    .FirstOrDefault(o => o.ChipNumber == _currentDog.ChipNumber);

                if (ownerRelation != null)
                {
                    var currentOwner = owners.FirstOrDefault(o =>
                        o.Owner.OwnerId == ownerRelation.OwnerId);
                    cmbOwner.SelectedItem = currentOwner;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке владельцев: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadData()
        {
            txtName.Text = _currentDog.DogName ?? "";
            txtBreed.Text = _currentDog.Breed ?? "";
            txtColor.Text = _currentDog.Color ?? "";
            txtHeight.Text = _currentDog.HeightCm?.ToString() ?? "";
            txtWeight.Text = _currentDog.WeightKg?.ToString() ?? "";
            txtMother.Text = _currentDog.MotherName ?? "";
            txtFather.Text = _currentDog.FatherName ?? "";

            if (_currentDog.ChipNumber > 0)
            {
                txtChip.Text = _currentDog.ChipNumber.ToString();
            }
            else
            {
                txtChip.Text = "";
            }

            if (_currentDog.BirthDate != default)
            {
                dpBirthDate.SelectedDate = _currentDog.BirthDate.ToDateTime(TimeOnly.MinValue);
            }
            else
            {
                dpBirthDate.SelectedDate = DateTime.Now;
            }

            if (_currentDog.Gender == "Male")
                rbMale.IsChecked = true;
            else if (_currentDog.Gender == "Female")
                rbFemale.IsChecked = true;
            else
            {
                rbMale.IsChecked = false;
                rbFemale.IsChecked = false;
            }

            chkIsAlive.IsChecked = _currentDog.IsAlive ?? true;
        }

        private void LoadKennels()
        {
            try
            {
                var kennels = _context.Kennels
                    .OrderBy(k => k.KennelName)
                    .Select(k => new KennelDisplay { Kennel = k })
                    .ToList();

                cmbKennel.ItemsSource = kennels;
                cmbKennel.DisplayMemberPath = "DisplayText";
                cmbKennel.SelectedValuePath = "Kennel";

                if (_currentDog.KennelId.HasValue)
                {
                    var currentKennel = kennels.FirstOrDefault(k =>
                        k.Kennel.KennelId == _currentDog.KennelId.Value);
                    cmbKennel.SelectedItem = currentKennel;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке питомников: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверка обязательных полей
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Введите кличку собаки!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtName.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtBreed.Text))
                {
                    MessageBox.Show("Введите породу собаки!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtBreed.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtChip.Text))
                {
                    MessageBox.Show("Введите номер чипа!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtChip.Focus();
                    return;
                }

                if (!int.TryParse(txtChip.Text, out int chipNumber))
                {
                    MessageBox.Show("Номер чипа должен быть числом!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtChip.Focus();
                    return;
                }

                if (chipNumber <= 0)
                {
                    MessageBox.Show("Номер чипа должен быть положительным числом!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtChip.Focus();
                    return;
                }

                if (!dpBirthDate.SelectedDate.HasValue)
                {
                    MessageBox.Show("Введите дату рождения собаки!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    dpBirthDate.Focus();
                    return;
                }

                if (rbMale.IsChecked != true && rbFemale.IsChecked != true)
                {
                    MessageBox.Show("Выберите пол собаки!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверка уникальности чипа
                if (_isNewDog)
                {
                    var existingDog = _context.Dogs.Find(chipNumber);
                    if (existingDog != null)
                    {
                        MessageBox.Show("Собака с таким номером чипа уже существует!", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        txtChip.Focus();
                        return;
                    }
                }
                else if (chipNumber != _currentDog.ChipNumber)
                {
                    var existingDog = _context.Dogs.Find(chipNumber);
                    if (existingDog != null && existingDog.ChipNumber != _currentDog.ChipNumber)
                    {
                        MessageBox.Show("Собака с таким номером чипа уже существует!", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        txtChip.Focus();
                        return;
                    }
                }

                // Обновление данных собаки
                _currentDog.ChipNumber = chipNumber;
                _currentDog.DogName = txtName.Text.Trim();
                _currentDog.Breed = txtBreed.Text.Trim();
                _currentDog.Color = txtColor.Text.Trim();
                _currentDog.MotherName = txtMother.Text.Trim();
                _currentDog.FatherName = txtFather.Text.Trim();
                _currentDog.IsAlive = chkIsAlive.IsChecked ?? true;
                _currentDog.BirthDate = DateOnly.FromDateTime(dpBirthDate.SelectedDate.Value);

                if (int.TryParse(txtHeight.Text, out int height))
                    _currentDog.HeightCm = height;
                else
                    _currentDog.HeightCm = null;

                if (int.TryParse(txtWeight.Text, out int weight))
                    _currentDog.WeightKg = weight;
                else
                    _currentDog.WeightKg = null;

                if (rbMale.IsChecked == true)
                    _currentDog.Gender = "Male";
                else if (rbFemale.IsChecked == true)
                    _currentDog.Gender = "Female";

                var selectedKennel = cmbKennel.SelectedItem as KennelDisplay;
                if (selectedKennel != null)
                {
                    _currentDog.KennelId = selectedKennel.Kennel.KennelId;
                }
                else
                {
                    _currentDog.KennelId = null;
                }

                // Сохранение собаки
                if (_isNewDog)
                {
                    _context.Dogs.Add(_currentDog);
                }
                else
                {
                    _context.Dogs.Update(_currentDog);
                }

                _context.SaveChanges();

                // Обновление связи с владельцем
                var selectedOwner = cmbOwner.SelectedItem as OwnerDisplay;
                var existingRelation = _context.DogOwners
                    .FirstOrDefault(o => o.ChipNumber == _currentDog.ChipNumber);

                if (selectedOwner != null)
                {
                    if (existingRelation == null)
                    {
                        var newRelation = new DogOwner
                        {
                            ChipNumber = _currentDog.ChipNumber,
                            OwnerId = selectedOwner.Owner.OwnerId
                        };
                        _context.DogOwners.Add(newRelation);
                    }
                    else
                    {
                        existingRelation.OwnerId = selectedOwner.Owner.OwnerId;
                        _context.DogOwners.Update(existingRelation);
                    }
                }
                else
                {
                    if (existingRelation != null)
                    {
                        _context.DogOwners.Remove(existingRelation);
                    }
                }

                _context.SaveChanges();

                // Если это не новая собака, обновляем критерии
                if (!_isNewDog)
                {
                    LoadImportantCriteria();
                }

                MessageBox.Show("Данные успешно сохранены!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void txtChip_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0);
        }

        private void txtHeight_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0);
        }

        private void txtWeight_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0);
        }
    }
}