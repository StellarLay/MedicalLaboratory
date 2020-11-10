using Newtonsoft.Json;
using SessionOne.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace SessionOne.ViewModel
{
    /// <summary>
    /// Это главный класс, отвечающий за всю выполняемую логику приложения, 
    /// задания от комманд приходят сюда и здесь взаимодействуют с базой данных, 
    /// выполняя необходимую работу
    /// </summary>
    class DBViewModel
    {
        DispatcherTimer timer = new DispatcherTimer();

        public MedicalLaboratoryEntities DataBaseModel;
        public DBViewModel()
        {
            // Инициализируем наш контекст данных, чтобы в дальнейшем с ним работать
            DataBaseModel = new MedicalLaboratoryEntities();

            timer.Tick += new EventHandler(timer_tick);
            timer.Interval = new TimeSpan(0, 0, 0, 1);

            LoadData();
        }

        // Коллекции, в которые собираются данные из определенных таблиц БД, благодаря чему мы можем работать независимо от SQL Server
        public ObservableCollection<StrahovieCompaniiViewModel> Companies { get; private set; }
        public ObservableCollection<PacientsViewModel> Pacients { get; private set; }
        public ObservableCollection<ServiceViewModel> Services { get; private set; }
        public ObservableCollection<AnalyserViewModel> Analysers { get; private set; }
        public ObservableCollection<OrdersViewModel> Orders { get; private set; }

        // Метод для заполнения всех необходимых нам коллекций для дальнейшей работы с ними
        public void LoadData()
        {
            try
            {
                Companies = new ObservableCollection<StrahovieCompaniiViewModel>();
                foreach (var item in DataBaseModel.StrahovieCompanii)
                {
                    Companies.Add(new StrahovieCompaniiViewModel(item));
                }

                Pacients = new ObservableCollection<PacientsViewModel>();
                foreach (var item in DataBaseModel.Pacients)
                {
                    Pacients.Add(new PacientsViewModel(item));
                }

                Services = new ObservableCollection<ServiceViewModel>();
                foreach (var item in DataBaseModel.Services)
                {
                    Services.Add(new ServiceViewModel(item));
                }

                Analysers = new ObservableCollection<AnalyserViewModel>();
                foreach (var item in DataBaseModel.Analyzers)
                {
                    Analysers.Add(new AnalyserViewModel(item));
                }

                Orders = new ObservableCollection<OrdersViewModel>();
                foreach (var item in DataBaseModel.Orders.Where(w => w.StatusService != "Выполнена" && w.StatusService != null))
                {
                    Orders.Add(new OrdersViewModel(item));
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        // Метод для авторизации пользователя
        public string LoginUser(string login, string password)
        {
            var user = DataBaseModel.Users.FirstOrDefault(w => w.login == login && w.password == password);

            string typename = "";
            if (user != null)
            {
                var types = DataBaseModel.Types.FirstOrDefault(w => w.Id == user.type);
                typename = types.Name;
                if (types.Name == "Лаборант исследователь")
                {
                    LaborantIssledovatel form = new LaborantIssledovatel();
                    
                    form.Show();
                }
                else if(types.Name == "Лаборант")
                {
                    LaborantPage form = new LaborantPage();
                    var cur = App.Current.Windows.OfType<Window>().FirstOrDefault(o => o.IsActive);
                    cur.Hide();
                    form.Show();
                }
                else if(types.Name == "Бухгалтер")
                {
                    //BuhgalterPage form = new BuhgalterPage();
                }
                else
                {
                    AdminPage form = new AdminPage();
                }
                return types.Name;
            }
            else
            {
                MessageBox.Show("Неуспешная попытка авторизации!");
            }
            return typename;
        }

        // Добавление пациента
        public void AddPacient(Pacients pacient)
        {
            if(string.IsNullOrEmpty(pacient.FIO) || pacient.DateBirthday == null || 
                pacient.PassportSerial == null || pacient.PassportNumber == null ||
                pacient.Phone == null || string.IsNullOrEmpty(pacient.Email) ||
                pacient.PolisNumber == null || pacient.TypePolis == null)
            {
                MessageBox.Show("Заполните все поля!");
            }
            else
            {
                DataBaseModel.Pacients.Add(pacient);
                DataBaseModel.SaveChanges();
                Pacients.Add(new PacientsViewModel(pacient));
                
                MessageBox.Show("Пациент успешно добавлен!");
            }
        }

        // Оформление заказа
        public void CreateOrder(string fio, string service)
        {
            var item = DataBaseModel.Pacients.FirstOrDefault(w => w.FIO == fio);
            if(item == null)
            {
                MessageBox.Show("Пациент не найден! При нажатии на ОК откроется окно добавление пациента");
                AddPacientPage page = new AddPacientPage();
                page.Show();
            }
            else
            {
                var pacient = DataBaseModel.Pacients.FirstOrDefault(w => w.FIO == fio);
                var getService = DataBaseModel.Services.FirstOrDefault(w => w.Service == service);
                string serviceCode = getService.Code.ToString();
                Orders order = new Orders
                {
                    PacientId = pacient.Id,
                    Services = serviceCode,
                    DateCreate = DateTime.Now,
                    StatusOrder = "Starting",
                    StatusService = "Processed",
                    TimeDay = 6
                };

                DataBaseModel.Orders.Add(order);
                DataBaseModel.SaveChanges();
                MessageBox.Show("Заказ успешно оформлен :) Общая сумма заказа: " + getService.Price + "руб");
            }
        }

        // Взаимодействие с API
        int patientId = 0;
        string analyservalue = "";
        string serviceCode = "";
        public string pgvalue = "";
        public bool IsComplete = false;

        string errorMessage = "";
        // С интервалом в секунду обращаемся к апи, чтобы узнать результат
        private async void timer_tick(object sender, EventArgs e)
        {
            // GET Request
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync("http://localhost:5000/api/analyzer/" + analyservalue);

                if(response.StatusCode.ToString() == "BadRequest")
                {
                    errorMessage = "400! " +  await response.Content.ReadAsStringAsync();
                }
                else
                {
                    var body = await response.Content.ReadAsStringAsync();
                    dynamic data = JsonConvert.DeserializeObject(body);

                    pgvalue = data["progress"];
                    if(!string.IsNullOrEmpty(pgvalue))
                    {
                        IsComplete = false;
                        pgvalue += "100";
                    }
                    else
                    {
                        double result = data["services"][0]["result"];
                        result = data["services"][0]["result"];


                        pgvalue = "0";
                        IsComplete = true;

                        if (MessageBox.Show(
                            "Результаты анализа: " + result + ". Принимаете ли вы результат анализа?",
                            "Информация",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Information) == MessageBoxResult.Yes)
                        {
                            var patient = DataBaseModel.Orders.FirstOrDefault(w => w.PacientId == patientId);
                            patient.StatusService = "Выполнена";
                            DataBaseModel.SaveChanges();

                            MessageBox.Show("Статус анализа : Выполнена, пациент может узнать данные :)");
                        }
                    }

                }
            }
        }

        private async void ApiProcess()
        {
            string myData = @"{ ""patient"": ""{"+  patientId + @"}"", ""services"": [{ ""serviceCode"": "+ serviceCode + "}]}";
            using (var client = new HttpClient())
            {
                // POST Request
                var response = await client.PostAsync(
                    "http://localhost:5000/api/analyzer/" + analyservalue,
                    new StringContent(myData, Encoding.UTF8, "application/json")); ;

                if(response.StatusCode.ToString() == "BadRequest")
                {
                    l = 1;
                    MessageBox.Show("Ошибка 400: " + await response.Content.ReadAsStringAsync());
                }
                else
                {
                    l = 0;
                    timer.Start();
                }
            }
        }
        public int l = 0;
        // Анализатор
        public void AnalyseOrder(string fio, string analysatorValue, string serviceName)
        {
            if(!string.IsNullOrEmpty(fio) && !string.IsNullOrEmpty(analysatorValue) && !string.IsNullOrEmpty(serviceName))
            {
                var getPacient = DataBaseModel.Pacients.FirstOrDefault(w => w.FIO == fio);
                var getService = DataBaseModel.Services.FirstOrDefault(w => w.Service == serviceName);
                string sc = getService.Code.ToString();
                var getServiceOrder = DataBaseModel.Orders.FirstOrDefault(w => w.Services == sc);

                if(getServiceOrder == null)
                {
                    MessageBox.Show("Заказа на анализ : " + serviceName + " нет!");
                }
                else
                {
                    int k = Convert.ToInt32(getServiceOrder.Services);
                    var AnalysatorService = DataBaseModel.Services.FirstOrDefault(w => w.Code == k);

                    if (AnalysatorService.Analysers != analysatorValue)
                    {
                        MessageBox.Show("Для анализа : " + serviceName + " выберите другой анализатор!");
                        l = 1;
                    }
                    else
                    {
                        l = 0;
                        serviceCode = getServiceOrder.Services;
                        patientId = getPacient.Id;
                        analyservalue = analysatorValue;
                        ApiProcess();
                    }
                }
            }
            else
            {
                l = 1;
                MessageBox.Show("Выберите пациента, анализатор и вид анализа!");
            }
        }
    }
}
