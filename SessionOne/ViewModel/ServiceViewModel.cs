using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SessionOne.ViewModel
{
    public class ServiceViewModel : VM
    {
        public Services model;
        public ServiceViewModel(Services ServiceModel)
        {
            model = ServiceModel;

            Code = model.Code;
            Service = model.Service;
            Price = model.Price.ToString();
            Analysers = model.Analysers.ToString();
        }

        private int code;
        public int Code
        {
            get => code;
            set => SetField(ref code, value);
        }

        private string service;
        public string Service
        {
            get => service;
            set => SetField(ref service, value);
        }

        private string price;
        public string Price
        {
            get => price;
            set => SetField(ref price, value);
        }
        public string TypeResult { get; set; }
        public string Analysers { get; set; }
    }
}
