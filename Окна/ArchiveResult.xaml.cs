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
    public partial class ArchiveResult : Window, INotifyPropertyChanged
    {
        private КлубContext _context;
        private ArchiveEventViewModel _selectedEvent;

        public ObservableCollection<ArchiveEventViewModel> ArchiveEvents { get; set; }

        public ArchiveEventViewModel SelectedEvent
        {
            get => _selectedEvent;
            set
            {
                _selectedEvent = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ArchiveResult()
        {
            InitializeComponent();
            _context = new КлубContext(); // Исправлено на Клуб6Context
            ArchiveEvents = new ObservableCollection<ArchiveEventViewModel>();

            LoadArchiveData();
            DataContext = this;
        }

        private void LoadArchiveData()
        {
            try
            {
                ArchiveEvents.Clear();

                // Загружаем завершенные мероприятия (статус "Completed" или аналогичный)
                var completedEvents = _context.Event
                    .Include(e => e.Status)
                    .Include(e => e.Composition)
                    .Where(e => e.Status.StatusName == "Завершено" ||
                                e.Status.StatusName == "Completed" ||
                                e.Status.StatusName.Contains("заверш") ||
                                e.StatusId == 2) // ID статуса "Завершено" (проверь в БД)
                    .OrderByDescending(e => e.EventDate)
                    .ToList();

                foreach (var eventItem in completedEvents)
                {
                    // Считаем количество собак в мероприятии через DogList
                    int dogsCount = _context.DogList
                        .Count(dl => dl.EventId == eventItem.EventId);

                    var archiveEvent = new ArchiveEventViewModel
                    {
                        EventId = eventItem.EventId,
                        EventDate = eventItem.EventDate,
                        EventVenue = eventItem.EventVenue ?? "Не указано",
                        DogsCount = dogsCount,
                        CompositionTitle = eventItem.Composition?.Title ?? "Не указано",
                        StatusName = eventItem.Status?.StatusName ?? "Неизвестно"
                    };

                    ArchiveEvents.Add(archiveEvent);
                }

                if (!ArchiveEvents.Any())
                {
                    MessageBox.Show("В архиве пока нет завершенных мероприятий.", "Информация");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки архива: {ex.Message}", "Ошибка");
            }
        }

        private void btnView_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedEvent != null)
            {
                try
                {
                    // Открываем карточку результатов мероприятия
                    var cardWindow = new Result_card(SelectedEvent.EventId);
                    cardWindow.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка открытия карточки результатов: {ex.Message}", "Ошибка");
                }
            }
            else
            {
                MessageBox.Show("Выберите мероприятие из списка", "Информация");
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            new Schedule().Show();
            this.Close();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ArchiveList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            btnView_Click(sender, e);
        }
    }

    // ViewModel для отображения архивных мероприятий
    public class ArchiveEventViewModel : INotifyPropertyChanged
    {
        private int _eventId;
        public int EventId
        {
            get => _eventId;
            set
            {
                _eventId = value;
                OnPropertyChanged();
            }
        }

        private string _eventName;
        public string EventName
        {
            get => _eventName;
            set
            {
                _eventName = value;
                OnPropertyChanged();
            }
        }

        private DateOnly _eventDate;
        public DateOnly EventDate
        {
            get => _eventDate;
            set
            {
                _eventDate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FormattedDate));
            }
        }

        public string FormattedDate => EventDate.ToString("dd.MM.yyyy");

        private string _eventVenue;
        public string EventVenue
        {
            get => _eventVenue;
            set
            {
                _eventVenue = value;
                OnPropertyChanged();
            }
        }

        private int _dogsCount;
        public int DogsCount
        {
            get => _dogsCount;
            set
            {
                _dogsCount = value;
                OnPropertyChanged();
            }
        }

        private string _compositionTitle;
        public string CompositionTitle
        {
            get => _compositionTitle;
            set
            {
                _compositionTitle = value;
                OnPropertyChanged();
            }
        }

        private string _statusName;
        public string StatusName
        {
            get => _statusName;
            set
            {
                _statusName = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}