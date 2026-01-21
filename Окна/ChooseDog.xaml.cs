using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Клуб_6.Models;

namespace Клуб_6.Окна
{
    public partial class ChooseDog : Window
    {
        private Клуб6Context _context;
        private int _compositionId;
        private string _compositionName;
        private int _defaultStatusId = 1;

        public ObservableCollection<SimpleDog> AvailableDogs { get; set; }
        public ObservableCollection<SimpleDog> SelectedDogs { get; set; }
        private List<SimpleDog> _allDogs;

        public class SimpleDog
        {
            public int ChipNumber { get; set; }
            public string DogName { get; set; }
            public string Breed { get; set; }
            public string OwnerName { get; set; }
        }

        public ChooseDog(int compositionId, string compositionName)
        {
            InitializeComponent();

            _context = new Клуб6Context();
            _compositionId = compositionId;
            _compositionName = compositionName;

            InitializeCollections();
            LoadCompositionInfo();
            LoadEventStatuses();
            LoadAllDogs();
            RefreshAvailableDogs();

            listAvailableDogs.ItemsSource = AvailableDogs;
            listSelectedDogs.ItemsSource = SelectedDogs;

            UpdateSelectedCount();
        }

        private void InitializeCollections()
        {
            AvailableDogs = new ObservableCollection<SimpleDog>();
            SelectedDogs = new ObservableCollection<SimpleDog>();
            _allDogs = new List<SimpleDog>();
        }

        private void LoadCompositionInfo()
        {
            txtEventType.Text = _compositionName;
            txtEventTitle.Text = $"СОЗДАНИЕ НОВОГО МЕРОПРИЯТИЯ";
        }

        private void LoadEventStatuses()
        {
            try
            {
                var statuses = _context.EventStatuses.ToList();
                var defaultStatus = statuses.FirstOrDefault(s =>
                    s.StatusName.Contains("процесс") ||
                    s.StatusName.Contains("планир") ||
                    s.StatusName == "В процессе" ||
                    s.StatusName == "Планируется");

                if (defaultStatus != null)
                {
                    _defaultStatusId = defaultStatus.StatusId;
                }
                else if (statuses.Any())
                {
                    _defaultStatusId = statuses.First().StatusId;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке статусов: {ex.Message}", "Ошибка");
            }
        }

        private void LoadAllDogs()
        {
            try
            {
                _allDogs.Clear();

                // Получаем всех живых собак
                var dogs = _context.Dogs
                    .Where(d => d.IsAlive == true)
                    .OrderBy(d => d.DogName)
                    .ToList();

                foreach (var dog in dogs)
                {
                    _allDogs.Add(new SimpleDog
                    {
                        ChipNumber = dog.ChipNumber,
                        DogName = dog.DogName,
                        Breed = dog.Breed ?? "Не указана"
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке списка собак: {ex.Message}", "Ошибка");
            }
        }

        private void RefreshAvailableDogs()
        {
            try
            {
                AvailableDogs.Clear();

                var selectedChipNumbers = new HashSet<int>(SelectedDogs.Select(d => d.ChipNumber));

                var availableDogs = _allDogs
                    .Where(d => !selectedChipNumbers.Contains(d.ChipNumber))
                    .ToList();

                if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    var searchTerm = txtSearch.Text.Trim().ToLower();
                    availableDogs = availableDogs
                        .Where(d =>
                            d.DogName?.ToLower().Contains(searchTerm) == true ||
                            d.Breed?.ToLower().Contains(searchTerm) == true ||
                            d.OwnerName?.ToLower().Contains(searchTerm) == true ||
                            d.ChipNumber.ToString().Contains(searchTerm))
                        .ToList();
                }

                foreach (var dog in availableDogs)
                {
                    AvailableDogs.Add(dog);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении списка собак: {ex.Message}", "Ошибка");
            }
        }

        private void UpdateSelectedCount()
        {
            txtSelectedCount.Text = $"({SelectedDogs.Count})";
            btnMoveToAvailable.IsEnabled = SelectedDogs.Count > 0;
            btnClearSelected.IsEnabled = SelectedDogs.Count > 0;
        }

        private void btnMoveToSelected_Click(object sender, RoutedEventArgs e)
        {
            if (listAvailableDogs.SelectedItems.Count > 0)
            {
                var dogsToAdd = new List<SimpleDog>();

                foreach (SimpleDog dog in listAvailableDogs.SelectedItems)
                {
                    dogsToAdd.Add(dog);
                }

                foreach (var dog in dogsToAdd)
                {
                    SelectedDogs.Add(dog);
                }

                RefreshAvailableDogs();
                UpdateSelectedCount();
            }
        }

        private void btnMoveToAvailable_Click(object sender, RoutedEventArgs e)
        {
            if (listSelectedDogs.SelectedItems.Count > 0)
            {
                var dogsToRemove = new List<SimpleDog>();

                foreach (SimpleDog dog in listSelectedDogs.SelectedItems)
                {
                    dogsToRemove.Add(dog);
                }

                foreach (var dog in dogsToRemove)
                {
                    SelectedDogs.Remove(dog);
                }

                RefreshAvailableDogs();
                UpdateSelectedCount();
            }
        }

        private void btnClearSelected_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedDogs.Count > 0)
            {
                var result = MessageBox.Show($"Очистить все выбранные собаки ({SelectedDogs.Count} шт.)?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SelectedDogs.Clear();
                    RefreshAvailableDogs();
                    UpdateSelectedCount();
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInput())
                    return;

                var newEvent = new Event
                {
                    CompositionId = _compositionId,
                    EventDate = DateOnly.FromDateTime(dpDate.SelectedDate.Value),
                    EventVenue = txtLocation.Text.Trim(),
                    StatusId = _defaultStatusId,
                    Judge1 = txtJudge1.Text.Trim(),
                    Judge2 = txtJudge2.Text.Trim(),
                    CommitteeChairman = txtChairman.Text.Trim(),
                    Organization = txtOrganization.Text.Trim(),
                    TestOrganizer = txtOrganizer.Text.Trim(),
                    Host = txtHost.Text.Trim()
                };

                _context.Events.Add(newEvent);
                _context.SaveChanges();

                int participantNumber = 1;
                foreach (var selectedDog in SelectedDogs)
                {
                    // Находим собаку в базе
                    var dog = _context.Dogs.Find(selectedDog.ChipNumber);

                    var dogList = new DogList
                    {
                        EventId = newEvent.EventId,
                        DogId = selectedDog.ChipNumber,
                        ParticipantNumber = participantNumber.ToString("D3"),
                        DogName = dog?.DogName ?? selectedDog.DogName
                    };

                    _context.DogLists.Add(dogList);
                    participantNumber++;
                }

                _context.SaveChanges();

                MessageBox.Show(
                    $"Мероприятие успешно создано!\n\n" +
                    $"Тип: {_compositionName}\n" +
                    $"Дата: {newEvent.EventDate:dd.MM.yyyy}\n" +
                    $"Место: {newEvent.EventVenue}\n" +
                    $"Участников: {SelectedDogs.Count}",
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                var scheduleWindow = new Schedule();
                scheduleWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании мероприятия: {ex.Message}\n\n{ex.InnerException?.Message}", "Ошибка");
            }
        }

        private bool ValidateInput()
        {
            if (!dpDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите дату испытания!", "Ошибка");
                dpDate.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtLocation.Text))
            {
                MessageBox.Show("Введите место проведения!", "Ошибка");
                txtLocation.Focus();
                return false;
            }

            if (SelectedDogs.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы одну собаку к мероприятию!", "Внимание");
                return false;
            }

            return true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            var chooseEventWindow = new ChooseEvent();
            chooseEventWindow.Show();
            this.Close();
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshAvailableDogs();
        }
    }
}