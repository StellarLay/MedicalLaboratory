using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SessionOne.ViewModel
{
    public class PacientsViewModel : VM
    {
        public Pacients model;
        public PacientsViewModel(Pacients PacientModel)
        {
            model = PacientModel;

            Id = model.Id;
            FIO = model.FIO;
        }

        private int id;
        public int Id
        {
            get => id;
            set => SetField(ref id, value);
        }

        private string fio;
        public string FIO
        {
            get => fio;
            set => SetField(ref fio, value);
        }
    }
}
