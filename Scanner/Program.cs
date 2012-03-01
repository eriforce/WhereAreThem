using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PureLib.Common;
using PureLib.Common.Entities;
using WhereAreThem.Model;

namespace WhereAreThem {
    class Program {
        private const string updateArgumentName = "u";
        private const FileAttributes filter = FileAttributes.Hidden | FileAttributes.System;

        [STAThread]
        static void Main(string[] args) {
            IPersistence persistence = Constant.GetPersistence(Type.GetType(ConfigurationManager.AppSettings["persistence"]));
            string outputPath = Path.Combine(ConfigurationManager.AppSettings["outputPath"], Environment.MachineName);
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            Arguments arguments = new Arguments(args);
            if (arguments.ContainsKey(updateArgumentName)) {
                string path = arguments.GetValue(updateArgumentName);
                if (path.IsNullOrEmpty())
                    path = ChooseDirectory();
                if (!Directory.Exists(path))
                    throw new ArgumentException("Path '{0}' cannot be found.".FormatWith(path));
                string[] parts = path.Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
                parts[0] += Path.DirectorySeparatorChar;
                string driveLetter = parts[0].Substring(0, parts[0].IndexOf(Path.VolumeSeparatorChar));
                string listPath = Path.Combine(outputPath, Path.ChangeExtension(driveLetter, Constant.ListExt));
                if (!System.IO.File.Exists(listPath))
                    throw new FileNotFoundException("List '{0}' cannot be found.".FormatWith(listPath));
                Folder drive = persistence.Load(listPath);
                UpdateFolder(drive, parts);
                Save(persistence, listPath, drive);
            }
            else {
                foreach (string letter in ConfigurationManager.AppSettings["drives"].ToUpper().Split(',')) {
                    Folder drive = GetFolder(new DirectoryInfo("{0}{1}{2}".FormatWith(letter, Path.VolumeSeparatorChar, Path.DirectorySeparatorChar)));
                    Save(persistence, Path.Combine(outputPath, Path.ChangeExtension(letter, Constant.ListExt)), drive);
                }
            }
            Console.WriteLine();
            Console.WriteLine("List saved.");
            Console.ReadLine();
        }

        private static void Save(IPersistence persistence, string listPath, Folder drive) {
            drive.CreatedDateUtc = DateTime.UtcNow;
            persistence.Save(drive, listPath);
        }

        private static string ChooseDirectory() {
            using (FolderBrowserDialog fbdDes = new FolderBrowserDialog()) {
                fbdDes.RootFolder = Environment.SpecialFolder.MyComputer;
                fbdDes.ShowNewFolderButton = false;
                fbdDes.Description = "Choose a folder to perform updating.";
                if (fbdDes.ShowDialog() == DialogResult.OK)
                    return fbdDes.SelectedPath;
                else
                    return null;
            }
        }

        private static void UpdateFolder(Folder root, string[] pathParts) {
            Folder folder = root;
            for (int i = 1; i < pathParts.Length; i++) {
                if (folder.Folders == null)
                    folder.Folders = new List<Folder>();
                Folder current = folder.Folders.SingleOrDefault(f => f.Name.Equals(pathParts[i], StringComparison.OrdinalIgnoreCase));
                if (current == null) {
                    current = GetFolder(new DirectoryInfo(Path.Combine(pathParts.Take(i + 1).ToArray())));
                    folder.Folders.Add(current);
                    folder.Folders.Sort();
                    return;
                }
                else
                    folder = current;
            }
            Folder newFolder = GetFolder(new DirectoryInfo(Path.Combine(pathParts)));
            folder.CreatedDateUtc = newFolder.CreatedDateUtc;
            folder.Folders = newFolder.Folders;
            folder.Files = newFolder.Files;
        }

        private static Folder GetFolder(DirectoryInfo directory) {
            Console.WriteLine(directory.FullName);
            Folder folder = new Folder() {
                Name = directory.Name,
                CreatedDateUtc = directory.CreationTimeUtc,
            };
            try {
                folder.Files = directory.GetFiles()
                     .Where(f => !f.Attributes.HasFlag(filter))
                     .Select(f => new WhereAreThem.Model.File() {
                         Name = f.Name,
                         FileSize = f.Length,
                         CreatedDateUtc = f.CreationTimeUtc,
                         ModifiedDateUtc = f.LastWriteTimeUtc
                     }).ToList();
                folder.Folders = directory.GetDirectories()
                        .Where(d => !d.Attributes.HasFlag(filter))
                        .Select(d => GetFolder(d)).ToList();
                folder.Files.Sort();
                folder.Folders.Sort();
            }
            catch (Exception) { }
            return folder;
        }
    }
}
