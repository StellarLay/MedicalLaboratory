using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SessionOne.ViewModel
{
    public class NotSuccessServices : VM
    {
        private string _Service;
        public string Service
        {
            get => _Service;
            set => SetField(ref _Service, value);
        }

        private string _Status;
        public string Status
        {
            get => _Status;
            set => SetField(ref _Status, value);
        }
        public string Analysator { get; set; }
    }
}
