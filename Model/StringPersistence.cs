using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WhereAreThem.Model {
    public class StringPersistence : IPersistence {
        private const char columnSeparator = '|';
        private const char folderFileSeparator = '.';
        private const string rowFormat = "{0}{1}{2}";
        private const string folderFormat = "{1}{0}{2}";
        private const string fileFormat = "{1}{0}{2}{0}{3}{0}{4}";

        public void Save(Folder folder, string path) {
            string tempFile = Path.ChangeExtension(path, "tmp");
            using (StreamWriter sw = new FileInfo(tempFile).CreateText()) {
                Save(folder, 0, sw);
            }
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
            System.IO.File.Move(tempFile, path);
        }

        private void Save(Folder folder, int level, StreamWriter sw) {
            sw.WriteLine(GetRow(level++, GetFolderString(folder)));
            if (folder.Folders != null)
                foreach (Folder f in folder.Folders) {
                    Save(f, level, sw);
                }
            sw.WriteLine(GetRow(level, folderFileSeparator.ToString()));
            if (folder.Files != null)
                foreach (File f in folder.Files) {
                    sw.WriteLine(GetRow(level, GetFileString(f)));
                }
        }

        private string GetRow(int level, string itemString) {
            return string.Format(rowFormat, level, columnSeparator, itemString);
        }


        private string GetFolderString(Folder folder) {
            return string.Format(folderFormat, columnSeparator,
                folder.Name, folder.CreatedDateUtc.Ticks);
        }

        private string GetFileString(File file) {
            return string.Format(fileFormat, columnSeparator,
                file.Name, file.Size, file.CreatedDateUtc.Ticks, file.ModifiedDateUtc.Ticks);
        }

        public Folder Load(string path) {
            using (StreamReader sr = new FileInfo(path).OpenText()) {
                string line = sr.ReadLine();
                string[] parts = line.Split(columnSeparator);
                Dictionary<int, Folder> recentFolders = new Dictionary<int, Folder>() {
                    { 0, GetFolder(parts) }
                };
                int prevLevel = 0;
                bool isFile = false;

                while (!sr.EndOfStream) {
                    line = sr.ReadLine();
                    if (string.IsNullOrEmpty(line))
                        continue;
                    parts = line.Split(columnSeparator);

                    int currentLevel = GetLevel(parts);
                    if (currentLevel == prevLevel) {
                        if (GetName(parts) == folderFileSeparator.ToString())
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
                        if (GetName(parts) == folderFileSeparator.ToString())
                            isFile = true;
                        else
                            AddFolder(parts, recentFolders, currentLevel);
                    }
                    prevLevel = currentLevel;
                }
                return recentFolders[0];
            }
        }

        private void AddFolder(string[] parts, Dictionary<int, Folder> recentFolders, int currentLevel) {
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

        private Folder GetFolder(string[] lineParts) {
            return new Folder() {
                Name = GetName(lineParts),
                CreatedDateUtc = new DateTime(long.Parse(lineParts[2]))
            };
        }

        private File GetFile(string[] lineParts) {
            return new File() {
                Name = GetName(lineParts),
                Size = long.Parse(lineParts[2]),
                CreatedDateUtc = new DateTime(long.Parse(lineParts[3])),
                ModifiedDateUtc = new DateTime(long.Parse(lineParts[4]))
            };
        }

        private int GetLevel(string[] lineParts) {
            return int.Parse(lineParts[0]);
        }

        private string GetName(string[] lineParts) {
            return lineParts[1];
        }
    }
}
