using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Клуб_6.Models;
using Microsoft.EntityFrameworkCore;

namespace Клуб_6.Окна
{
    public partial class ViewEventTemplate : Window
    {
        private КлубContext _context;
        private int _compositionId;

        public class DisciplineViewModel
        {
            public string DisciplineName { get; set; }
            public int? Coefficient { get; set; }
        }

        public class CriterionViewModel
        {
            public string CriterionName { get; set; }
            public string Type { get; set; }
            public string Options { get; set; }
        }

        public ViewEventTemplate(int compositionId, string compositionName)
        {
            InitializeComponent();
            _context = new КлубContext();
            _compositionId = compositionId;

            txtTemplateName.Text = compositionName;
            LoadTemplateData();
        }

        private void LoadTemplateData()
        {
            try
            {
                // Загружаем дисциплины
                var disciplines = _context.Discipline
                    .Where(d => d.CompositionID == _compositionId)
                    .OrderBy(d => d.DisciplineId)
                    .Select(d => new DisciplineViewModel
                    {
                        DisciplineName = d.DisciplineName,
                        Coefficient = d.Coefficient
                    })
                    .ToList();

                if (disciplines.Any())
                {
                    lvDisciplines.ItemsSource = disciplines;
                    txtNoDisciplines.Visibility = Visibility.Collapsed;
                }
                else
                {
                    lvDisciplines.Visibility = Visibility.Collapsed;
                    txtNoDisciplines.Visibility = Visibility.Visible;
                }

                // Загружаем критерии с опциями
                var criteria = _context.Criterion
                    .Include(c => c.Options)
                    .Where(c => c.CompositionID == _compositionId)
                    .OrderBy(c => c.CriterionID)
                    .ToList();

                var criteriaViewModels = new List<CriterionViewModel>();

                foreach (var criterion in criteria)
                {
                    string type = criterion.Options.Any() ? "С выбором" : "Текстовый";
                    string options = "";

                    if (criterion.Options.Any())
                    {
                        var optionValues = criterion.Options
                            .OrderBy(o => o.OptionId)
                            .Select(o => o.OptionValue)
                            .ToList();
                        options = string.Join(" | ", optionValues);
                    }

                    criteriaViewModels.Add(new CriterionViewModel
                    {
                        CriterionName = criterion.CriterionName,
                        Type = type,
                        Options = options
                    });
                }

                if (criteriaViewModels.Any())
                {
                    lvCriteria.ItemsSource = criteriaViewModels;
                    txtNoCriteria.Visibility = Visibility.Collapsed;
                }
                else
                {
                    lvCriteria.Visibility = Visibility.Collapsed;
                    txtNoCriteria.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка");
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}