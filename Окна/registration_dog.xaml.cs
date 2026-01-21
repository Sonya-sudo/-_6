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
    public partial class registration_dog : Window
    {
        private Клуб6Context _context; 

        public registration_dog()
        {
            InitializeComponent();
            _context = new Клуб6Context();

            LoadKennels();
            LoadOwners();
            SetInitialWatermarks();
        }

        private void LoadKennels()
        {
            try
            {
                var kennels = _context.Kennels
                    .OrderBy(k => k.KennelName)
                    .ToList();

                var kennelList = new System.Collections.Generic.List<object>();
                kennelList.Add(new { KennelId = 0, KennelName = "(не выбран)" });

                foreach (var kennel in kennels)
                {
                    kennelList.Add(new
                    {
                        KennelId = kennel.KennelId,
                        KennelName = kennel.KennelName
                    });
                }

                cmbKennel.ItemsSource = kennelList;
                cmbKennel.DisplayMemberPath = "KennelName";
                cmbKennel.SelectedValuePath = "KennelId";
                cmbKennel.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке питомников: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadOwners()
        {
            try
            {
                var owners = _context.Owners
                    .OrderBy(o => o.LastName)
                    .ThenBy(o => o.FirstName)
                    .ToList();

                var ownerDisplayList = new System.Collections.Generic.List<object>();
                ownerDisplayList.Add(new
                {
                    OwnerId = 0,
                    DisplayText = "(не выбран)"
                });

                foreach (var owner in owners)
                {
                    string displayText = $"{owner.LastName} {owner.FirstName}";
                    if (!string.IsNullOrWhiteSpace(owner.MiddleName))
                    {
                        displayText += $" {owner.MiddleName}";
                    }

                    ownerDisplayList.Add(new
                    {
                        OwnerId = owner.OwnerId,
                        DisplayText = displayText,
                        Owner = owner
                    });
                }

                cmbOwner.ItemsSource = ownerDisplayList;
                cmbOwner.DisplayMemberPath = "DisplayText";
                cmbOwner.SelectedValuePath = "OwnerId";
                cmbOwner.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке владельцев: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearForm()
        {
            txtChip.Text = "";
            txtName.Text = "";
            txtBreed.Text = "";
            txtColor.Text = "";
            txtHeight.Text = "";
            txtWeight.Text = "";
            txtMother.Text = "";
            txtFather.Text = "";
            dpBirthDate.SelectedDate = null;
            rbMale.IsChecked = false;
            rbFemale.IsChecked = false;
            chkIsAlive.IsChecked = true;
            cmbKennel.SelectedIndex = 0;
            cmbOwner.SelectedIndex = 0;
            txtName.Focus();
        }
        private void SetInitialWatermarks()
        {
            TextBox_LostFocus(txtChip, null);
            TextBox_LostFocus(txtName, null);
            TextBox_LostFocus(txtBreed, null);
            TextBox_LostFocus(txtColor, null);
            TextBox_LostFocus(txtHeight, null);
            TextBox_LostFocus(txtWeight, null);
            TextBox_LostFocus(txtMother, null);
            TextBox_LostFocus(txtFather, null);
        }

        private void SetWatermarkIfEmpty(TextBox textBox)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = textBox.Tag?.ToString() ?? "";
                textBox.Foreground = System.Windows.Media.Brushes.Gray;
                textBox.FontStyle = FontStyles.Italic;
            }
        }
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                // Если текст равен подсказке - очищаем
                if (textBox.Text == textBox.Tag?.ToString())
                {
                    textBox.Text = "";
                    textBox.Foreground = System.Windows.Media.Brushes.Black;
                    textBox.FontStyle = FontStyles.Normal;
                }
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                // Если поле пустое - показываем подсказку
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = textBox.Tag?.ToString() ?? "";
                    textBox.Foreground = System.Windows.Media.Brushes.Gray;
                    textBox.FontStyle = FontStyles.Italic;
                }
            }
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || txtName.Text == txtName.Tag?.ToString())
            {
                MessageBox.Show("Введите кличку собаки!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtBreed.Text) || txtBreed.Text == txtBreed.Tag?.ToString())
            {
                MessageBox.Show("Введите породу собаки!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtBreed.Focus();
                return;
            }

            if (txtChip.Text == txtChip.Tag?.ToString())
            {
                MessageBox.Show("Введите номер чипа собаки!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtChip.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(txtChip.Text))
            {
                MessageBox.Show("Введите номер чипа собаки!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtChip.Focus();
                return;
            }

            string chipText = txtChip.Text.Trim();

            if (!int.TryParse(chipText, out int chipNumber) || chipNumber <= 0)
            {
                MessageBox.Show("Номер чипа должен быть положительным числом!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtChip.Focus();
                return;
            }

            var existingDog = _context.Dogs.Find(chipNumber);
            if (existingDog != null)
            {
                MessageBox.Show("Собака с таким номером чипа уже существует!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtChip.Focus();
                return;
            }

            if (rbMale.IsChecked != true && rbFemale.IsChecked != true)
            {
                MessageBox.Show("Выберите пол собаки!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!dpBirthDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите дату рождения собаки!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                dpBirthDate.Focus();
                return;
            }

            if (dpBirthDate.SelectedDate.Value > DateTime.Now)
            {
                MessageBox.Show("Дата рождения не может быть в будущем!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                dpBirthDate.Focus();
                return;
            }

            if (cmbOwner.SelectedValue == null || cmbOwner.SelectedIndex == 0)
            {
                MessageBox.Show("Выберите владельца собаки из списка!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbOwner.Focus();
                return;
            }

            var newDog = new Dog
            {
                ChipNumber = chipNumber,
                DogName = txtName.Text == txtName.Tag?.ToString() ? "" : txtName.Text.Trim(),
                Breed = txtBreed.Text == txtBreed.Tag?.ToString() ? "" : txtBreed.Text.Trim(),
                Color = txtColor.Text == txtColor.Tag?.ToString() ? "" : txtColor.Text.Trim(),
                MotherName = txtMother.Text == txtMother.Tag?.ToString() ? "" : txtMother.Text.Trim(),
                FatherName = txtFather.Text == txtFather.Tag?.ToString() ? "" : txtFather.Text.Trim(),
                IsAlive = chkIsAlive.IsChecked ?? true,
                BirthDate = DateOnly.FromDateTime(dpBirthDate.SelectedDate.Value),
                Gender = rbMale.IsChecked == true ? "Male" : "Female"
            };

            if (txtHeight.Text != txtHeight.Tag?.ToString() && int.TryParse(txtHeight.Text, out int height) && height > 0)
                newDog.HeightCm = height;

            if (txtWeight.Text != txtWeight.Tag?.ToString() && int.TryParse(txtWeight.Text, out int weight) && weight > 0)
                newDog.WeightKg = weight;

            if (cmbKennel.SelectedValue != null && cmbKennel.SelectedIndex > 0)
            {
                if (int.TryParse(cmbKennel.SelectedValue.ToString(), out int kennelId) && kennelId > 0)
                {
                    newDog.KennelId = kennelId;
                }
            }

            _context.Dogs.Add(newDog);
            _context.SaveChanges();

            if (cmbOwner.SelectedValue != null && cmbOwner.SelectedIndex > 0)
            {
                if (int.TryParse(cmbOwner.SelectedValue.ToString(), out int ownerId) && ownerId > 0)
                {
                    var ownerDogLink = new DogOwner
                    {
                        OwnerId = ownerId,
                        ChipNumber = newDog.ChipNumber
                    };

                    _context.DogOwners.Add(ownerDogLink);
                    _context.SaveChanges();
                }
            }

            MessageBox.Show($"Собака успешно зарегистрирована!\nЧип: {newDog.ChipNumber}",
                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            ClearForm();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Window newWindow = new dog_registry();
            newWindow.Show();
            this.Close();
        }

        private void AddOwnerButton_Click(object sender, RoutedEventArgs e)
        {
            var ownerWindow = new registration_owner();
            ownerWindow.Closed += (s, args) =>
            {
                LoadOwners();
            };

            ownerWindow.Show();
        }

        private void AddKennelButton_Click(object sender, RoutedEventArgs e)
        {
            var kennelWindow = new registration_nursery();
            kennelWindow.Closed += (s, args) =>
            {
                LoadKennels();
            };

            kennelWindow.Show();
        }
    }
}
