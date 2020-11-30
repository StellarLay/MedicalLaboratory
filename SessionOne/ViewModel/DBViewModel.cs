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
        public DispatcherTimer timeSession = new DispatcherTimer();
        public DispatcherTimer timeApi = new DispatcherTimer();

        public MedicalLaboratoryEntities DataBaseModel;
        public DBViewModel()
        {
            // Инициализируем наш контекст данных, чтобы в дальнейшем с ним работать
            DataBaseModel = new MedicalLaboratoryEntities();
            LoadData();

            timeSession.Tick += new EventHandler(session_time);
            timeSession.Interval = new TimeSpan(0, 0, 0, 0, 1);
            timeApi.Tick += new EventHandler(timer_tick);
            timeApi.Interval = new TimeSpan(0, 0, 0, 0, 1);
        }

        // Коллекции, в которые собираются данные из определенных таблиц БД, благодаря чему мы можем работать независимо от SQL Server
        public ObservableCollection<StrahovieCompaniiViewModel> Companies { get; private set; }
        public ObservableCollection<PacientsViewModel> Pacients { get; private set; }
        public ObservableCollection<ServiceViewModel> Services { get; private set; }
        public ObservableCollection<OrdersViewModel> Orders { get; private set; }
        public ObservableCollection<ServiceFilterPatient> ServicesPatientFilter { get; private set; }
        public ObservableCollection<HistoryViewModel> Histories { get; private set; }

        // доп коллекции
        private ObservableCollection<AnalyserViewModel> _Analysers;
        public ObservableCollection<AnalyserViewModel> Analysers
        {
            get => _Analysers;
            set => SetField(ref _Analysers, value);
        }

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

        // Таймер времени сеанса
        public int ms = 0;
        public int s = 0;
        public int min = 0;
        public int h = 0;
        private void session_time(object sender, EventArgs e)
        {
            if (ms == 60)
            {
                s++;
                ms = 0;
            }
            else if (s == 60)
            {
                min++;
                s = 0;
            }
            else if (min == 60)
            {
                h++;
                min = 0;
            }
            else
            {
                ms++;
            }
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
                                      where o.PacientId == getPatientId.Id && o.StatusService == "Не выполнена"
                                      select new ServiceFilterPatient{ Code = s.Code, Service = s.Service};

            ServicesPatientFilter.Clear();
            foreach (var item in servicePatientOrder)
            {
                ServicesPatientFilter.Add(item);
            }
        }

        // Подгружаем историю входа
        public void LoadHistory()
        {
            var result = from history in DataBaseModel.History
                         join user in DataBaseModel.Users on history.LoginId equals user.id
                         select new HistoryViewModel
                         {
                             Time = history.Time,
                             LoginName = user.name,
                             Status = history.Status
                         };

            Histories = new ObservableCollection<HistoryViewModel>(result);
        }

        // Отображаем статус услуг в зависимости от выбранного анализатора
        public void NotSuccessService(string analysatorValue, string fioPatient)
        {
            var result = from order in DataBaseModel.Orders
                         join service in DataBaseModel.Services on order.Services equals service.Code
                         join analyser in DataBaseModel.Analyzers on service.Analysers equals analyser.Id
                         where analyser.Name == analysatorValue && order.StatusService == "Выполнена" || order.StatusService == "Не выполнена"
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

        // Отображаем Завершенные услуги в зависимости от выбранного анализатора
        public void ServicesProcess(string analysatorValue, string fioPatient)
        {
            var result = from order in DataBaseModel.Orders
                         join service in DataBaseModel.Services on order.Services equals service.Code
                         join analyser in DataBaseModel.Analyzers on service.Analysers equals analyser.Id
                         where analyser.Name == analysatorValue && order.StatusService == "Завершено"
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
            try
            {
                var cur = App.Current.Windows.OfType<Window>().FirstOrDefault(o => o.IsActive);
                var user = DataBaseModel.Users.FirstOrDefault(w => w.login == login && w.password == password);
                string typename = "";

                if (user != null)
                {
                    var types = DataBaseModel.Types.FirstOrDefault(w => w.Id == user.type);
                    typename = types.Name;
                    App.username = user.name;
                    App.userimage = user.MainImage;
                    App.roleName = types.Name;
                    App.statusSession = false;
                    errorMessageLogin = "";

                    // Когда прогрес бар загрузился
                    if (statusLoading)
                    {
                        switch (types.Name)
                        {
                            case "Администратор":
                                AdminPage form = new AdminPage();
                                form.Show();
                                cur.Hide();
                                break;
                            case "Лаборант":
                                LaborantPage formLaborant = new LaborantPage();
                                formLaborant.Show();
                                cur.Hide();
                                break;
                            case "Лаборант исследователь":
                                LaborantIssledovatel formLaborantIssledovatel = new LaborantIssledovatel();
                                formLaborantIssledovatel.Show();
                                cur.Hide();
                                break;
                            case "Бухгалтер":
                                BuhgalterPage formBuhgalter = new BuhgalterPage();
                                formBuhgalter.Show();
                                break;
                        }
                    }
                }
                else if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
                {
                    errorMessageLogin = "Введите логин и пароль!";
                    return false;
                }
                else
                {
                    errorMessageLogin = "Такого пользователя не существует!";
                    return false;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return true;
        }

        // Меняем статус услуги, если одобрена
        public void ChangeStatusService(string servicename, string patient, string analysator)
        {
            var patientId = DataBaseModel.Pacients.FirstOrDefault(w => w.FIO == patient);
            var serviceId = DataBaseModel.Services.FirstOrDefault(w => w.Service == servicename);

            var item = DataBaseModel.Orders.FirstOrDefault(w => w.PacientId == patientId.Id && w.Services == serviceId.Code);
            item.StatusService = "Выполнена";
            DataBaseModel.SaveChanges();

            // Обновляем коллекцию
            NotSuccessService(analysator, patient);

            // Удаляем с коллекции завершенных услуг выбранную
            var removeItem = ProcessedServices.FirstOrDefault(w => w.Services == servicename && w.Patient == patientId.Id);
            ProcessedServices.Remove(removeItem);
        }

        // Get запрос API
        int patientId = 0;
        int serviceId = 0;
        string patientname = "";
        string servicename = "";
        string analyservalue = "";
        string progressvalue = "";
        private async void timer_tick(object sender, EventArgs e)
        {
            // GET Request
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync("http://localhost:5000/api/analyzer/" + analyservalue);

                if (response.StatusCode.ToString() != "BadRequest")
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    dynamic data = JsonConvert.DeserializeObject(responseBody);
                    progressvalue = data["progress"];

                    if (string.IsNullOrEmpty(progressvalue))
                    {
                        var result = data["services"][0]["result"];

                        var patient = DataBaseModel.Orders.FirstOrDefault(w => w.PacientId == patientId && w.Services == serviceId);
                        patient.StatusService = "Завершено";

                        string convertResult = result;
                        int k = 0;
                        for (int i = 0; i < convertResult.Length; i++)
                        {
                            if (char.IsDigit(convertResult[i]))
                            {
                                k = 1;
                            }
                        }
                        if (k == 1)
                        {
                            float res = float.Parse(convertResult.Replace('.', ','));
                            patient.Result = Math.Round(res, 2).ToString();
                        }
                        else
                        {
                            patient.Result = result;
                        }

                        DataBaseModel.SaveChanges();

                        // Подгружаем завершенные услуги
                        ProcessedServices.Clear();
                        ServicesProcess(analyservalue, patientname);

                        // Обновляем доступные исследования
                        ServicesPatientFilter.Clear();
                        LoadServicesAnalyser(patientname);

                        analyseMessage = "Анализ выполнен!";
                        timeApi.Stop();
                    }
                }
            }
        }

        // После нажатия на кнопку "Отправить на исследование" попадаем в этот метод
        public string analyseMessage;
        public async Task<bool> AnalyseOrder(string fio, string analysatorValue, string serviceName)
        {
            var getPatient = DataBaseModel.Pacients.FirstOrDefault(w => w.FIO == fio);
            var getService = DataBaseModel.Services.FirstOrDefault(w => w.Service == serviceName);
            patientId = getPatient.Id;
            analyservalue = analysatorValue;
            serviceId = getService.Code;
            patientname = fio;
            servicename = serviceName;

            string myData = @"{ ""patient"": ""{" + getPatient.Id + @"}"", ""services"": [{ ""serviceCode"": " + getService.Code + "}]}";
            using (var client = new HttpClient())
            {
                // POST Request
                var response = await client.PostAsync(
                    "http://localhost:5000/api/analyzer/" + analysatorValue,
                    new StringContent(myData, Encoding.UTF8, "application/json")); ;

                if (response.StatusCode.ToString() == "BadRequest")
                {
                    analyseMessage = "Анализатор занят другим процессом!";
                    return false;
                }
                else
                {
                    analyseMessage = "";
                    timeApi.Start();
                }
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
        public string colorText;
        public bool CreateOrder(string fio, string service, string timeday)
        {
            var item = DataBaseModel.Pacients.FirstOrDefault(w => w.FIO == fio);
            if(item == null)
            {
                MessageBox.Show("Пациент не найден! При нажатии на ОК откроется окно добавление пациента");
                AddPacientPage page = new AddPacientPage();
                page.Show();
                return false;
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
                    StatusOrder = "Ожидание результата",
                    StatusService = "Не выполнена",
                    TimeDay = Convert.ToInt32(timeday),
                    Price = getService.Price
                };

                var getOrder = DataBaseModel.Orders.FirstOrDefault(w => w.PacientId == pacient.Id && w.Services == getService.Code);
                if(getOrder == null)
                {
                    colorText = "#FF1EBC59";
                    DataBaseModel.Orders.Add(order);
                    DataBaseModel.SaveChanges();
                    errorMessageLogin = "Заказ успешно оформлен :) Общая сумма заказа: " + getService.Price + " руб.";
                }
                else
                {
                    errorMessageLogin = "Заказ на данное исследование уже существует!";
                    colorText = "#FFD02727";
                }

                return true;
            }
        }
    }
}
