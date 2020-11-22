using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SessionOne.ViewModel
{
    class TypePolisViewModel : VM
    {
        public PolisTypes model;
        public TypePolisViewModel(PolisTypes PolisModel)
        {
            model = PolisModel;

            Id = model.Id;
            Name = model.Name;
        }

        private int id;
        public int Id
        {
            get => id;
            set => SetField(ref id, value);
        }
        private string name;
        public string Name
        {
            get => name;
            set => SetField(ref name, value);
        }
    }
}
