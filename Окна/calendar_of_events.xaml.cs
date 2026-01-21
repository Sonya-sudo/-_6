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

namespace Клуб_6.Окна
{
    public partial class calendar_of_events : Window
    {
        public calendar_of_events()
        {
            InitializeComponent();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            Window newWindow = null;
            newWindow = new MainWindow();
            newWindow.Show();
            this.Close();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            if (clickedButton == null) return;

            Window newWindow = null;

            switch (clickedButton.Name)
            {
                case "btnCalendarOfEvents":
                    newWindow = new Schedule();
                    break;
                case "btnRegistrationEvent":
                    newWindow = new ChooseEvent();
                    break;
                case "btnArchive":
                    newWindow = new ArchiveResult();
                    break;
                default:
                    return;
            }

            newWindow.Show();
            this.Close();
        }
    }
}
