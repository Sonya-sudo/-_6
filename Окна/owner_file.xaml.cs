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
    public partial class owner_file : Window
    {
        private КлубContext _context;
        public ObservableCollection<object> OwnerList { get; set; }

        public owner_file()
        {
            InitializeComponent();
            _context = new КлубContext();
            LoadOwners();
            DataContext = this;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (BoxOwner.SelectedItem == null)
            {
                MessageBox.Show("Выберите владельца для удаления!", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("Вы уверены, что хотите удалить выбранного владельца?\n" +
                                       "Все связанные данные (связи с собаками) также будут удалены!",
                                       "Подтверждение удаления",
                                       MessageBoxButton.YesNo,
                                       MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    dynamic selectedItem = BoxOwner.SelectedItem;
                    Owner selectedOwner = selectedItem.Owner;

                    if (selectedOwner == null)
                    {
                        MessageBox.Show("Не удалось получить данные владельца", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var dogRelations = _context.DogOwner
                        .Where(r => r.OwnerId == selectedOwner.OwnerId)
                        .ToList();

                    if (dogRelations.Any())
                    {
                        var confirmResult = MessageBox.Show($"У этого владельца есть {dogRelations.Count} собак(а/и).\n" +
                                                          "Все связи будут удалены, но собаки останутся в базе.\n" +
                                                          "Продолжить удаление?",
                                                          "Подтверждение",
                                                          MessageBoxButton.YesNo,
                                                          MessageBoxImage.Question);

                        if (confirmResult != MessageBoxResult.Yes)
                        {
                            return;
                        }
                    }

                    _context.DogOwner.RemoveRange(dogRelations);
                    _context.Owner.Remove(selectedOwner);
                    _context.SaveChanges();
                    LoadOwners();

                    MessageBox.Show("Владелец успешно удален!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LoadOwners()
        {
            try
            {
                var owners = _context.Owner
                    .OrderBy(o => o.LastName)
                    .ThenBy(o => o.FirstName)
                    .ToList();

                var ownersWithDogs = owners.Select(owner => new
                {
                    LastName = owner.LastName,
                    FirstName = owner.FirstName,
                    MiddleName = owner.MiddleName,
                    Phone = owner.Phone,
                    Owner = owner,
                    DogsNames = string.Join(", ",
                        _context.DogOwner
                            .Where(r => r.OwnerId == owner.OwnerId)
                            .Join(_context.Dog,
                                r => r.ChipNumber,
                                d => d.ChipNumber,
                                (r, d) => d.DogName)
                            .ToList())
                }).ToList();

                OwnerList = new ObservableCollection<dynamic>(ownersWithDogs);
                BoxOwner.ItemsSource = OwnerList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке владельцев: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BoxEvents_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            btnChange_Click(sender, e);
        }
        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            if (BoxOwner.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите владельца из списка!", "Внимание",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            dynamic selectedItem = BoxOwner.SelectedItem;
            var selectedOwner = (Owner)selectedItem.Owner;

            var cardWindow = new owner_card(selectedOwner, _context);

            cardWindow.Closed += (s, args) =>
            {
                LoadOwners();
            };

            cardWindow.Show();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            Window newWindow = new register_of_owners();
            newWindow.Show();
            this.Close();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}