using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using PureLib.Common;

namespace WhereAreThem.Model.Models {
    public class PropertyInfo {
        public int FolderCount { get; private set; }
        public int FileCount { get; private set; }
        public long TotalSize { get; private set; }

        public string FolderCountString
            => ToNumber(FolderCount);
        public string FileCountString
            => ToNumber(FileCount);
        public string TotalSizeString
            => ToNumber(TotalSize);
        public string TotalSizeFriendlyString
            => TotalSize.ToFriendlyString();

        public PropertyInfo(Folder parent, string[] selectedItems) {
            IEnumerable<File> files = (parent.Files == null) ? new File[0] :
                 parent.Files.Where(i => selectedItems.Contains(i.Name));
            IEnumerable<Folder> folders = (parent.Folders == null) ? new Folder[0] :
                 parent.Folders.Where(i => selectedItems.Contains(i.Name));

            FileCount = folders.Sum(f => GetFileCount(f)) + files.Count();
            FolderCount = folders.Sum(f => GetFolderCount(f));
            TotalSize = files.Sum(f => f.Size) + folders.Sum(f => f.Size);
            if (selectedItems.Length > 1)
                FolderCount += folders.Count();
        }

        private int GetFileCount(Folder folder) {
            int count = folder.Files == null ? 0 : folder.Files.Count;
            if (folder.Folders != null)
                count += folder.Folders.Sum(f => GetFileCount(f));
            return count;
        }

        private int GetFolderCount(Folder folder) {
            return folder.Folders == null ? 0 : (folder.Folders.Count + folder.Folders.Sum(f => GetFolderCount(f)));
        }

        private string ToNumber(long value) {
            return value.ToString("n0");
        }
    }
}