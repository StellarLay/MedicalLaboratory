using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SessionOne.ViewModel
{
    class OrdersViewModel : VM
    {
        public Orders model;
        public OrdersViewModel(Orders OrderModel)
        {
            model = OrderModel;

            Id = model.Id;
            DateCreate = model.DateCreate;
            PacientId = model.PacientId;
            Services = model.Services.ToString();
            StatusOrder = model.StatusOrder;
            StatusService = model.StatusService;
            TimeDay = model.TimeDay;
        }

        public int Id { get; set; }
        public Nullable<System.DateTime> DateCreate { get; set; }
        public Nullable<int> PacientId { get; set; }
        public string Services { get; set; }
        public string StatusOrder { get; set; }
        public string StatusService { get; set; }
        public Nullable<int> TimeDay { get; set; }
    }
}
