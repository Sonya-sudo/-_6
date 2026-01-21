using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Клуб_6.Models;
using Microsoft.EntityFrameworkCore;

namespace Клуб_6.Окна
{
    public partial class Schedule : Window
    {
        private Клуб6Context _context;
        public ObservableCollection<Event> EventList { get; set; }
        private Event selectedEvent;

        public Schedule()
        {
            InitializeComponent();
            _context = new Клуб6Context();
            LoadEvents();
            DataContext = this;
            UpdateButtonsState(false);
        }

        private void LoadEvents()
        {
            try
            {
                var events = _context.Events
                    .Include(e => e.Status)
                    .Include(e => e.Composition)
                    .Where(e => e.Status.StatusName == "In Progress")
                    .OrderByDescending(e => e.EventId)
                    .ToList();

                EventList = new ObservableCollection<Event>(events);
                BoxEvent.ItemsSource = EventList;

                if (!events.Any())
                {
                    var всеСобытия = _context.Events
                        .Include(e => e.Status)
                        .ToList();
                }

                var selectedDog = lstParticipants.SelectedItem as DogList;
                if (selectedDog != null)
                {
                    // Используем связанную таблицу Dog для получения Breed
                    var dogInfo = _context.Dogs
                        .FirstOrDefault(d => d.ChipNumber == selectedDog.DogId);

                    if (dogInfo != null)
                    {
                        string info = $"{selectedDog.DogName} ({dogInfo.Breed}) - №{selectedDog.ParticipantNumber}";
                        MessageBox.Show($"Выбран: {info}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки мероприятий: {ex.Message}", "Ошибка");
            }
        }

        private void BoxEvent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BoxEvent.SelectedItem is Event событие)
            {
                selectedEvent = событие;
                LoadEventData(событие);
                UpdateButtonsState(true);
            }
            else
            {
                ClearForm();
                UpdateButtonsState(false);
                cardTitle.Text = "КАРТОЧКА СОБЫТИЯ";
            }
        }

        private void LoadEventData(Event событие)
        {
            try
            {
                var полноеСобытие = _context.Events
                    .Include(e => e.Status)
                    .Include(e => e.Composition)
                    .FirstOrDefault(e => e.EventId == событие.EventId);

                if (полноеСобытие == null) return;

                txtName.Text = полноеСобытие.Composition?.Title ?? "";
                dpDate.SelectedDate = полноеСобытие.EventDate.ToDateTime(TimeOnly.MinValue);
                txtLocation.Text = полноеСобытие.EventVenue ?? "";
                txtJudge1.Text = полноеСобытие.Judge1 ?? "";
                txtJudge2.Text = полноеСобытие.Judge2 ?? "";
                txtOrganization.Text = полноеСобытие.Organization ?? "";
                txtCommitteeChairman.Text = полноеСобытие.CommitteeChairman ?? "";
                txtHost.Text = полноеСобытие.Host ?? "";
                txtTestOrganizer.Text = полноеСобытие.TestOrganizer ?? "";

                txtStatus.Text = GetRussianStatusName(полноеСобытие.Status?.StatusName);

                // ЗАГРУЗИТЬ УЧАСТНИКОВ (СОБАК) ЭТОГО МЕРОПРИЯТИЯ
                LoadParticipants(полноеСобытие.EventId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка");
            }
        }

        private void LoadParticipants(int eventId)
        {
            try
            {
                // Очищаем текущий список
                lstParticipants.Items.Clear();

                // Загружаем собак для этого мероприятия
                var participants = _context.DogLists
                    .Include(dl => dl.Dog) // Важно: включаем связанную таблицу Dog
                    .Where(dl => dl.EventId == eventId)
                    .OrderBy(dl => dl.DogName)
                    .ToList();

                // Добавляем участников в ListView
                foreach (var participant in participants)
                {
                    lstParticipants.Items.Add(participant);
                }

                // Если участников нет, показываем сообщение
                if (!participants.Any())
                {
                    lstParticipants.Items.Add(new { DogName = "Нет участников", Breed = "", ParticipantNumber = "" });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки участников: {ex.Message}", "Ошибка");
                lstParticipants.Items.Add(new { DogName = "Ошибка загрузки", Breed = "", ParticipantNumber = "" });
            }
        }

        private string GetRussianStatusName(string englishStatus)
        {
            return englishStatus switch
            {
                "In Progress" => "В процессе",
                "Completed" => "Завершено",
                _ => englishStatus ?? "Неизвестно"
            };
        }

        private void UpdateButtonsState(bool isEventSelected)
        {
            btnSave.IsEnabled = isEventSelected;
            btnDelete.IsEnabled = isEventSelected;
            btnResult.IsEnabled = isEventSelected;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedEvent == null) return;

            try
            {
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Введите название мероприятия!", "Ошибка");
                    return;
                }

                var событиеДляОбновления = _context.Events
                    .Include(e => e.Composition)
                    .FirstOrDefault(e => e.EventId == selectedEvent.EventId);


                if (событиеДляОбновления == null) return;

                if (dpDate.SelectedDate.HasValue)
                {
                    событиеДляОбновления.EventDate = DateOnly.FromDateTime(dpDate.SelectedDate.Value);
                }

                // Используем правильное имя свойства

                событиеДляОбновления.Composition.Title = txtName.Text.Trim();
                событиеДляОбновления.EventVenue = txtLocation.Text.Trim();
                событиеДляОбновления.Judge1 = txtJudge1.Text.Trim();
                событиеДляОбновления.Judge2 = txtJudge2.Text.Trim();
                событиеДляОбновления.Organization = txtOrganization.Text.Trim();
                событиеДляОбновления.CommitteeChairman = txtCommitteeChairman.Text.Trim();
                событиеДляОбновления.Host = txtHost.Text.Trim();
                событиеДляОбновления.TestOrganizer = txtTestOrganizer.Text.Trim();

                _context.SaveChanges();

                LoadEvents();
                MessageBox.Show("Данные обновлены!", "Успех");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка");
            }
        }

        private void ClearForm()
        {
            txtName.Text = "";
            txtLocation.Text = "";
            txtJudge1.Text = "";
            txtJudge2.Text = "";
            txtOrganization.Text = "";
            txtCommitteeChairman.Text = "";
            txtHost.Text = "";
            txtTestOrganizer.Text = "";
            dpDate.SelectedDate = null;
            txtStatus.Text = "";
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (BoxEvent.SelectedItem is Event событие)
            {

                MessageBoxResult result = MessageBox.Show(
                    $"УДАЛИТЬ СОБЫТИЕ?\n\n" +
                    $"Дата: {событие.EventDate:dd.MM.yyyy}\n",
                    "ПОДТВЕРЖДЕНИЕ УДАЛЕНИЯ",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning,
                    MessageBoxResult.No);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // 1. Получаем все DogList записи для этого мероприятия
                        var dogLists = _context.DogLists
                            .Where(dl => dl.EventId == событие.EventId)
                            .ToList();

                        foreach (var dogList in dogLists)
                        {
                            var recordId = dogList.RecordId;

                            // 1.1. Удаляем связи DogCriteriaResults через разводную таблицу
                            var criteriaLinks = _context.DogCriteriaResultsDogLists
                                .Where(link => link.RecordId == recordId)
                                .ToList();


                            // Удаляем сами связи из разводной таблицы
                            _context.DogCriteriaResultsDogLists.RemoveRange(criteriaLinks);

                            // 1.2. Удаляем DogDisciplines
                            var dogDisciplines = _context.DogDisciplines
                                .Where(dd => dd.RecordId == recordId)
                                .ToList();
                            _context.DogDisciplines.RemoveRange(dogDisciplines);

                            // 1.3. Удаляем DogList запись
                            _context.DogLists.Remove(dogList);
                        }

                        // 2. Удаляем само мероприятие
                        _context.Events.Remove(событие);
                        _context.SaveChanges();

                        // 3. Обновляем UI
                        EventList.Remove(событие);
                        ClearForm();
                        UpdateButtonsState(false);

                        MessageBox.Show($"Событие удалено!", "Успех");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка");
                    }
                }
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var chooseEventWindow = new ChooseEvent();
            chooseEventWindow.Show();
            this.Close();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            var newWindow = new calendar_of_events();
            newWindow.Show();
            this.Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ResultButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedEvent != null)
            {
                var resultWindow = new Result_results(selectedEvent.EventId);
                resultWindow.Show();
                this.Close();
            }
        }
    }
}