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
    class DBViewModel : VM
    {
        public MedicalLaboratoryEntities DataBaseModel;
        public DBViewModel()
        {
            // Инициализируем наш контекст данных, чтобы в дальнейшем с ним работать
            DataBaseModel = new MedicalLaboratoryEntities();
            LoadData();
        }

        // Коллекции, в которые собираются данные из определенных таблиц БД, благодаря чему мы можем работать независимо от SQL Server
        public ObservableCollection<StrahovieCompaniiViewModel> Companies { get; private set; }
        public ObservableCollection<PacientsViewModel> Pacients { get; private set; }
        public ObservableCollection<ServiceViewModel> Services { get; private set; }

        private ObservableCollection<AnalyserViewModel> _Analysers;
        public ObservableCollection<AnalyserViewModel> Analysers
        {
            get => _Analysers;
            set => SetField(ref _Analysers, value);
        }

        public ObservableCollection<OrdersViewModel> Orders { get; private set; }

        // доп коллекции
        public ObservableCollection<ServiceFilterPatient> ServicesPatientFilter { get; private set; }

        private ObservableCollection<NotSuccessServices> _NotSuccessServ;
        public ObservableCollection<NotSuccessServices> NotSuccessServ
        {
            get => _NotSuccessServ;
            set => SetField(ref _NotSuccessServ, value);
        }

        private ObservableCollection<ProcessedServices> _ProcessedServices;
        public ObservableCollection<ProcessedServices> ProcessedServices
        {
            get => _ProcessedServices;
            set => SetField(ref _ProcessedServices, value);
        }

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

                ServicesPatientFilter = new ObservableCollection<ServiceFilterPatient>();
                var servicePatientOrder = from s in DataBaseModel.Services
                                          select new ServiceFilterPatient { Code = s.Code, Service = s.Service };
                foreach (var item in servicePatientOrder)
                {
                    ServicesPatientFilter.Add(item);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // После выбора пациента подгружаем только анализы, доступные для него
        public void LoadServicesAnalyser(string patientFio)
        {
            Services.Clear();

            var getPatientId = DataBaseModel.Pacients.FirstOrDefault(w => w.FIO == patientFio);
            var servicePatientOrder = from o in DataBaseModel.Orders
                                      join s in DataBaseModel.Services on o.Services equals s.Code
                                      where o.PacientId == getPatientId.Id
                                      select new ServiceFilterPatient{ Code = s.Code, Service = s.Service};

            ServicesPatientFilter.Clear();
            foreach (var item in servicePatientOrder)
            {
                ServicesPatientFilter.Add(item);
            }
        }

        // Отображаем невыполненные услуги в зависимости от выбранного анализатора
        public void NotSuccessService(string analysatorValue, string fioPatient)
        {
            var result = from order in DataBaseModel.Orders
                         join service in DataBaseModel.Services on order.Services equals service.Code
                         join analyser in DataBaseModel.Analyzers on service.Analysers equals analyser.Id
                         where analyser.Name == analysatorValue && order.StatusService == "Не выполнена"
                         select new NotSuccessServices
                         {
                             Service = service.Service,
                             Status = order.StatusService,
                             Patient = (int)order.PacientId,
                             Analysator = analyser.Name
                         };

            // Если мы не выбрали пациента, то грузим все невыполненные услуги
            if (string.IsNullOrEmpty(fioPatient))
            {
                NotSuccessServ = new ObservableCollection<NotSuccessServices>(result);
            }
            // А если выбрали, то только его услуги
            else
            {
                var getIdPatient = DataBaseModel.Pacients.FirstOrDefault(w => w.FIO == fioPatient);
                NotSuccessServ = new ObservableCollection<NotSuccessServices>(result.Where(w => w.Patient == getIdPatient.Id));
            }
        }

        // Отображаем услуги в работе в зависимости от выбранного анализатора
        public void ServicesProcess(string analysatorValue, string fioPatient)
        {
            var result = from order in DataBaseModel.Orders
                         join service in DataBaseModel.Services on order.Services equals service.Code
                         join analyser in DataBaseModel.Analyzers on service.Analysers equals analyser.Id
                         where analyser.Name == analysatorValue && order.StatusService != "Не выполнена"
                         select new ProcessedServices
                         {
                             Services = service.Service,
                             StatusService = order.StatusService,
                             Result = order.Result,
                             Patient = (int)order.PacientId,
                             Analysator = analyser.Name
                         };

            // Если мы не выбрали пациента, то грузим все услуги в работе
            if (string.IsNullOrEmpty(fioPatient))
            {
                ProcessedServices = new ObservableCollection<ProcessedServices>(result);
            }
            // А если выбрали, то только его услуги
            else
            {
                var getIdPatient = DataBaseModel.Pacients.FirstOrDefault(w => w.FIO == fioPatient);
                ProcessedServices = new ObservableCollection<ProcessedServices>(result.Where(w => w.Patient == getIdPatient.Id));
            }
        }

        // При выборе услуги грузим соответствующий анализатор
        public void LoadAnalysatorForService(string serviceName)
        {
            var getAnalysatorId = DataBaseModel.Services.FirstOrDefault(w => w.Service == serviceName);
            var result = DataBaseModel.Analyzers.Where(w => w.Id == getAnalysatorId.Analysers);

            Analysers = new ObservableCollection<AnalyserViewModel>();
            foreach (var item in result)
            {
                Analysers.Add(new AnalyserViewModel(item));
            }
        }

        // Метод для авторизации пользователя
        public string errorMessageLogin;
        public bool statusLoading;
        public bool LoginUser(string login, string password)
        {
            var user = DataBaseModel.Users.FirstOrDefault(w => w.login == login && w.password == password);
            string typename = "";

            if (user != null)
            {
                var types = DataBaseModel.Types.FirstOrDefault(w => w.Id == user.type);
                typename = types.Name;
                App.username = user.name;
                App.userimage = user.MainImage;

                // Когда прогрес бар загрузился
                if(statusLoading)
                {
                    switch (types.Name)
                    {
                        case "Администратор":
                            AdminPage form = new AdminPage();
                            form.Show();
                            errorMessageLogin = "";
                            break;
                        case "Лаборант":
                            LaborantPage formLaborant = new LaborantPage();
                            var cur = App.Current.Windows.OfType<Window>().FirstOrDefault(o => o.IsActive);
                            cur.Hide();
                            formLaborant.Show();
                            errorMessageLogin = "";
                            break;
                        case "Лаборант исследователь":
                            var curLogin = App.Current.Windows.OfType<Window>().FirstOrDefault(o => o.IsActive);
                            curLogin.Hide();
                            LaborantIssledovatel formLaborantIssledovatel = new LaborantIssledovatel();
                            formLaborantIssledovatel.Show();
                            errorMessageLogin = "";
                            break;
                        case "Бухгалтер":
                            BuhgalterPage formBuhgalter = new BuhgalterPage();
                            formBuhgalter.Show();
                            errorMessageLogin = "";
                            break;
                    }
                }
            }
            else if(string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                errorMessageLogin = "Введите логин и пароль!";
                return false;
            }
            else
            {
                errorMessageLogin = "Такого пользователя не существует!";
                return false;
            }

            return true;
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
                    Services = Convert.ToInt32(serviceCode),
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
        int serviceCode = 0;
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
                    //timer.Start();
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
                int sc = getService.Code;
                var getServiceOrder = DataBaseModel.Orders.FirstOrDefault(w => w.Services == sc);

                if(getServiceOrder == null)
                {
                    MessageBox.Show("Заказа на анализ : " + serviceName + " нет!");
                }
                else
                {
                    int k = Convert.ToInt32(getServiceOrder.Services);
                    var AnalysatorService = DataBaseModel.Services.FirstOrDefault(w => w.Code == k);
                    var getAnalysator = DataBaseModel.Analyzers.FirstOrDefault(w => w.Name == analysatorValue);
                    if (AnalysatorService.Analysers != getAnalysator.Id)
                    {
                        MessageBox.Show("Для анализа : " + serviceName + " выберите другой анализатор!");
                        l = 1;
                    }
                    else
                    {
                        l = 0;
                        serviceCode = (int)getServiceOrder.Services;
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
