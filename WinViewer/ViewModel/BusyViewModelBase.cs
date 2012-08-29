using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public async Task BusyAsync(string content, Action action) {
            BusyContent = content;
            IsBusy = true;
            await Task.Run(action);
            IsBusy = false;
        }
    }
}
