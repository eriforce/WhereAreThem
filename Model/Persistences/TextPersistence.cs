using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WhereAreThem.Model.Models;
using PureLib.Common;
using IO = System.IO;

namespace WhereAreThem.Model.Persistences {
    public class TextPersistence : PersistenceBase {
        private const char columnSeparator = '|';
        private const char pluginSeparator = ',';
        private const char pluginFieldSeparator = ':';

        public override void Save(Folder folder, IO.Stream stream) {
            IO.StreamWriter sw = new IO.StreamWriter(stream);
            Save(folder, 0, sw);
            sw.Flush();
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
            return string.Join(columnSeparator.ToString(), new object[] {
                level,
                itemString
            });
        }

        private string GetFolderString(Folder folder) {
            return string.Join(columnSeparator.ToString(), new object[] {
                folder.Name,
                folder.CreatedDateUtc.Ticks
            });
        }

        private string GetFileString(File file) {
            return string.Join(columnSeparator.ToString(), new object[] {
                file.Name,
                file.Size,
                file.CreatedDateUtc.Ticks,
                file.ModifiedDateUtc.Ticks,
                GetDataText(file.Data)
            });
        }

        public override Folder Load(IO.Stream stream) {
            IO.StreamReader sr = new IO.StreamReader(stream);
            string line = sr.ReadLine();
            string[] parts = line.Split(columnSeparator);
            int folderLinePartsLength = parts.Length;
            Dictionary<int, Folder> recentFolders = new Dictionary<int, Folder>() {
                { 0, GetFolder(parts) }
            };

            while (!sr.EndOfStream) {
                line = sr.ReadLine();
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
                Data = GetData(lineParts[i])
            };
        }

        private string GetDataText(Dictionary<string, string> data) {
            if (data == null)
                return null;

            return string.Join(pluginSeparator.ToString(), data.Select(p => {
                string encodedDescription = Encoding.UTF8.GetBytes(p.Value).ToBase64String();
                return $"{p.Key}{pluginFieldSeparator}{encodedDescription}";
            }));
        }

        private Dictionary<string, string> GetData(string text) {
            if (text.IsNullOrEmpty())
                return null;

            return text.Split(pluginSeparator).Select(p => {
                string[] parts = p.Split(pluginFieldSeparator);
                return new { k = parts[0], v = parts[1] };
            }).ToDictionary(p => p.k, p => Encoding.UTF8.GetString(p.v.FromBase64String()));
        }
    }
}
