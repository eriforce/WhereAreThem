using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WhereIsThem.Model {
    public static class Persistence {
        public static void Save(this Folder folder, string path) {
            using (StreamWriter sw = new FileInfo(path).CreateText()) {
                Save(folder, 0, sw);
            }
        }

        public static Folder Load(string path) {
            using (StreamReader sr = new FileInfo(path).OpenText()) {
                string line = sr.ReadLine();
                string[] parts = line.Split(Constant.ColumnSeparator);
                Dictionary<int, Folder> recentFolders = new Dictionary<int, Folder>() {
                    { 0, GetFolder(parts) }
                };
                int prevLevel = 0;
                bool isFile = false;

                while (!sr.EndOfStream) {
                    line = sr.ReadLine();
                    if (string.IsNullOrEmpty(line))
                        continue;
                    parts = line.Split(Constant.ColumnSeparator);

                    int currentLevel = GetLevel(parts);
                    if (currentLevel == prevLevel) {
                        if (GetName(parts) == Constant.FolderFileSeparator.ToString())
                            isFile = true;
                        else if (!isFile)
                            AddFolder(parts, recentFolders, currentLevel);
                        else {
                            int parent = currentLevel - 1;
                            if (recentFolders[parent].Files == null)
                                recentFolders[parent].Files = new List<File>();
                            recentFolders[parent].Files.Add(GetFile(parts));
                        }
                    }
                    else {
                        if (GetName(parts) == Constant.FolderFileSeparator.ToString())
                            isFile = true;
                        else
                            AddFolder(parts, recentFolders, currentLevel);
                    }
                    prevLevel = currentLevel;
                }
                return recentFolders[0];
            }
        }

        private static void Save(Folder folder, int level, StreamWriter sw) {
            sw.WriteLine(GetRow(level++, folder));
            if (folder.Folders != null)
                foreach (Folder f in folder.Folders) {
                    Save(f, level, sw);
                }
            sw.WriteLine(GetRow(level, Constant.FolderFileSeparator));
            if (folder.Files != null)
                foreach (File f in folder.Files) {
                    sw.WriteLine(GetRow(level, f));
                }
        }

        private static string GetRow(int level, object item) {
            return string.Format(Constant.RowFormat, level, Constant.ColumnSeparator, item.ToString());
        }

        private static void AddFolder(string[] parts, Dictionary<int, Folder> recentFolders, int currentLevel) {
            int parent = currentLevel - 1;
            Folder f = GetFolder(parts);
            if (recentFolders[parent].Folders == null)
                recentFolders[parent].Folders = new List<Folder>();
            recentFolders[parent].Folders.Add(f);

            if (recentFolders.ContainsKey(currentLevel))
                recentFolders[currentLevel] = f;
            else
                recentFolders.Add(currentLevel, f);
        }

        private static Folder GetFolder(string[] lineParts) {
            return new Folder() {
                Name = GetName(lineParts),
                CreatedDateUtc = new DateTime(long.Parse(lineParts[2]))
            };
        }

        private static File GetFile(string[] lineParts) {
            return new File() {
                Name = GetName(lineParts),
                Size = long.Parse(lineParts[2]),
                CreatedDateUtc = new DateTime(long.Parse(lineParts[3])),
                ModifiedDateUtc = new DateTime(long.Parse(lineParts[4]))
            };
        }

        private static int GetLevel(string[] lineParts) {
            return int.Parse(lineParts[0]);
        }

        private static string GetName(string[] lineParts) {
            return lineParts[1];
        }

    }
}
