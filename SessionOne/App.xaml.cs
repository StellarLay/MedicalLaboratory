using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SessionOne
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string username;
        public static string userimage;
        public static string roleName;
        public static bool statusSession = false;
        public static int lastIdNewOrder;

        public static string HeadReport;
        public static string MainReport;

        // Свойства для окна результатов анализа
        public static string FIO;
        public static string Laborant;
        public static string StartDate;
        public static string NameService;
        public static string Result;
        public static string Comment;
    }
}
