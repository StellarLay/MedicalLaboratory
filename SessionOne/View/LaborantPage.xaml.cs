using SessionOne.ViewModel;
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

namespace SessionOne.View
{
    /// <summary>
    /// Логика взаимодействия для LaborantPage.xaml
    /// </summary>
    public partial class LaborantPage : Window
    {
        public LaborantPage()
        {
            InitializeComponent();
            DataContext = new ApplicationViewModel();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (App.statusSession == false)
            {
                if (MessageBox.Show(
                    "Вы действительно хотите выйти?",
                    "Информация",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    e.Cancel = false;
                    Application.Current.Shutdown();
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
