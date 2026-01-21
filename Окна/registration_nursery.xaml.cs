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
    public partial class registration_nursery : Window
    {
        private Клуб6Context _context;
        private Kennel _newNursery;
        private bool _isMouseClick = false;
        private string _phoneMask = "+7(000)000-00-00";

        public registration_nursery()
        {
            InitializeComponent();
            _context = new Клуб6Context();
            _newNursery = new Kennel();
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

        private void ClearForm()
        {
            _newNursery = new Kennel();
            txtName.Text = "";
            dpBirthDate.SelectedDate = null;
            txtCountry.Text = "";
            txtCity.Text = "";
            txtAddress.Text = "";
            txtPhone.Text = "";
            txtEmail.Text = "";
            txtName.Focus();

            SetInitialWatermarks();
        }

        private void SetInitialWatermarks()
        {
            SetWatermarkIfEmpty(txtName);
            SetWatermarkIfEmpty(txtCountry);
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
            if (string.IsNullOrWhiteSpace(txtName.Text) || txtName.Text == txtName.Tag?.ToString())
            {
                System.Windows.MessageBox.Show("Введите название питомника!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtName.Focus();
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

            _newNursery.KennelName = txtName.Text == txtName.Tag?.ToString() ? "" : txtName.Text.Trim();
            _newNursery.Email = txtEmail.Text == txtEmail.Tag?.ToString() ? "" : txtEmail.Text.Trim();
            _newNursery.City = txtCity.Text == txtCity.Tag?.ToString() ? "" : txtCity.Text.Trim();
            _newNursery.Address = txtAddress.Text == txtAddress.Tag?.ToString() ? "" : txtAddress.Text.Trim();
            _newNursery.Country = txtCountry.Text == txtCountry.Tag?.ToString() ? "" : txtCountry.Text.Trim();

            _newNursery.Phone = FormatPhoneForDisplay(phoneDigits);

            if (dpBirthDate.SelectedDate.HasValue)
            {
                _newNursery.FoundationDate = DateOnly.FromDateTime(dpBirthDate.SelectedDate.Value);
            }
            else
            {
                _newNursery.FoundationDate = null;
            }

            _context.Kennels.Add(_newNursery);
            _context.SaveChanges();

            System.Windows.MessageBox.Show($"Питомник успешно зарегистрирован!\n",
                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            ClearForm();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Window newWindow = new nursery_registry();
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

        private void txtPhone_KeyDown(object sender, KeyEventArgs e)
        {
            _isMouseClick = false;
        }
    }
}
