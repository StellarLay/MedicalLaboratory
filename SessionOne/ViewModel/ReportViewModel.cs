using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SessionOne.ViewModel
{
    class ReportViewModel : VM
    {
        public Reports model;
        public ReportViewModel(Reports ServiceModel)
        {
            model = ServiceModel;

            Id = model.Id;
            Name = model.Name;
            Result = model.Result;
            FromDate = model.FromDate;
            ToDate = model.ToDate;
            Status = model.Status;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public double Result { get; set; }
        public string Status { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
