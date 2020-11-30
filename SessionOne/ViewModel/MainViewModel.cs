using SessionOne.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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

            // Подгрузим приветствие пользователя
            UserName = "Добро пожаловать, " + App.username + "!";
            UserImage = App.userimage;
            DateBirthday = App.dateBirthday;

            ValuePatientAnalyzer = "";
            ServiceValue = "";
            AnalysatorValue = "";
            WarningMessage = "";
            OrderValue = App.lastIdNewOrder.ToString();
            IsNewOrder = false;
            ColorMessage = "#000000";
            IsVisibleAnalyseBtn = "Visible";

            // Запуск таймера сеанса лаборантов
            if (App.roleName == "Лаборант исследователь" || App.roleName == "Лаборант")
            {
                timerSession.Start();
            }
            else if(App.roleName == "Администратор")
            {
                DataBase.LoadHistory();
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
        public ICommand SendAnalyseCommand { get; }
        public ICommand SelectedPatientCommand { get; }
        public ICommand OpenInstructionCommand { get; }
        public ICommand SuccessServiceCommand { get; }


        /// <summary>
        /// Методы
        /// </summary>

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
            if (DataBase.min == 10)
            {
                App.statusSession = true;
                var cur = App.Current.Windows.OfType<Window>().FirstOrDefault(o => o.IsActive);
                LoginPage page = new LoginPage();
                page.Show();
                cur.Close();
                WarningMessage = "";
                timerSession.Stop();
            }
            else if (DataBase.min == 5)
            {
                WarningMessage = "До окончания сеанса осталась 5 минут";
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

            if(cur.Name == "AddPacientForm")
            {
                cur.Close();
            }
            else if (cur.Name == "OrderForm")
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

        // Создаем новый объект класса Pacients и добавляем его
        private void AddPacient()
        {
            var getPolisId = DataBase.DataBaseModel.PolisTypes.FirstOrDefault(w => w.Name == NamePolis);
            var getCompanyId = DataBase.DataBaseModel.StrahovieCompanii.FirstOrDefault(w => w.Name == NameCompany);
            if(getPolisId != null && getCompanyId != null)
            {
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

                bool result = DataBase.AddPacient(pacient);
                ColorMessage = DataBase.colorText;

                if (result)
                {
                    WarningMessage = DataBase.errorMessageLogin;
                }
                else
                {
                    WarningMessage = DataBase.errorMessageLogin;
                }
            }
            else
            {
                ColorMessage = "#FFD02727";
                WarningMessage = "Заполните все поля!";
            }
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
                WarningMessage = DataBase.errorMessageLogin;
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
                OnPropertyChanged("Numberl");
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

        // Свойство комбобокса имени страховой компании
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

        // Свойство комбобокса типа полиса
        private string _NamePolis;
        public string NamePolis
        {
            get => _NamePolis;
            set
            {
                _NamePolis = value;
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
    }
}
