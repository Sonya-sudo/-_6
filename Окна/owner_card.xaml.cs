using System;
using System.Collections.Generic;
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
    public partial class owner_card : Window
    {
        private Owner _currentOwner;
        private Клуб6Context _context;
        private bool _isNewOwner;

        public owner_card(Owner owner = null, Клуб6Context context = null)
        {
            InitializeComponent();
            _context = context ?? new Клуб6Context();

            if (owner == null)
            {
                _currentOwner = new Owner();
                _isNewOwner = true;
                Title = "Создание нового владельца";
            }
            else
            {
                _currentOwner = _context.Owners.Find(owner.OwnerId);
                _isNewOwner = false;
                Title = "Редактирование карточки владельца";

                LoadDogsInfo();
            }

            LoadData();
        }

        private void LoadData()
        {
            txtLastName.Text = _currentOwner.LastName ?? "";
            txtFirstName.Text = _currentOwner.FirstName ?? "";
            txtMiddleName.Text = _currentOwner.MiddleName ?? "";
            txtEmail.Text = _currentOwner.Email ?? "";
            txtCity.Text = _currentOwner.City ?? "";
            txtAddress.Text = _currentOwner.Address ?? "";
            txtPhone.Text = _currentOwner.Phone ?? "";

            if (_currentOwner.BirthDate.HasValue)
            {
                dpBirthDate.SelectedDate = _currentOwner.BirthDate.Value.ToDateTime(TimeOnly.MinValue);
            }

            LoadOwnerDogs();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtLastName.Text))
                {
                    MessageBox.Show("Введите фамилию владельца!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtLastName.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtFirstName.Text))
                {
                    MessageBox.Show("Введите имя владельца!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtFirstName.Focus();
                    return;
                }

                _currentOwner.LastName = txtLastName.Text.Trim();
                _currentOwner.FirstName = txtFirstName.Text.Trim();
                _currentOwner.MiddleName = txtMiddleName.Text.Trim();
                _currentOwner.Email = txtEmail.Text.Trim();
                _currentOwner.City = txtCity.Text.Trim();
                _currentOwner.Address = txtAddress.Text.Trim();
                _currentOwner.Phone = txtPhone.Text.Trim();

                if (dpBirthDate.SelectedDate.HasValue)
                {
                    _currentOwner.BirthDate = DateOnly.FromDateTime(dpBirthDate.SelectedDate.Value);
                }
                else
                {
                    _currentOwner.BirthDate = null;
                }

                if (_isNewOwner)
                {
                    _context.Owners.Add(_currentOwner);
                }
                else
                {
                    _context.Owners.Update(_currentOwner);
                }

                _context.SaveChanges();

                MessageBox.Show("Данные владельца успешно сохранены!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LstOwnerDogs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedDog = lstOwnerDogs.SelectedItem as Dog;

            if (selectedDog != null)
            {
                var dogCardWindow = new dog_card(selectedDog, _context);
                dogCardWindow.Owner = this;
                dogCardWindow.Closed += (s, args) =>
                {
                    if (!_isNewOwner)
                    {
                        LoadOwnerDogs();
                    }
                };

                dogCardWindow.Show();
            }
        }

        private void LoadDogsInfo()
        {
            try
            {
                var ownerRelations = _context.DogOwners
                    .Where(o => o.OwnerId == _currentOwner.OwnerId)
                    .ToList();

                var dogNames = new List<string>();

                foreach (var relation in ownerRelations)
                {
                    var dog = _context.Dogs.Find(relation.ChipNumber);
                    if (dog != null)
                    {
                        dogNames.Add(dog.DogName);
                    }
                }

                txtDogs.Text = string.Join(", ", dogNames);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке собак: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadOwnerDogs()
        {
            try
            {
                var ownerRelations = _context.DogOwners
                    .Where(o => o.OwnerId == _currentOwner.OwnerId)
                    .ToList();

                var dogList = new List<Dog>();
                var dogNames = new List<string>();

                foreach (var relation in ownerRelations)
                {
                    var dog = _context.Dogs.Find(relation.ChipNumber);
                    if (dog != null)
                    {
                        dogList.Add(dog);
                        dogNames.Add(dog.DogName);
                    }
                }

                lstOwnerDogs.ItemsSource = dogList;
                txtDogs.Text = string.Join(", ", dogNames);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке собак: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}