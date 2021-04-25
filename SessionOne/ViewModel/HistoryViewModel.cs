using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SessionOne.ViewModel
{
    public class HistoryViewModel : VM
    {
        private TimeSpan _Time;
        public TimeSpan Time
        {
            get => _Time;
            set => SetField(ref _Time, value);
        }

        private string _LoginName;
        public string LoginName
        {
            get => _LoginName;
            set => SetField(ref _LoginName, value);
        }

        private string _Status;
        public string Status
        {
            get => _Status;
            set => SetField(ref _Status, value);
        }

        public virtual Users Users { get; set; }
    }
}
