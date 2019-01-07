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

        public string FolderCountString => ToNumber(FolderCount);
        public string FileCountString => ToNumber(FileCount);
        public string TotalSizeString => ToNumber(TotalSize);
        public string TotalSizeFriendlyString => TotalSize.ToFriendlyString();

        public PropertyInfo(IEnumerable<FileSystemItem> items) {
            IEnumerable<File> files = items.Where(i => i is File).Cast<File>();
            IEnumerable<Folder> folders = items.Where(i => i is Folder).Cast<Folder>();

            FileCount = folders.Sum(f => GetFileCount(f)) + files.Count();
            FolderCount = folders.Sum(f => GetFolderCount(f));
            TotalSize = files.Sum(f => f.Size) + folders.Sum(f => f.Size);
            if (items.Count() > 1)
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