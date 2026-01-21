using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Клуб_6.Models;

namespace Клуб_6.Окна
{
    public partial class ChooseEvent : Window
    {
        private Клуб6Context _context;

        public ChooseEvent()
        {
            InitializeComponent();
            _context = new Клуб6Context();
            LoadEventTypes();
        }

        private void LoadEventTypes()
        {
            try
            {
                var eventTypes = _context.EventCompositions
                    .OrderBy(et => et.Title)
                    .Select(et => new
                    {
                        CompositionId = et.CompositionId,
                        Title = et.Title
                    })
                    .ToList();

                BoxEvents.ItemsSource = eventTypes;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке типов мероприятий: {ex.Message}", "Ошибка");
            }
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            if (BoxEvents.SelectedItem == null)
            {
                MessageBox.Show("Выберите тип мероприятия из списка!", "Внимание");
                return;
            }

            dynamic selectedItem = BoxEvents.SelectedItem;
            int eventTypeId = selectedItem.CompositionId;
            string eventTypeName = selectedItem.Title;

            var chooseDogWindow = new ChooseDog(eventTypeId, eventTypeName);
            chooseDogWindow.Show();
            this.Close();
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            var createEventTypeWindow = new Registration_performance();
            createEventTypeWindow.Show();
            this.Close();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            Window newWindow = new Schedule();
            newWindow.Show();
            this.Close();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void BoxEvents_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            btnSelect_Click(sender, e);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (BoxEvents.SelectedItem == null)
            {
                MessageBox.Show("Выберите тип мероприятия для удаления из списка!", "Внимание");
                return;
            }

            dynamic selectedItem = BoxEvents.SelectedItem;
            int compositionId = selectedItem.CompositionId;
            string compositionTitle = selectedItem.Title;

            var isUsed = _context.Events.Any(ev => ev.CompositionId == compositionId);

            if (isUsed)
            {
                MessageBox.Show($"Тип мероприятия '{compositionTitle}' нельзя удалить!\n" +
                               "Он уже используется в созданных мероприятиях.", "Ошибка");
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                $"УДАЛИТЬ ТИП МЕРОПРИЯТИЯ?\n\n" +
                $"'{compositionTitle}'\n\n" +
                "ЭТО ДЕЙСТВИЕ НЕЛЬЗЯ ОТМЕНИТЬ!",
                "ПОДТВЕРЖДЕНИЕ УДАЛЕНИЯ",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var composition = _context.EventCompositions
                        .FirstOrDefault(ec => ec.CompositionId == compositionId);

                    if (composition != null)
                    {
                        // Сначала удаляем связанные данные
                        var disciplines = _context.Disciplines
                            .Where(d => d.CompositionID == compositionId)
                            .ToList();

                        var criteria = _context.Criteria
                            .Where(c => c.CompositionID == compositionId)
                            .Include(c => c.Options)
                            .ToList();

                        var options = criteria
                            .SelectMany(c => c.Options)
                            .ToList();

                        _context.Options.RemoveRange(options);
                        _context.Criteria.RemoveRange(criteria);
                        _context.Disciplines.RemoveRange(disciplines);

                        // Теперь удаляем сам шаблон
                        _context.EventCompositions.Remove(composition);
                        _context.SaveChanges();

                        MessageBox.Show($"Тип мероприятия '{compositionTitle}' успешно удален!", "Успех");
                        LoadEventTypes();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка");
                }
            }
        }
    }
}