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
    /// Логика взаимодействия для LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Window
    {
        public LoginPage()
        {
            InitializeComponent();
            DataContext = new ApplicationViewModel();
        }

        // DragDrop window
        private void dragmoveBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            loginBox.Text = "srobken8";
            passBox.Password = "Cbmj3Yi";
        }

        private void CheckBox_Checked_1(object sender, RoutedEventArgs e)
        {
            loginBox.Text = "nmably1";
            passBox.Password = "ukM0e6";
        }

        private void CheckBox_Checked_2(object sender, RoutedEventArgs e)
        {
            loginBox.Text = "chacking0";
            passBox.Password = "4tzqHdkqzo4";
        }
    }
}
