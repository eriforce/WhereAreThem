using System;
using System.Collections.Generic;
using System.Text;
using WhereAreThem.Model.Models;
using IO = System.IO;

namespace WhereAreThem.Model.Persistences {
    public sealed class BinaryProvider : IFormatProvider {
        private const byte folderPrefix = 0;
        private const byte filePrefix = 1;

        private readonly byte[] _textBuffer = new byte[short.MaxValue];

        public void Save(Folder folder, IO.Stream stream) {
            IO.BinaryWriter bw = new(stream);
            Save(folder, 0, bw);
            bw.Flush();
        }

        private void Save(Folder folder, byte level, IO.BinaryWriter bw) {
            bw.Write(level++);
            bw.Write(folderPrefix);
            bw.Write(folder.CreatedDateUtc.Ticks);
            WriteText(bw, folder.Name);
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
                    WriteText(bw, f.Name);
                }
        }

        public Folder Load(IO.Stream stream) {
            IO.BinaryReader br = new(stream);

            if (stream.CanSeek) {
                stream.Seek(2, IO.SeekOrigin.Current);
            }
            else {
                br.ReadByte();
                br.ReadByte();
            }

            Dictionary<int, Folder> recentFolders = new() {
                { 0, GetFolder(br) }
            };
            try {
                while (!stream.CanSeek || stream.Position < stream.Length) {
                    byte currentLevel = br.ReadByte();
                    byte parent = (byte)(currentLevel - 1);
                    if (br.ReadByte() == folderPrefix) {
                        Folder f = GetFolder(br);
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
                        recentFolders[parent].Files.Add(GetFile(br));
                    }
                }
            }
            catch (IO.EndOfStreamException) { }

            return recentFolders[0];
        }

        private Folder GetFolder(IO.BinaryReader br) {
            DateTime created = new(br.ReadInt64());
            string name = ReadText(br);
            return new Folder {
                Name = name,
                CreatedDateUtc = created
            };
        }

        private File GetFile(IO.BinaryReader br) {
            long size = br.ReadInt64();
            DateTime created = new(br.ReadInt64());
            DateTime modified = new(br.ReadInt64());
            string name = ReadText(br);
            return new File {
                Name = name,
                FileSize = size,
                CreatedDateUtc = created,
                ModifiedDateUtc = modified,
            };
        }

        private void WriteText(IO.BinaryWriter bw, string text) {
            int length = Encoding.UTF8.GetBytes(text, 0, text.Length, _textBuffer, 0);
            bw.Write((short)length);
            bw.Write(_textBuffer, 0, length);
        }

        private string ReadText(IO.BinaryReader br) {
            int length = br.ReadInt16();
            br.Read(_textBuffer, 0, length);
            return Encoding.UTF8.GetString(_textBuffer, 0, length);
        }
    }
}
