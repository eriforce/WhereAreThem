using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PureLib.WPF;

namespace WhereAreThem.WinViewer.ViewModel {
    public abstract class BusyViewModelBase : ViewModelBase {
        private bool _isBusy;
        private string _busyContent;

        public bool IsBusy {
            get { return _isBusy; }
            set {
                _isBusy = value;
                RaiseChange(() => IsBusy);
            }
        }
        public string BusyContent {
            get { return _busyContent; }
            set {
                _busyContent = value;
                RaiseChange(() => BusyContent);
            }
        }

        public void Busy(string content, Action action) {
            BusyContent = content;
            IsBusy = true;
            action();
            IsBusy = false;
        }
    }
}
