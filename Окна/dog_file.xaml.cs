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
using Microsoft.EntityFrameworkCore;

namespace Клуб_6.Окна
{
    public partial class dog_file : Window
    {
        private КлубContext _context;
        public ObservableCollection<dynamic> DogList { get; set; }

        public List<string> СтатусыСобаки = new List<string> { "Жив", "Мертв" };

        public dog_file()
        {
            InitializeComponent();
            _context = new КлубContext();
            LoadDogs();
            DataContext = this;
        }

        private void LoadDogs()
        {
            try
            {
                var dogs = _context.Dog
                    .Include(d => d.Kennel)
                    .OrderBy(d => d.DogName)
                    .ToList();

                var dogsWithOwnerInfo = new ObservableCollection<dynamic>();

                foreach (var dog in dogs)
                {
                    var ownerRelation = _context.DogOwner
                        .FirstOrDefault(o => o.ChipNumber == dog.ChipNumber);

                    string владелецФИО = "не указан";

                    if (ownerRelation != null)
                    {
                        var owner = _context.Owner.Find(ownerRelation.OwnerId);
                        if (owner != null)
                        {
                            владелецФИО = $"{owner.LastName} {owner.FirstName}";
                            if (!string.IsNullOrWhiteSpace(owner.MiddleName))
                            {
                                владелецФИО += $" {owner.MiddleName}";
                            }
                        }
                    }

                    string статусСтрока = dog.IsAlive ?? true ? "Жив" : "Мертв";

                    dogsWithOwnerInfo.Add(new
                    {
                        Dog = dog,
                        ChipNumber = dog.ChipNumber,
                        DogName = dog.DogName,
                        Breed = dog.Breed,
                        BirthDate = dog.BirthDate.ToString("dd.MM.yyyy"),
                        MotherName = dog.MotherName,
                        FatherName = dog.FatherName,
                        Gender = dog.Gender,
                        HeightCm = dog.HeightCm,
                        WeightKg = dog.WeightKg,
                        Color = dog.Color,
                        IsAlive = статусСтрока,
                        KennelName = dog.Kennel?.KennelName ?? "не указан",
                        Kennel = dog.Kennel,
                        ВладелецФИО = владелецФИО
                    });
                }

                DogList = dogsWithOwnerInfo;
                BoxDog.ItemsSource = DogList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            Window newWindow = new dog_registry();
            newWindow.Show();
            this.Close();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void BoxEvents_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            btnChange_Click(sender, e);
        }
        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            if (BoxDog.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите собаку из списка!", "Внимание",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            dynamic selectedItem = BoxDog.SelectedItem;
            var selectedDog = (Dog)selectedItem.Dog;

            var cardWindow = new dog_card(selectedDog, _context);

            cardWindow.Closed += (s, args) =>
            {
                LoadDogs();
            };

            cardWindow.Show();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (BoxDog.SelectedItem == null)
            {
                MessageBox.Show("Выберите собаку для удаления!", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result_2 = MessageBox.Show("Вы уверены, что хотите удалить выбранную собаку?\n" +
                                        "Все связанные данные также будут удалены!",
                                        "Подтверждение удаления",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Warning);

            if (result_2 == MessageBoxResult.Yes)
            {
                try
                {
                    dynamic selectedItem = BoxDog.SelectedItem;
                    var chip = (int)selectedItem.ChipNumber;
                    var selectedDog = _context.Dog.Find(chip);

                    if (selectedDog == null)
                    {
                        MessageBox.Show("Не удалось получить данные собаки", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // 1. Удаляем DogList записи и связанные данные
                    var dogListEntries = _context.DogList
                        .Where(d => d.DogId == chip)
                        .ToList();

                    foreach (var dogListEntry in dogListEntries)
                    {
                        // Получаем RecordId этой записи
                        var recordId = dogListEntry.RecordId;

                        // 1.1. Удаляем связи DogCriteriaResults через разводную таблицу
                        var criteriaLinks = _context.DogCriteriaResultsDogList
                            .Where(link => link.RecordId == recordId)
                            .ToList();


                        // Удаляем сами связи из разводной таблицы
                        _context.DogCriteriaResultsDogList.RemoveRange(criteriaLinks);

                        // 1.2. Удаляем DogDisciplines
                        var dogDisciplines = _context.DogDiscipline
                            .Where(dd => dd.RecordId == recordId)
                            .ToList();
                        _context.DogDiscipline.RemoveRange(dogDisciplines);

                        // Удаляем DogList запись
                        _context.DogList.Remove(dogListEntry);
                    }

                    // 2. Удаляем DogOwners
                    var ownerRelations = _context.DogOwner
                        .Where(r => r.ChipNumber == chip)
                        .ToList();
                    _context.DogOwner.RemoveRange(ownerRelations);

                    // 3. Удаляем саму собаку
                    _context.Dog.Remove(selectedDog);

                    _context.SaveChanges();

                    LoadDogs();

                    MessageBox.Show("Собака успешно удалена!", "Успех",
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
}