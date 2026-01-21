using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Клуб_6.Models;

namespace Клуб_6.Окна
{
    public partial class nursery_file : Window
    {
        private Клуб6Context _context;
        public ObservableCollection<Kennel> NurseryList { get; set; }

        public nursery_file()
        {
            InitializeComponent();
            _context = new Клуб6Context();
            LoadNurseries();
            DataContext = this;
        }

        private void LoadNurseries()
        {
            try
            {
                var nurseries = _context.Kennels
                    .OrderBy(n => n.KennelName)
                    .ToList();

                NurseryList = new ObservableCollection<Kennel>(nurseries);
                BoxNursery.ItemsSource = NurseryList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке питомников: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            Window newWindow = new nursery_registry();
            newWindow.Show();
            this.Close();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            if (BoxNursery.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите питомник из списка!", "Внимание",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedNursery = (Kennel)BoxNursery.SelectedItem;
            var cardWindow = new nursery_card(selectedNursery, _context);

            cardWindow.Closed += (s, args) =>
            {
                LoadNurseries();
            };

            cardWindow.Show();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (BoxNursery.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите питомник из списка!", "Внимание",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedNursery = (Kennel)BoxNursery.SelectedItem;

            var result = MessageBox.Show($"Вы уверены, что хотите удалить питомник '{selectedNursery.KennelName}'?\n" +
                                       "У всех собак этого питомника поле питомника будет очищено.",
                                       "Подтверждение удаления",
                                       MessageBoxButton.YesNo,
                                       MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                var dogsInKennel = _context.Dogs
                    .Where(d => d.KennelId == selectedNursery.KennelId)
                    .ToList();

                int dogCount = dogsInKennel.Count;

                if (dogCount > 0)
                {
                    foreach (var dog in dogsInKennel)
                    {
                        dog.KennelId = null;
                    }
                    _context.SaveChanges();
                }

                var kennelToDelete = _context.Kennels.Find(selectedNursery.KennelId);
                if (kennelToDelete != null)
                {
                    _context.Kennels.Remove(kennelToDelete);
                    _context.SaveChanges();
                }

                NurseryList.Remove(selectedNursery);

                MessageBox.Show("Питомник успешно удален!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}