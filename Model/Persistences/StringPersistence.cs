using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WhereAreThem.Model {
    public class StringPersistence : PersistenceBase {
        private const char columnSeparator = '|';
        private const string rowFormat = "{0}{1}{2}";
        private const string folderFormat = "{1}{0}{2}";
        private const string fileFormat = "{1}{0}{2}{0}{3}{0}{4}";

        public override void Save(Folder folder, Stream stream) {
            StreamWriter writer = new StreamWriter(stream);
            Save(folder, 0, writer);
            writer.Flush();
        }

        private void Save(Folder folder, int level, StreamWriter sw) {
            sw.WriteLine(GetRow(level++, GetFolderString(folder)));
            if (folder.Folders != null)
                foreach (Folder f in folder.Folders) {
                    Save(f, level, sw);
                }
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

        public override Folder Load(Stream stream) {
            StreamReader reader = new StreamReader(stream);
            string line = reader.ReadLine();
            string[] parts = line.Split(columnSeparator);
            int folderLinePartsLength = parts.Length;
            Dictionary<int, Folder> recentFolders = new Dictionary<int, Folder>() {
                    { 0, GetFolder(parts) }
                };

            while (!reader.EndOfStream) {
                line = reader.ReadLine();
                if (string.IsNullOrEmpty(line))
                    continue;
                parts = line.Split(columnSeparator);

                int currentLevel = GetLevel(parts);
                int parent = currentLevel - 1;
                if (parts.Length == folderLinePartsLength) {
                    Folder f = GetFolder(parts);
                    if (recentFolders[parent].Folders == null)
                        recentFolders[parent].Folders = new List<Folder>();
                    recentFolders[parent].Folders.Add(f);

                    if (recentFolders.ContainsKey(currentLevel))
                        recentFolders[currentLevel] = f;
                    else
                        recentFolders.Add(currentLevel, f);
                }
                else {
                    if (recentFolders[parent].Files == null)
                        recentFolders[parent].Files = new List<File>();
                    recentFolders[parent].Files.Add(GetFile(parts));
                }
            }
            return recentFolders[0];
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
                FileSize = long.Parse(lineParts[2]),
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
