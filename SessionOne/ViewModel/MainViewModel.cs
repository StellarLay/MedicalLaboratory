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
        public DBViewModel DataBase { get; }

        public ApplicationViewModel()
        {
            DataBase = new DBViewModel();

            timer.Tick += new EventHandler(timer_tick);
            timer.Interval = new TimeSpan(0, 0, 0, 1);

            // При отрабатывании команды мы попадаем в определенный метод и выполняется определенная логика
            LoginCommand = new RelayCommand<object>(Login);
            RefreshCaptchaCommand = new RelayCommand<object>(_=>RefreshCaptcha());
            BackBtnCommand = new RelayCommand<object>(_=>BackBtn());
            AddPacientBtnCommand = new RelayCommand<object>(_=>AddPacientBtn());
            AddPacientDataCommand = new RelayCommand<object>(_=>AddPacient());
            OpenOrderFormCommand = new RelayCommand<object>(_=>OpenOrderForm());
            CreateOrderCommand = new RelayCommand<object>(_=>CreateOrder());
            CheckAnalyserCommand = new RelayCommand<AnalyserViewModel>(CheckSelectAnalyser);
            CheckServiceCommand = new RelayCommand<ServiceViewModel>(CheckSelectService);
            SelectedFIOCommand = new RelayCommand<PacientsViewModel>(SelectedPacient);
            SendAnalyseCommand = new RelayCommand<object>(_ => SendAnalyse());

            MaterialValue = "";
        }

        // Commands
        public ICommand LoginCommand { get; }
        public ICommand RefreshCaptchaCommand { get; }
        public ICommand BackBtnCommand { get; }
        public ICommand AddPacientBtnCommand { get; }
        public ICommand AddPacientDataCommand { get; }
        public ICommand OpenOrderFormCommand { get; }
        public ICommand CreateOrderCommand { get; }
        public ICommand CheckAnalyserCommand { get; }
        public ICommand CheckServiceCommand { get; }
        public ICommand SelectedFIOCommand { get; }
        public ICommand SendAnalyseCommand { get; }

        /// <summary>
        /// Методы
        /// </summary>
        private void Login(object values)
        {
            PasswordValue = values.GetType().GetProperty("Password").GetValue(values).ToString();
            string nameForm = DataBase.LoginUser(LoginValue, PasswordValue);
        }

        // Открытие формы добавление пациента
        private void AddPacientBtn()
        {
            AddPacientPage page = new AddPacientPage();
            page.Show();
        }
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
        private void RefreshCaptcha() => Captcha();
        private void Captcha()
        {
            Random rnd = new Random();
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            CaptchaValue = "";
            for (int i = 0; i < 4; i++)
            {
                CaptchaValue += chars[rnd.Next(0,10)];
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
        private void CheckSelectAnalyser(AnalyserViewModel analyser) => AnalysatorValue = analyser.Name;

        // Отлавливаем выбранную услугу
        private void CheckSelectService(ServiceViewModel service) => ServiceValue = service.Service;

        // Отлавливаем выбранного пациента
        private void SelectedPacient(PacientsViewModel pacient) => ValuePacient = pacient.FIO;

        // Отправляем данные на анализ
        private void SendAnalyse()
        {
            //int pbValue = 
            DataBase.AnalyseOrder(ValuePacient, AnalysatorValue, ServiceValue);
            //ProgBarValue = pbValue.ToString();

            if(DataBase.l != 1)
            {
                timer.Start();
            }
        }

        private void timer_tick(object sender, EventArgs e)
        {
            var barValue = DataBase.pgvalue;

            ProgBarValue += barValue;
            if (DataBase.IsComplete == false)
            {
                IsAnalyse = true;
            }
            else
            {
                IsAnalyse = false;
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

        private string captchavalue;
        public string CaptchaValue
        {
            get => captchavalue;
            set
            {
                captchavalue = value;
                OnPropertyChanged("CaptchaValue");
            }
        }

        private string captchainput;
        public string CaptchaInput
        {
            get => captchainput;
            set
            {
                captchainput = value;
                OnPropertyChanged("CaptchaInput");
            }
        }

        private bool iscaptcha;
        public bool IsCaptcha
        {
            get => iscaptcha;
            set
            {
                iscaptcha = value;
                OnPropertyChanged("IsCaptcha");
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

        private string progbarvalue;
        public string ProgBarValue
        {
            get => progbarvalue;
            set
            {
                progbarvalue = value;
                OnPropertyChanged("ProgBarValue");
            }
        }
    }
}
