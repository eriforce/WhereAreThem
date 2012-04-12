using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PureLib.Common;

namespace WhereAreThem.Model {
    public class PropertyInfo {
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
                return Utility.ToFriendlyString(TotalSize);
            }
        }

        public PropertyInfo(Folder parent, string[] selectedItems) {
            List<File> files = parent.Files == null ? new List<File>() :
                parent.Files.Where(i => selectedItems.Contains(i.Name)).ToList();
            List<Folder> folders = parent.Folders == null ? new List<Folder>() :
                parent.Folders.Where(i => selectedItems.Contains(i.Name)).ToList();

            FileCount = folders.Sum(f => GetFileCount(f)) + files.Count;
            FolderCount = folders.Sum(f => GetFolderCount(f));
            TotalSize = files.Sum(f => f.Size) + folders.Sum(f => f.Size);

            if (selectedItems.Length > 1)
                FolderCount += folders.Count;
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