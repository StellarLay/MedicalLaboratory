using SessionOne.View;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace SessionOne.ViewModel
{
    /// <summary>
    /// Класс, отвечающий за взаимодействие Представлений с логикой
    /// </summary>
    class ApplicationViewModel : VM
    {
        DispatcherTimer timer = new DispatcherTimer();
        DispatcherTimer timerSession = new DispatcherTimer();
        DispatcherTimer timerAnalyse = new DispatcherTimer();

        public DBViewModel DataBase { get; }

        public ApplicationViewModel()
        {
            DataBase = new DBViewModel();

            timer.Tick += new EventHandler(timer_bar);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            timerSession.Tick += new EventHandler(timer_session);
            timerSession.Interval = new TimeSpan(0, 0, 0, 0, 1);
            timerAnalyse.Tick += new EventHandler(timer_analyse);
            timerAnalyse.Interval = new TimeSpan(0, 0, 0, 0, 1);

            // При отрабатывании команды мы попадаем в определенный метод и выполняется определенная логика
            LoginCommand = new RelayCommand<object>(Login);
            BackBtnCommand = new RelayCommand<object>(_=>BackBtn());
            AddPacientBtnCommand = new RelayCommand<object>(_=>AddPacientBtn());
            AddPacientDataCommand = new RelayCommand<object>(_=>AddPacient());
            OpenOrderFormCommand = new RelayCommand<object>(_=>OpenOrderForm());
            CreateOrderCommand = new RelayCommand<object>(_=>CreateOrder());
            CheckAnalyserCommand = new RelayCommand<AnalyserViewModel>(CheckSelectAnalyser);
            CheckServiceCommand = new RelayCommand<ServiceFilterPatient>(CheckSelectService);
            SelectedFIOCommand = new RelayCommand<PacientsViewModel>(SelectedPacient);
            SendAnalyseCommand = new RelayCommand<object>(_ => SendAnalyse());
            SelectedPatientCommand = new RelayCommand<PacientsViewModel>(SelectedPatient);
            OpenInstructionCommand = new RelayCommand<object>(_=> OpenInstructionPage());
            SuccessServiceCommand = new RelayCommand<ProcessedServices>(SuccessServiceBtn);
            SelectionFioPacientCommand = new RelayCommand<object>(_ => SelectOrderFIO());
            SelectionResultFioPacientCommand = new RelayCommand<object>(_ => SelectResultService());
            SelectionResultServiceCommand = new RelayCommand<object>(_ => SelectionResultService());
            ReportBtnCommand = new RelayCommand<object>(ReportBtn);
            SuccessReportCommand = new RelayCommand<ReportViewModel>(SuccessReport);
            SearchByFioCommand = new RelayCommand<object>(_ => SearchByFio());
            ChangePacientCommand = new RelayCommand<object>(_ => ChangePatient());
            ResultPacientBtnCommand = new RelayCommand<object>(_ => OpenResultPacientWindow());
            GetResultPacientCommand = new RelayCommand<object>(_ => GetResultService());
            OpenPdfResultCommand = new RelayCommand<object>(_ => OpenPdfResult());
            OpenPrintTicketCommand = new RelayCommand<object>(_ => OpenPrintTicket());

            // Подгрузим приветствие пользователя
            UserName = "Добро пожаловать, " + App.username + "!";
            UserImage = App.userimage;

            ValuePatientAnalyzer = "";
            ServiceValue = "";
            AnalysatorValue = "";
            WarningMessage = "";
            OrderValue = App.lastIdNewOrder.ToString();
            IsNewOrder = false;
            IsGetResultBtn = false;
            ViewSelectAnalyse = false;
            ColorMessage = "#000000";
            IsVisibleAnalyseBtn = "Visible";
            VisibleOpenResultBtn = "Hidden";
            NamePolis = "Медицинское страхование";
            NameCompany = "ВТБ Страхование";
            DateBirthday = DateTime.Now;

            HeadReportText = App.HeadReport;
            MainReportText = App.MainReport;

            // Свойства для окна результатов анализа
            PrintFio = App.FIO;
            PrintLaborant = "Лаборант: " + " " + App.username;
            PrintStartDate = "Дата взятия образца: " + " " + App.StartDate;
            PrintDate = "Дата печати результата: " + " " + DateTime.Now.ToString("dd/MM/yyyy");

            // Запуск таймера сеанса лаборантов
            if (App.roleName == "Лаборант исследователь" || App.roleName == "Лаборант")
            {
                timerSession.Start();
            }
            else if(App.roleName == "Администратор")
            {
                DataBase.LoadHistory();
            }
            else if(App.roleName == "Бухгалтер")
            {
                DataBase.LoadReports();
            }
            App.roleName = "";
        }

        // Commands
        public ICommand LoginCommand { get; }
        public ICommand BackBtnCommand { get; }
        public ICommand AddPacientBtnCommand { get; }
        public ICommand AddPacientDataCommand { get; }
        public ICommand OpenOrderFormCommand { get; }
        public ICommand CreateOrderCommand { get; }
        public ICommand CheckAnalyserCommand { get; }
        public ICommand CheckServiceCommand { get; }
        public ICommand SelectedFIOCommand { get; }
        public ICommand SelectionFioPacientCommand { get; }
        public ICommand SelectionResultFioPacientCommand { get; }
        public ICommand SelectionResultServiceCommand { get; }
        public ICommand SendAnalyseCommand { get; }
        public ICommand SelectedPatientCommand { get; }
        public ICommand OpenInstructionCommand { get; }
        public ICommand SuccessServiceCommand { get; }
        public ICommand ReportBtnCommand { get; }
        public ICommand SuccessReportCommand { get; }
        public ICommand SearchByFioCommand { get; }
        public ICommand ChangePacientCommand { get; }
        public ICommand ResultPacientBtnCommand { get; }
        public ICommand GetResultPacientCommand { get; }
        public ICommand OpenPdfResultCommand { get; }
        public ICommand OpenPrintTicketCommand { get; }


        /// <summary>
        /// Методы
        /// </summary>
        
        // Открываем окно результатов анализа
        private void OpenPrintTicket()
        {
            // Дата взятия анализа
            var getPatientId = DataBase.DataBaseModel.Pacients.FirstOrDefault(w => w.FIO == SelectionFioPacient).Id;
            var getAnalyse = DataBase.DataBaseModel.Services.FirstOrDefault(w => w.Service == SelectService).Code;
            var getAnalyseDate = DataBase.DataBaseModel.Orders.FirstOrDefault(w => w.Services == getAnalyse && w.PacientId == getPatientId).DateCreate;
            
            var dateTime = getAnalyseDate;
            var formatDate = dateTime.ToString("dd.MM.yyyy");
            App.StartDate = formatDate;

            VisiblePrintBtn = "Visible";
            TicketPrint form = new TicketPrint();
            form.Show();
        }

        // Формируем .PDF файл с результатами анализов
        private void OpenPdfResult()
        {
            VisiblePrintBtn = "Hidden";
            var cur = App.Current.Windows.OfType<Window>().FirstOrDefault(o => o.IsActive);

            PrintDialog printDialog = new PrintDialog();
            if(printDialog.ShowDialog() == true)
            {
                printDialog.PrintVisual(cur, "Gg");
            }
        }

        // Авторизаци пользователя
        private void Login(object values)
        {
            PasswordValue = values.GetType().GetProperty("Password").GetValue(values).ToString();
            IsLogin = DataBase.LoginUser(LoginValue, PasswordValue);
            ErrorMessage = DataBase.errorMessageLogin;

            if (IsLogin)
            {
                ErrorMessage = "";
                timer.Start();
            }
        }

        // Поиск пациента по ФИО
        private void SearchByFio()
        {
            var item = DataBase.DataBaseModel.Pacients.FirstOrDefault(w => w.FIO == FIO);
            if (item != null)
            {
                var getNamePolis = DataBase.DataBaseModel.PolisTypes.FirstOrDefault(w => w.Id == item.TypePolis);
                var getNameCompany = DataBase.DataBaseModel.StrahovieCompanii.FirstOrDefault(w => w.Id == item.StrahovayaCompania);

                DateBirthday = (DateTime)item.DateBirthday;
                Serial = item.PassportSerial.ToString();
                Number = item.PassportNumber.ToString();
                Phone = item.Phone;
                Email = item.Email;
                PolisNumber = item.PolisNumber;
                NamePolis = getNamePolis.Name;
                NameCompany = getNameCompany.Name;
            }
            else
            {
                MessageBox.Show("Запрашиваемый пациент не найден!");
            }
        }

        // Изменяем данные пациента
        private void ChangePatient()
        {
            var item = DataBase.DataBaseModel.Pacients.FirstOrDefault(w => w.FIO == FIO);
            if(item != null)
            {
                var getIdPolis = DataBase.DataBaseModel.PolisTypes.FirstOrDefault(w => w.Name == NamePolis);
                var getIdCompany = DataBase.DataBaseModel.StrahovieCompanii.FirstOrDefault(w => w.Name == NameCompany);

                item.FIO = FIO;
                item.DateBirthday = DateBirthday;
                item.PassportSerial = int.Parse(Serial);
                item.PassportNumber = int.Parse(Number);
                item.Phone = Phone;
                item.Email = Email;
                item.PolisNumber = PolisNumber;
                item.TypePolis = getIdPolis.Id;
                item.StrahovayaCompania = getIdCompany.Id;

                DataBase.DataBaseModel.SaveChanges();

                MessageBox.Show("Данные о пациенте: " + FIO + " успешно обновлены!");
            }
            else
            {
                MessageBox.Show("Данного пациента не существует в базе данных!");
            }
        }

        // Таймер прогресс бара анализатора
        int msAn = 0;
        int sAn = 0;
        private void timer_analyse(object sender, EventArgs e)
        {
            if (msAn == 60)
            {
                sAn++;
                msAn = 0;
            }
            else if (sAn == 4)
            {
                IsProcess = false;
                IsIndeterminate = false;
                IsVisibleAnalyseBtn = "Visible";
                sAn = 0;
                timerAnalyse.Stop();
            }
            else
            {
                msAn++;
            }
        }

        // Время сеанса
        private void timer_session(object sender, EventArgs e)
        {
            DataBase.timeSession.Start();
            if (DataBase.min == 0)
            {
                App.statusSession = true;
                var cur = App.Current.Windows.OfType<Window>().FirstOrDefault(o => o.IsActive);
                LoginPage page = new LoginPage();
                page.Show();
                cur.Close();
                WarningMessage = "";
                timerSession.Stop();
            }
            else if (DataBase.min == 10)
            {
                WarningMessage = "До окончания сеанса осталась 10 минут";
            }
            TimerValue = "Время сеанса: " + DataBase.h + "ч : " + DataBase.min + "м";
            TimerValue2 = DataBase.ms.ToString() + DataBase.s.ToString();
        }

        // Таймер прогресс бара авторизации
        int ms = 0;
        private void timer_bar(object sender, EventArgs e)
        {
            ms++;
            if(ms == 180)
            {
                IsIndeterminate = false;
                ms = 0;
                // Включаем форму
                DataBase.statusLoading = true;
                DataBase.LoginUser(LoginValue, PasswordValue);
                timer.Stop();
                IsLogin = false;
            }
            else
            {
                IsIndeterminate = true;
            }
        }

        // Получим результаты анализов
        private void GetResultService()
        {
            if(!string.IsNullOrEmpty(SelectionFioPacient) && !string.IsNullOrEmpty(SelectService))
            {
                int getPatientId = DataBase.DataBaseModel.Pacients.FirstOrDefault(w => w.FIO == SelectionFioPacient).Id;
                int getServiceId = DataBase.DataBaseModel.Services.FirstOrDefault(w => w.Service == SelectService).Code;

                var statusOrder = DataBase.DataBaseModel.Orders.FirstOrDefault(w => w.PacientId == getPatientId && w.Services == getServiceId);
                if (statusOrder.StatusService == "Выполнена")
                {
                    VisibleOpenResultBtn = "Visible";
                }
                else
                {
                    WarningMessage = "Выбранный анализ ещё не готов!";
                }
            }
        }

        private void OpenResultPacientWindow()
        {
            ResultPacientPage page = new ResultPacientPage();
            page.Show();
        }

        // Открытие формы добавление пациента
        private void AddPacientBtn()
        {
            AddPacientPage page = new AddPacientPage();
            page.Show();
        }

        // Логика кнопки "Назад"
        private void BackBtn()
        {
            App.statusSession = true;
            DataBase.timeSession.Stop();
            timerSession.Stop();
            var cur = App.Current.Windows.OfType<Window>().FirstOrDefault(o => o.IsActive);

            if(cur.Name == "AddPacientForm" ||
                cur.Name == "OrderForm" ||
                cur.Name == "ReportForm")
            {
                cur.Close();
            }
            else
            {
                cur.Close();
                LoginPage form = new LoginPage();
                form.Show();
            }
        }

        // Открываем инструкцию для работы лаборанта-исследователя
        private void OpenInstructionPage()
        {
            InstructionPage page = new InstructionPage();
            var cur = App.Current.Windows.OfType<Window>().FirstOrDefault(o => o.IsActive);
            if(cur.Name == "ManualForm")
            {
                cur.Close();
            }
            else
            {
                page.Show();
            }
        }

        // Отлавливаем выбранного пациента в анализаторе
        private void SelectedPatient(PacientsViewModel pacient)
        {
            DataBase.LoadServicesAnalyser(ValuePatientAnalyzer);
            DataBase.NotSuccessService(AnalysatorValue, ValuePatientAnalyzer);
        }

        // Отлавливаем выбранный анализатор
        private void CheckSelectAnalyser(AnalyserViewModel analyser)
        {
            AnalysatorValue = analyser.Name;
            // Подгружаем невыполненные услуги
            DataBase.NotSuccessService(AnalysatorValue, ValuePatientAnalyzer);

            // Подгружаем услуги в работе
            DataBase.ServicesProcess(AnalysatorValue, ValuePatientAnalyzer);

            if(!string.IsNullOrEmpty(AnalysatorValue) && !string.IsNullOrEmpty(ValuePatientAnalyzer) && !string.IsNullOrEmpty(ServiceValue))
            {
                IsEnableAnalyseBtn = true;
            }
            else
            {
                IsEnableAnalyseBtn = false;
            }
        }

        // Отлавливаем выбранную услугу
        private void CheckSelectService(ServiceFilterPatient service)
        {
            ServiceValue = service.Service;
            DataBase.LoadAnalysatorForService(ServiceValue);
        }

        // Отлавливаем выбранного пациента
        private void SelectedPacient(PacientsViewModel pacient) => ValuePacient = pacient.FIO;

        // Кнопка одобрения результата
        private void SuccessServiceBtn(ProcessedServices service)
        {
            if(MessageBox.Show(
                "Одобрить результат?",
                "Информация",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                DataBase.ChangeStatusService(service.Services, ValuePatientAnalyzer, AnalysatorValue);
            }
        }

        // Принимаем отчеты
        private void SuccessReport(ReportViewModel report)
        {
            if (MessageBox.Show(
                "Принять выбранный отчет?",
                "Информация",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                DataBase.SuccessReport(report);
            }
        }

        // Создаем новый объект класса Pacients и добавляем его
        private void AddPacient()
        {
            var getPolisId = DataBase.DataBaseModel.PolisTypes.Where(w => w.Name == NamePolis).FirstOrDefault();
            var getCompanyId = DataBase.DataBaseModel.StrahovieCompanii.Where(w => w.Name == NameCompany).FirstOrDefault();
            Pacients pacient = new Pacients
            {
                FIO = FIO,
                DateBirthday = DateBirthday,
                PassportSerial = Convert.ToInt32(Serial),
                PassportNumber = Convert.ToInt32(Number),
                Phone = Phone,
                Email = Email,
                PolisNumber = PolisNumber,
                TypePolis = getPolisId.Id,
                StrahovayaCompania = getCompanyId.Id
            };
            DataBase.AddPacient(pacient);
            ColorMessage = DataBase.colorText;
            ErrorMessage = DataBase.errorMessageLogin;
        }
        
        // Открыть форму заказов
        private void OpenOrderForm()
        {
            var getLastIdOrder = DataBase.DataBaseModel.Orders.OrderByDescending(o => o.Id).FirstOrDefault();
            int lastId = getLastIdOrder.Id + 1;
            App.lastIdNewOrder = lastId;
            AddOrderPage page = new AddOrderPage();
            page.Show();
        }

        // Отправляем данные на анализ
        private async void SendAnalyse()
        {
            bool result = await DataBase.AnalyseOrder(ValuePatientAnalyzer, AnalysatorValue, ServiceValue);

            if(result == false)
            {
                AnalyseMessage = DataBase.analyseMessage;
                IsVisibleAnalyseBtn = "Visible";
            }
            else
            {
                IsIndeterminate = true;
                IsProcess = true;
                timerAnalyse.Start();
                IsVisibleAnalyseBtn = "Hidden";
                AnalyseMessage = "";
            }
        }

        // Выбрали пациента в создании заказа
        private void SelectOrderFIO()
        {
            WarningMessage = "";
            if(!string.IsNullOrEmpty(SelectionFioPacient) && !string.IsNullOrEmpty(SelectService)
                && !string.IsNullOrEmpty(TimeDayValue))
            {
                IsNewOrder = true;
            }
            else
            {
                IsNewOrder = false;
            }
        }

        private void SelectResultService()
        {
            VisibleOpenResultBtn = "Hidden";
            WarningMessage = "";

            if (!string.IsNullOrEmpty(SelectionFioPacient))
            {
                if (!string.IsNullOrEmpty(SelectionFioPacient) && !string.IsNullOrEmpty(SelectService))
                {
                    IsGetResultBtn = true;
                }
                else
                {
                    ViewSelectAnalyse = true;
                }
                DataBase.LoadResultServices(SelectionFioPacient);
            }
            else
            {
                IsGetResultBtn = false;
            }
        }

        // Отлавливаем выбранный анализ при получении результатов
        private void SelectionResultService()
        {
            VisibleOpenResultBtn = "Hidden";
            WarningMessage = "";

            App.FIO = SelectionFioPacient;

            if (!string.IsNullOrEmpty(SelectionFioPacient))
            {
                if (!string.IsNullOrEmpty(SelectionFioPacient) && !string.IsNullOrEmpty(SelectService))
                {
                    IsGetResultBtn = true;
                }
                else
                {
                    ViewSelectAnalyse = true;
                }
            }
            else
            {
                IsGetResultBtn = false;
            }
        }

        // Создание заказа
        private void CreateOrder()
        {
            bool result = DataBase.CreateOrder(SelectionFioPacient, SelectService, TimeDayValue);
            ColorMessage = DataBase.colorText;

            if (result)
            {
                int lastId = Convert.ToInt32(OrderValue) + 1;
                OrderValue = lastId.ToString();
                WarningMessage = DataBase.errorMessageLogin;
            }
            else
            {
                WarningMessage = "";
            }
        }

        // Кнопки отчетов
        private void ReportBtn(object nameBtn)
        {
            if(FromDate == null || ToDate == null)
            {
                ErrorMessage = "Заполните фильтр!";
            }
            else
            {
                double res = 0;
                App.HeadReport = "";
                App.MainReport = "";

                if (nameBtn.ToString() == "StatsOneBtn")
                {
                    var item = DataBase.DataBaseModel.Orders.Where(w => w.DateCreate >= FromDate && w.DateCreate <= ToDate);
                    if(item != null)
                    {
                        App.HeadReport = "Отчет по количеству заказов с " + FromDate.Value.Date.Date.ToShortDateString() + " по " + ToDate.Value.Date.Date.ToShortDateString();
                        App.MainReport = "Общее количество заказов за данный период: " + item.Count();
                        res = item.Count();

                        Report win = new Report();
                        win.Show();
                    }
                    else
                    {
                        MessageBox.Show("Заказов в данный период не было");
                    }
                }
                else if (nameBtn.ToString() == "StatsTwoBtn")
                {
                    var item = DataBase.DataBaseModel.Pacients.Where(w => w.RegisterDate >= FromDate && w.RegisterDate <= ToDate);
                    if (item != null)
                    {
                        App.HeadReport = "Отчет по зарегистрированным пациентам с " + FromDate.Value.Date.Date.ToShortDateString() + " по " + ToDate.Value.Date.Date.ToShortDateString();
                        App.MainReport = "Количество новых пациентов за данный период: " + item.Count();
                        res = item.Count();

                        Report win = new Report();
                        win.Show();
                    }
                    else
                    {
                        MessageBox.Show("В данный период не был зарегистрирован ни один пациент");
                    }
                }
                else if(nameBtn.ToString() == "StatsThreeBtn")
                {
                    var item = DataBase.DataBaseModel.Orders.Where(w => w.DateCreate >= FromDate && w.DateCreate <= ToDate).Sum(s => s.Price);
                    if(item != null)
                    {
                        App.HeadReport = "Отчет по общей прибыли с " + FromDate.Value.Date.Date.ToShortDateString() + " по " + ToDate.Value.Date.Date.ToShortDateString();
                        App.MainReport = "Общая прибыль составляет: " + item + "руб.";
                        res = Math.Round((double)item);

                        Report win = new Report();
                        win.Show();
                    }
                    else
                    {
                        MessageBox.Show("В данной период не было оформлено ни одного заказа");
                    }
                }

                // Занесём отчёт в БД
                Reports report = new Reports
                {
                    Name = App.HeadReport,
                    Result = res,
                    FromDate = (DateTime)FromDate,
                    ToDate = (DateTime)ToDate,
                    Status = "Не принят"
                };
                DataBase.DataBaseModel.Reports.Add(report);
                DataBase.DataBaseModel.SaveChanges();
                ErrorMessage = "";
            }
        }

        private string _VisiblePrintBtn;
        public string VisiblePrintBtn
        {
            get => _VisiblePrintBtn;
            set
            {
                _VisiblePrintBtn = value;
                OnPropertyChanged("VisiblePrintBtn");
            }
        }

        // Properties 
        private string login;
        public string LoginValue
        {
            get => login;
            set
            {
                login = value;
                OnPropertyChanged("LoginValue");
            }
        }

        private string passwordvalue;
        public string PasswordValue
        {
            get => passwordvalue;
            set
            {
                passwordvalue = value;
                OnPropertyChanged("PasswordValue");
            }
        }

        // Progress bar status
        private bool isindeterminate;
        public bool IsIndeterminate
        {
            get => isindeterminate;
            set
            {
                isindeterminate = value;
                OnPropertyChanged("IsIndeterminate");
            }
        }

        // Свойство сообщение об ошибке
        private string errormessage;
        public string ErrorMessage
        {
            get => errormessage;
            set
            {
                errormessage = value;
                OnPropertyChanged("ErrorMessage");
            }
        }

        // Цвет сообщения
        private string _ColorMessage;
        public string ColorMessage
        {
            get => _ColorMessage;
            set
            {
                _ColorMessage = value;
                OnPropertyChanged("ColorMessage");
            }
        }

        // Статус, если мы вошли
        private bool islogin;
        public bool IsLogin
        {
            get => islogin;
            set
            {
                islogin = value;
                OnPropertyChanged("IsLogin");
            }
        }

        // Свойство сообщение об ошибке
        private string userimage;
        public string UserImage
        {
            get => userimage;
            set
            {
                userimage = value;
                OnPropertyChanged("UserImage");
            }
        }

        private string username;
        public string UserName
        {
            get => username;
            set
            {
                username = value;
                OnPropertyChanged("UserName");
            }
        }

        // Свойства для добавления пациента
        private string fio;
        public string FIO
        {
            get => fio;
            set
            {
                fio = value;
                OnPropertyChanged("FIO");
            }
        }

        private DateTime datebirthday;
        public DateTime DateBirthday
        {
            get => datebirthday;
            set
            {
                datebirthday = value;
                OnPropertyChanged("DateBirthday");
            }
        }

        private string serial;
        public string Serial
        {
            get => serial;
            set
            {
                serial = value;
                OnPropertyChanged("Serial");
            }
        }

        private string number;
        public string Number
        {
            get => number;
            set
            {
                number = value;
                OnPropertyChanged("Number");
            }
        }

        private string phone;
        public string Phone
        {
            get => phone;
            set
            {
                phone = value;
                OnPropertyChanged("Phone");
            }
        }

        private string email;
        public string Email
        {
            get => email;
            set
            {
                email = value;
                OnPropertyChanged("Email");
            }
        }

        private string polisnumber;
        public string PolisNumber
        {
            get => polisnumber;
            set
            {
                polisnumber = value;
                OnPropertyChanged("PolisNumber");
            }
        }

        private string polistype;
        public string PolisType
        {
            get => polistype;
            set
            {
                polistype = value;
                OnPropertyChanged("PolisType");
            }
        }

        private string namecompany;
        public string NameCompany
        {
            get => namecompany;
            set
            {
                namecompany = value;
                OnPropertyChanged("NameCompany");
            }
        }

        private string namepolis;
        public string NamePolis
        {
            get => namepolis;
            set
            {
                namepolis = value;
                OnPropertyChanged("NamePolis");
            }
        }

        // Свойства для создания заказа
        private string selectionfioPacient;
        public string SelectionFioPacient
        {
            get => selectionfioPacient;
            set
            {
                selectionfioPacient = value;
                OnPropertyChanged("SelectionFioPacient");
            }
        }

        private string selectservice;
        public string SelectService
        {
            get => selectservice;
            set
            {
                selectservice = value;
                OnPropertyChanged("SelectService");
            }
        }

        // Свойство номера заказа в окне добавления заказа
        private string _OrderValue;
        public string OrderValue
        {
            get => _OrderValue;
            set
            {
                _OrderValue = value;
                OnPropertyChanged("OrderValue");
            }
        }

        // Выбрали ли мы пациента и услуг в окне добавления заказа
        private bool _IsNewOrder;
        public bool IsNewOrder
        {
            get => _IsNewOrder;
            set
            {
                _IsNewOrder = value;
                OnPropertyChanged("IsNewOrder");
            }
        }

        private string _VisibleOpenResultBtn;
        public string VisibleOpenResultBtn
        {
            get => _VisibleOpenResultBtn;
            set
            {
                _VisibleOpenResultBtn = value;
                OnPropertyChanged("VisibleOpenResultBtn");
            }
        }

        private bool _IsGetResultBtn;
        public bool IsGetResultBtn
        {
            get => _IsGetResultBtn;
            set
            {
                _IsGetResultBtn = value;
                OnPropertyChanged("IsGetResultBtn");
            }
        }

        private bool _ViewSelectAnalyse;
        public bool ViewSelectAnalyse
        {
            get => _ViewSelectAnalyse;
            set
            {
                _ViewSelectAnalyse = value;
                OnPropertyChanged("ViewSelectAnalyse");
            }
        }

        // Свойство времени выполнения заказа
        private string _TimeDayValue;
        public string TimeDayValue
        {
            get => _TimeDayValue;
            set
            {
                _TimeDayValue = value;
                OnPropertyChanged("TimeDayValue");
            }
        }

        // Свойства для работы с анализатором
        private string analysatorvalue;
        public string AnalysatorValue
        {
            get => analysatorvalue;
            set
            {
                analysatorvalue = value;
                OnPropertyChanged("AnalysatorValue");
            }
        }

        private string servicevalue;
        public string ServiceValue
        {
            get => servicevalue;
            set
            {
                servicevalue = value;
                OnPropertyChanged("ServiceValue");
            }
        }

        private bool isanalyse;
        public bool IsAnalyse
        {
            get => isanalyse;
            set
            {
                isanalyse = value;
                OnPropertyChanged("IsAnalyse");
            }
        }

        private string valuepacient;
        public string ValuePacient
        {
            get => valuepacient;
            set
            {
                valuepacient = value;
                OnPropertyChanged("ValuePacient");
            }
        }

        private string valuepacientanalyzer;
        public string ValuePatientAnalyzer
        {
            get => valuepacientanalyzer;
            set
            {
                valuepacientanalyzer = value;
                OnPropertyChanged("ValuePatientAnalyzer");
            }
        }

        private string _AnalyseMessage;
        public string AnalyseMessage
        {
            get => _AnalyseMessage;
            set
            {
                _AnalyseMessage = value;
                OnPropertyChanged("AnalyseMessage");
            }
        }

        private bool _IsEnableAnalyseBtn;
        public bool IsEnableAnalyseBtn
        {
            get => _IsEnableAnalyseBtn;
            set
            {
                _IsEnableAnalyseBtn = value;
                OnPropertyChanged("IsEnableAnalyseBtn");
            }
        }

        private string _IsVisibleAnalyseBtn;
        public string IsVisibleAnalyseBtn
        {
            get => _IsVisibleAnalyseBtn;
            set
            {
                _IsVisibleAnalyseBtn = value;
                OnPropertyChanged("IsVisibleAnalyseBtn");
            }
        }

        private bool _IsProcess;
        public bool IsProcess
        {
            get => _IsProcess;
            set
            {
                _IsProcess = value;
                OnPropertyChanged("IsProcess");
            }
        }

        // Свойство времени сеанса
        private string _TimerValue;
        public string TimerValue
        {
            get => _TimerValue;
            set
            {
                _TimerValue = value;
                OnPropertyChanged("TimerValue");
            }
        }

        private string _TimerValue2;
        public string TimerValue2
        {
            get => _TimerValue2;
            set
            {
                _TimerValue2 = value;
                OnPropertyChanged("TimerValue2");
            }
        }

        private string _WarningMessage;
        public string WarningMessage
        {
            get => _WarningMessage;
            set
            {
                _WarningMessage = value;
                OnPropertyChanged("WarningMessage");
            }
        }

        // Свойства отчетов
        private string _HeadReportText;
        public string HeadReportText
        {
            get => _HeadReportText;
            set
            {
                _HeadReportText = value;
                OnPropertyChanged("HeadReportText");
            }
        }

        private string _MainReportText;
        public string MainReportText
        {
            get => _MainReportText;
            set
            {
                _MainReportText = value;
                OnPropertyChanged("MainReportText");
            }
        }

        private Nullable<DateTime> _FromDate;
        public Nullable<DateTime> FromDate
        {
            get => _FromDate;
            set
            {
                _FromDate = value;
                OnPropertyChanged("FromDate");
            }
        }

        private Nullable<DateTime> _ToDate;
        public Nullable<DateTime> ToDate
        {
            get => _ToDate;
            set
            {
                _ToDate = value;
                OnPropertyChanged("ToDate");
            }
        }

        // Свойства для окна результатов анализов
        private string _PrintFio;
        public string PrintFio
        {
            get => _PrintFio;
            set
            {
                _PrintFio = value;
                OnPropertyChanged("PrintFio");
            }
        }

        private string _PrintLaborant;
        public string PrintLaborant
        {
            get => _PrintLaborant;
            set
            {
                _PrintLaborant = value;
                OnPropertyChanged("PrintLaborant");
            }
        }

        private string _PrintStartDate;
        public string PrintStartDate
        {
            get => _PrintStartDate;
            set
            {
                _PrintStartDate = value;
                OnPropertyChanged("PrintStartDate");
            }
        }

        private string _PrintDate;
        public string PrintDate
        {
            get => _PrintDate;
            set
            {
                _PrintDate = value;
                OnPropertyChanged("PrintDate");
            }
        }
    }
}
