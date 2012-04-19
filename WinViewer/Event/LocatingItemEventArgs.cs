using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WhereAreThem.WinViewer {
    public class LocatingItemEventArgs : EventArgs {
        public SearchResult Result { get; private set; }

        public LocatingItemEventArgs(SearchResult result) {
            Result = result;
        }
    }

    public delegate void LocatingItemEventHandler(object sender, LocatingItemEventArgs e);
}
