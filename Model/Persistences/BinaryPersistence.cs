using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PureLib.Common;
using WhereAreThem.Model.Models;
using IO = System.IO;

namespace WhereAreThem.Model.Persistences {
    public class BinaryPersistence : PersistenceBase {
        private const char stringSeparator = '\0';
        private const byte folderPrefix = 0;
        private const byte filePrefix = 1;

        public override void Save(Folder folder, IO.Stream stream) {
            IO.BinaryWriter writer = new IO.BinaryWriter(stream);
            Save(folder, 0, writer);
            writer.Flush();
        }

        private void Save(Folder folder, byte level, IO.BinaryWriter bw) {
            bw.Write(level++);
            bw.Write(folderPrefix);
            bw.Write(folder.CreatedDateUtc.Ticks);
            byte[] stringData = Encoding.UTF8.GetBytes(folder.Name);
            bw.Write((byte)stringData.Length);
            bw.Write(stringData);
            if (folder.Folders != null)
                foreach (Folder f in folder.Folders) {
                    Save(f, level, bw);
                }
            if (folder.Files != null)
                foreach (File f in folder.Files) {
                    bw.Write(level);
                    bw.Write(filePrefix);
                    bw.Write(f.Size);
                    bw.Write(f.CreatedDateUtc.Ticks);
                    bw.Write(f.ModifiedDateUtc.Ticks);
                    stringData = Encoding.UTF8.GetBytes(string.Join(stringSeparator.ToString(), f.Name, f.Description));
                    bw.Write(stringData.Length);
                    bw.Write(stringData);
                }
        }

        public override Folder Load(IO.Stream stream) {
            IO.BinaryReader reader = new IO.BinaryReader(stream);
            stream.Seek(2, IO.SeekOrigin.Begin);
            Dictionary<int, Folder> recentFolders = new Dictionary<int, Folder>() {
                { 0, GetFolder(reader) }
            };
            while (stream.Position < stream.Length) {
                byte currentLevel = reader.ReadByte();
                byte parent = (byte)(currentLevel - 1);
                if (reader.ReadByte() == folderPrefix) {
                    Folder f = GetFolder(reader);
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
                    recentFolders[parent].Files.Add(GetFile(reader));
                }
            }
            return recentFolders[0];
        }

        private Folder GetFolder(IO.BinaryReader br) {
            DateTime created = new DateTime(br.ReadInt64());
            int strLength = br.ReadByte();
            string name = Encoding.UTF8.GetString(br.ReadBytes(strLength));
            return new Folder() {
                Name = name,
                CreatedDateUtc = created
            };
        }

        private File GetFile(IO.BinaryReader br) {
            long size = br.ReadInt64();
            DateTime created = new DateTime(br.ReadInt64());
            DateTime modified = new DateTime(br.ReadInt64());
            int strLength = br.ReadInt32();
            string[] parts = Encoding.UTF8.GetString(br.ReadBytes(strLength)).Split('\0');
            return new File() {
                Name = parts[0],
                FileSize = size,
                CreatedDateUtc = created,
                ModifiedDateUtc = modified,
                Description = parts[1]
            };
        }
    }
}
