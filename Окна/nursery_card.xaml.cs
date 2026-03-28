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
    public partial class nursery_card : Window
    {
        private Kennel _currentNursery;
        private КлубContext _context;
        private bool _isNewNursery;

        public nursery_card(Kennel nursery = null, КлубContext context = null)
        {
            InitializeComponent();
            _context = context ?? new КлубContext();

            if (nursery == null)
            {
                _currentNursery = new Kennel();
                _isNewNursery = true;
                Title = "Создание нового питомника";
            }
            else
            {
                _currentNursery = _context.Kennel.Find(nursery.KennelId);
                _isNewNursery = false;
                Title = "Редактирование карточки питомника";
            }

            LoadData();
        }

        private void LoadData()
        {
            txtName.Text = _currentNursery.KennelName ?? "";
            txtEmail.Text = _currentNursery.Email ?? "";
            txtCountry.Text = _currentNursery.Country ?? "";
            txtCity.Text = _currentNursery.City ?? "";
            txtAddress.Text = _currentNursery.Address ?? "";
            txtPhone.Text = _currentNursery.Phone ?? "";

            if (_currentNursery.FoundationDate.HasValue)
            {
                dpFoundationDate.SelectedDate = _currentNursery.FoundationDate.Value.ToDateTime(TimeOnly.MinValue);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Введите название питомника!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtName.Focus();
                    return;
                }

                _currentNursery.KennelName = txtName.Text.Trim();
                _currentNursery.Email = txtEmail.Text.Trim();
                _currentNursery.Country = txtCountry.Text.Trim();
                _currentNursery.City = txtCity.Text.Trim();
                _currentNursery.Address = txtAddress.Text.Trim();
                _currentNursery.Phone = txtPhone.Text.Trim();

                if (dpFoundationDate.SelectedDate.HasValue)
                {
                    _currentNursery.FoundationDate = DateOnly.FromDateTime(dpFoundationDate.SelectedDate.Value);
                }
                else
                {
                    _currentNursery.FoundationDate = null;
                }

                if (_isNewNursery)
                {
                    _context.Kennel.Add(_currentNursery);
                }
                else
                {
                    _context.Kennel.Update(_currentNursery);
                }

                _context.SaveChanges();

                MessageBox.Show("Данные питомника успешно сохранены!", "Успех",
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
    }
}