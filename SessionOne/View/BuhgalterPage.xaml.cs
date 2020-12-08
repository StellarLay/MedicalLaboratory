﻿using SessionOne.ViewModel;
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
    /// Логика взаимодействия для BuhgalterPage.xaml
    /// </summary>
    public partial class BuhgalterPage : Window
    {
        public BuhgalterPage()
        {
            InitializeComponent();
            DataContext = new ApplicationViewModel();
        }
    }
}
