using System.Collections.Generic;
using System.Linq;
using PureLib.Common;

namespace WhereAreThem.Model.Models {
    public class PropertyInfo {
        public List<File> Files { get; private set; }
        public List<Folder> Folders { get; private set; }
        public int FolderCount { get; private set; }
        public int FileCount { get; private set; }
        public long TotalSize { get; private set; }

        public string FolderCountString => ToNumber(FolderCount);
        public string FileCountString => ToNumber(FileCount);
        public string TotalSizeString => ToNumber(TotalSize);
        public string TotalSizeFriendlyString => TotalSize.ToFriendlyString();

        public PropertyInfo(IEnumerable<FileSystemItem> items) {
            Files = items.Where(i => i is File).Cast<File>().ToList();
            Folders = items.Where(i => i is Folder).Cast<Folder>().ToList();

            FileCount = Folders.Sum(f => GetFileCount(f)) + Files.Count();
            FolderCount = Folders.Sum(f => GetFolderCount(f));
            TotalSize = Files.Sum(f => f.Size) + Folders.Sum(f => f.Size);
            if (items.Count() > 1)
                FolderCount += Folders.Count();
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