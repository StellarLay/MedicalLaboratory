using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SessionOne.ViewModel
{
    public class ProcessedServices : VM
    {
        private string _Services;
        public string Services
        {
            get => _Services;
            set => SetField(ref _Services, value);
        }

        private string _StatusService;
        public string StatusService
        {
            get => _StatusService;
            set => SetField(ref _StatusService, value);
        }

        private string _Result;
        public string Result
        {
            get => _Result;
            set => SetField(ref _Result, value);
        }

        public int Patient { get; set; }
        public string Analysator { get; set; }
    }
}
