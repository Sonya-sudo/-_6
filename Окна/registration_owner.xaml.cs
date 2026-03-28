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
using Xceed.Wpf.Toolkit;

namespace Клуб_6.Окна
{
    public partial class registration_owner : Window
    {
        private КлубContext _context;
        private Owner _newOwner;
        private bool _isMouseClick = false;
        private string _phoneMask = "+7(000)000-00-00";

        public registration_owner()
        {
            InitializeComponent();
            _context = new КлубContext();
            _newOwner = new Owner();
            SetInitialWatermarks();

            InitializePhoneMask();
        }

        private void InitializePhoneMask()
        {
            // Настройка MaskedTextBox
            txtPhone.Mask = _phoneMask;
            txtPhone.PromptChar = '_';
            txtPhone.ValueDataType = typeof(string);

            // Устанавливаем начальное значение с подсказками
            txtPhone.Text = "";
        }

        private void txtPhone_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _isMouseClick = true;

            var maskedBox = sender as MaskedTextBox;
            if (maskedBox != null)
            {
                maskedBox.Focus();

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    SetCursorAfterLastDigit(maskedBox);
                }), System.Windows.Threading.DispatcherPriority.Input);

                e.Handled = true;
            }
        }

        private void SetCursorAfterLastDigit(MaskedTextBox maskedBox)
        {
            string text = maskedBox.Text ?? "";

            int lastDigitIndex = -1;
            for (int i = text.Length - 1; i >= 0; i--)
            {
                if (i < text.Length && char.IsDigit(text[i]))
                {
                    lastDigitIndex = i;
                    break;
                }
            }

            if (lastDigitIndex != -1)
            {
                // Ставим курсор после последней цифры
                maskedBox.CaretIndex = lastDigitIndex + 1;
            }
            else
            {
                // Если цифр нет, ставим курсор на первую позицию для ввода
                // +7( - первые 3 символа, затем начинаются цифры
                maskedBox.CaretIndex = 3; // Позиция для первой цифры
            }
        }

        private void txtPhone_GotFocus(object sender, RoutedEventArgs e)
        {
            var maskedBox = sender as MaskedTextBox;
            if (maskedBox != null && !_isMouseClick)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    SetCursorAfterLastDigit(maskedBox);
                }), System.Windows.Threading.DispatcherPriority.Input);
            }
            _isMouseClick = false;
        }

        private void SetInitialWatermarks()
        {
            SetWatermarkIfEmpty(txtLastName);
            SetWatermarkIfEmpty(txtFirstName);
            SetWatermarkIfEmpty(txtMiddleName);
            SetWatermarkIfEmpty(txtEmail);
            SetWatermarkIfEmpty(txtCity);
            SetWatermarkIfEmpty(txtAddress);
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

        private bool IsPhoneComplete()
        {
            string phone = txtPhone.Text ?? "";

            // Проверяем, нет ли подсказочных символов '_' (PromptChar)
            return !phone.Contains(txtPhone.PromptChar) &&
                   phone.Length == _phoneMask.Length;
        }

        private string GetPhoneDigits()
        {
            string phone = txtPhone.Text ?? "";

            // Извлекаем только цифры из номера
            string digits = new string(phone.Where(char.IsDigit).ToArray());

            // Убираем первую цифру 7 (код страны)
            if (digits.Length > 0 && digits[0] == '7')
            {
                digits = digits.Substring(1);
            }

            return digits;
        }

        private string FormatPhoneForDisplay(string digits)
        {
            // digits должны содержать 10 цифр (без кода страны)
            if (digits.Length == 10)
            {
                return $"+7({digits.Substring(0, 3)}){digits.Substring(3, 3)}-{digits.Substring(6, 2)}-{digits.Substring(8, 2)}";
            }
            return digits;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtLastName.Text) || txtLastName.Text == txtLastName.Tag?.ToString())
                {
                    System.Windows.MessageBox.Show("Введите фамилию владельца!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtLastName.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtFirstName.Text) || txtFirstName.Text == txtFirstName.Tag?.ToString())
                {
                    System.Windows.MessageBox.Show("Введите имя владельца!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtFirstName.Focus();
                    return;
                }

                string phoneDigits = GetPhoneDigits();

                if (!IsPhoneComplete() || phoneDigits.Length != 10)
                {
                    System.Windows.MessageBox.Show("Заполните номер телефона полностью (10 цифр)!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtPhone.Focus();
                    return;
                }

                _newOwner.LastName = txtLastName.Text == txtLastName.Tag?.ToString() ? "" : txtLastName.Text.Trim();
                _newOwner.FirstName = txtFirstName.Text == txtFirstName.Tag?.ToString() ? "" : txtFirstName.Text.Trim();
                _newOwner.MiddleName = txtMiddleName.Text == txtMiddleName.Tag?.ToString() ? "" : txtMiddleName.Text.Trim();
                _newOwner.Email = txtEmail.Text == txtEmail.Tag?.ToString() ? "" : txtEmail.Text.Trim();
                _newOwner.City = txtCity.Text == txtCity.Tag?.ToString() ? "" : txtCity.Text.Trim();
                _newOwner.Address = txtAddress.Text == txtAddress.Tag?.ToString() ? "" : txtAddress.Text.Trim();

                _newOwner.Phone = FormatPhoneForDisplay(phoneDigits);

                if (dpBirthDate.SelectedDate.HasValue)
                {
                    _newOwner.BirthDate = DateOnly.FromDateTime(dpBirthDate.SelectedDate.Value);
                }
                else
                {
                    _newOwner.BirthDate = null;
                }

                _context.Owner.Add(_newOwner);
                _context.SaveChanges();

                System.Windows.MessageBox.Show($"Владелец успешно зарегистрирован!\n",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                ClearForm();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearForm()
        {
            txtLastName.Text = "";
            txtFirstName.Text = "";
            txtMiddleName.Text = "";
            txtPhone.Text = "";
            txtEmail.Text = "";
            txtCity.Text = "";
            txtAddress.Text = "";
            dpBirthDate.SelectedDate = null;
            txtLastName.Focus();

            _newOwner = new Owner();

            // Восстанавливаем подсказки
            SetInitialWatermarks();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Window newWindow = new register_of_owners();
            newWindow.Show();
            this.Close();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
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
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = textBox.Tag?.ToString() ?? "";
                    textBox.Foreground = System.Windows.Media.Brushes.Gray;
                    textBox.FontStyle = FontStyles.Italic;
                }
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void txtPhone_KeyDown(object sender, KeyEventArgs e)
        {
            _isMouseClick = false;
        }
    }
}
