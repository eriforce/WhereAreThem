using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WhereAreThem.Model.Models;
using IO = System.IO;

namespace WhereAreThem.Model.Persistences {
    public class TextPersistence : PersistenceBase {
        private const char columnSeparator = '|';
        private const string rowFormat = "{1}{0}{2}";
        private const string folderFormat = "{1}{0}{2}";
        private const string fileFormat = "{1}{0}{2}{0}{3}{0}{4}{0}{5}";

        public override void Save(Folder folder, IO.Stream stream) {
            IO.StreamWriter writer = new IO.StreamWriter(stream);
            Save(folder, 0, writer);
            writer.Flush();
        }

        private void Save(Folder folder, int level, IO.StreamWriter sw) {
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
            return string.Format(rowFormat, columnSeparator, level, itemString);
        }

        private string GetFolderString(Folder folder) {
            return string.Format(folderFormat, columnSeparator, folder.Name, folder.CreatedDateUtc.Ticks);
        }

        private string GetFileString(File file) {
            return string.Format(fileFormat, columnSeparator,
                file.Name, file.Size, file.CreatedDateUtc.Ticks, file.ModifiedDateUtc.Ticks,
                file.Description == null ? null : Convert.ToBase64String(Encoding.UTF8.GetBytes(file.Description)));
        }

        public override Folder Load(IO.Stream stream) {
            IO.StreamReader reader = new IO.StreamReader(stream);
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

                int currentLevel = int.Parse(parts[0]);
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
            int i = 1;
            return new Folder() {
                Name = lineParts[i++],
                CreatedDateUtc = new DateTime(long.Parse(lineParts[i++]))
            };
        }

        private File GetFile(string[] lineParts) {
            int i = 1;
            return new File() {
                Name = lineParts[i++],
                FileSize = long.Parse(lineParts[i++]),
                CreatedDateUtc = new DateTime(long.Parse(lineParts[i++])),
                ModifiedDateUtc = new DateTime(long.Parse(lineParts[i++])),
                Description = (i < lineParts.Length) ?
                    Encoding.UTF8.GetString(Convert.FromBase64String(lineParts[i])) : null
            };
        }
    }
}
