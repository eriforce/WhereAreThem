using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using PureLib.Common;

namespace WhereAreThem.Model {
    public class PropertyInfo {
        private File[] _files;
        private Folder[] _folders;

        public int FolderCount { get; private set; }
        public int FileCount { get; private set; }
        public long TotalSize { get; private set; }

        public string FolderCountString {
            get {
                return ToNumber(FolderCount);
            }
        }
        public string FileCountString {
            get {
                return ToNumber(FileCount);
            }
        }
        public string TotalSizeString {
            get {
                return ToNumber(TotalSize);
            }
        }
        public string TotalSizeFriendlyString {
            get {
                return TotalSize.ToFriendlyString();
            }
        }

        public PropertyInfo(Folder parent, string[] selectedItems) {
            _files = parent.Files == null ? new File[] { } :
                parent.Files.Where(i => selectedItems.Contains(i.Name)).ToArray();
            _folders = parent.Folders == null ? new Folder[] { } :
                parent.Folders.Where(i => selectedItems.Contains(i.Name)).ToArray();

            FileCount = _folders.Sum(f => GetFileCount(f)) + _files.Length;
            FolderCount = _folders.Sum(f => GetFolderCount(f));
            TotalSize = _files.Sum(f => f.Size) + _folders.Sum(f => f.Size);
            if (selectedItems.Length > 1)
                FolderCount += _folders.Length;
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