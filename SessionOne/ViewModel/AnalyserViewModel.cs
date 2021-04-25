using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SessionOne.ViewModel
{
    public class AnalyserViewModel : VM
    {
        public Analyzers model;
        public AnalyserViewModel(Analyzers AnalyserModel)
        {
            model = AnalyserModel;

            Id = model.Id;
            Name = model.Name;
        }

        public int Id { get; set; }
        public string Name { get; set; }
    }
}
