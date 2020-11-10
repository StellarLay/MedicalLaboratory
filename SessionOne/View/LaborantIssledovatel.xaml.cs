using SessionOne.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
    /// Логика взаимодействия для LaborantIssledovatel.xaml
    /// </summary>
    public partial class LaborantIssledovatel : Window
    {
        public LaborantIssledovatel()
        {
            InitializeComponent();
            DataContext = new ApplicationViewModel();
        }

        // Исходники
        private async void Ishodnik()
        {
            string myData = @"{ ""patient"": ""{id}"", ""services"": [{ ""serviceCode"": 000 }]}";
            using(var client = new HttpClient())
            {
                // POST
                var response = await client.PostAsync(
                    "http://localhost:5000/api/analyzer/{name}",
                    new StringContent(myData, Encoding.UTF8, "application/json")); ;

                // GET
                var responseGet = await client.GetAsync("http://localhost:5000/api/analyzer/{name}");
            }
        }
    }
}
