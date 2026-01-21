using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Клуб_6.Окна;

namespace Клуб_6
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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
                case "btnRegistry":
                    newWindow = new dog_registry();
                    break;
                case "btnOwners":
                    newWindow = new register_of_owners();
                    break;
                case "btnNursery":
                    newWindow = new nursery_registry();
                    break;
                case "btnCalendar":
                    newWindow = new calendar_of_events();
                    break;
                default:
                    return;
            }

            newWindow.Show();
            Close();
        }
    }
}