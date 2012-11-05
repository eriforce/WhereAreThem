using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhereAreThem.WinViewer.Event;

namespace WhereAreThem.WinViewer.Model {
    public class ExplorerNavigationService {
        private readonly int _navigationHistoryCount = int.Parse(ConfigurationManager.AppSettings["navigationHistoryCount"]);
        private List<LocatingItemEventArgs> _historyEntries;
        private int _currentEntryIndex;

        public LocatingItemEventArgs CurrentEntry {
            get { return _historyEntries.Any() ? _historyEntries[_currentEntryIndex] : null; }
        }
        public bool CanGoBack {
            get { return _currentEntryIndex > 0; }
        }
        public bool CanGoForward {
            get { return _currentEntryIndex < _historyEntries.Count - 1; }
        }

        public ExplorerNavigationService() {
            _historyEntries = new List<LocatingItemEventArgs>();
            _currentEntryIndex = -1;
        }

        public void AddBackEntry(LocatingItemEventArgs entry) {
            if (CanGoForward)
                _historyEntries.RemoveRange(_currentEntryIndex + 1, _historyEntries.Count - 1 - _currentEntryIndex);

            if (_historyEntries.Count >= _navigationHistoryCount)
                _historyEntries.RemoveAt(0);
            else
                _currentEntryIndex++;

            _historyEntries.Add(entry);
        }

        public void GoBack() {
            if (CanGoBack)
                _currentEntryIndex--;
        }

        public void GoForward() {
            if (CanGoForward)
                _currentEntryIndex++;
        }
    }
}
