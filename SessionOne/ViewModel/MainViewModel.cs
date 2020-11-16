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
        public DBViewModel DataBase { get; }

        public ApplicationViewModel()
        {
            DataBase = new DBViewModel();

            timer.Tick += new EventHandler(timer_bar);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            timerSession.Tick += new EventHandler(timer_session);
            timerSession.Interval = new TimeSpan(0, 0, 0, 0, 1);

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

            // Подгрузим приветствие пользователя
            UserName = "Добро пожаловать, " + App.username + "!";
            UserImage = App.userimage;

            MaterialValue = "";
            ValuePatientAnalyzer = "";
            ServiceValue = "";
            AnalysatorValue = "";
            WarningMessage = "";

            // Запуск таймера сеанса лаборантов
            var cur = App.Current.Windows.OfType<Window>().FirstOrDefault(o => o.IsActive);
            if(App.roleName == "Лаборант исследователь")
            {
                timerSession.Start();
            }
        }

        // Время сеанса
        private void timer_session(object sender, EventArgs e)
        {
            DataBase.timeSession.Start();
            var cur = App.Current.Windows.OfType<Window>().FirstOrDefault(o => o.IsActive);
            if (DataBase.min == 2)
            {
                LoginPage page = new LoginPage();
                page.Show();
                cur.Close();
                WarningMessage = "";
                timerSession.Stop();
            }
            else if(DataBase.min == 1)
            {
                WarningMessage = "До окончания сеанса осталась 1 минута";
            }
            TimerValue = "Время сеанса: " + DataBase.h + "ч : " + DataBase.min + "м";
            TimerValue2 = DataBase.ms.ToString() + DataBase.s.ToString();
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
            Pacients pacient = new Pacients
            {
                FIO = FIO,
                DateBirthday = DateBirthday,
                PassportSerial = Convert.ToInt32(Serial),
                PassportNumber = Convert.ToInt32(Number),
                Phone = Phone,
                Email = Email,
                PolisNumber = PolisNumber,
                TypePolis = PolisType
            };
            DataBase.AddPacient(pacient);
        }
        private void OpenOrderForm()
        {
            AddOrderPage page = new AddOrderPage();
            page.Show();
        }

        private void CreateOrder()
        {
            DataBase.CreateOrder(SelectionFioPacient, SelectService);
        }

        // Отлавливаем выбранный анализатор
        private void CheckSelectAnalyser(AnalyserViewModel analyser) {
            AnalysatorValue = analyser.Name;
            // Подгружаем невыполненные услуги
            DataBase.NotSuccessService(AnalysatorValue, ValuePatientAnalyzer);

            // Подгружаем услуги в работе
            DataBase.ServicesProcess(AnalysatorValue, ValuePatientAnalyzer);
        }

        // Отлавливаем выбранную услугу
        private void CheckSelectService(ServiceFilterPatient service)
        {
            ServiceValue = service.Service;
            DataBase.LoadAnalysatorForService(ServiceValue);
        }

        // Отлавливаем выбранного пациента
        private void SelectedPacient(PacientsViewModel pacient) => ValuePacient = pacient.FIO;

        // Отправляем данные на анализ
        private void SendAnalyse()
        {
            //DataBase.AnalyseOrder(ValuePacient, AnalysatorValue, ServiceValue);

            //if(DataBase.l != 1)
            //{
            //    timer.Start();
            //}
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

        // Progress bar
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

        private string materialvalue;
        public string MaterialValue
        {
            get => materialvalue;
            set
            {
                materialvalue = value;
                OnPropertyChanged("MaterialValue");
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
