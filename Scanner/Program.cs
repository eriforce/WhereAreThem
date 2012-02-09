using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using PureLib.Common;
using WhereAreThem.Model;

namespace WhereAreThem {
    class Program {
        private const FileAttributes filter = FileAttributes.Hidden | FileAttributes.System;

        static void Main(string[] args) {
            IPersistence persistence = Constant.GetPersistence(Type.GetType(ConfigurationManager.AppSettings["persistence"]));
            string outputPath = Path.Combine(ConfigurationManager.AppSettings["outputPath"], Environment.MachineName);
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);
            foreach (string letter in ConfigurationManager.AppSettings["drives"].ToUpper().Split(',')) {
                Folder f = GetDirectory(new DirectoryInfo("{0}:\\".FormatWith(letter)));
                persistence.Save(f, Path.Combine(outputPath, Path.ChangeExtension(letter, Constant.ListExt)));
            }
            Console.WriteLine("List saved.");
            Console.ReadLine();
        }

        private static Folder GetDirectory(DirectoryInfo directory) {
            Folder folder = new Folder() {
                Name = directory.Name,
                CreatedDateUtc = directory.CreationTimeUtc,
            };
            try {
                folder.Files = directory.GetFiles()
                     .Where(f => !f.Attributes.HasFlag(filter))
                     .Select(f => new WhereAreThem.Model.File() {
                         Name = f.Name,
                         Size = f.Length,
                         CreatedDateUtc = f.CreationTimeUtc,
                         ModifiedDateUtc = f.LastWriteTimeUtc
                     }).ToList();
                folder.Folders = directory.GetDirectories()
                        .Where(d => !d.Attributes.HasFlag(filter))
                        .Select(d => GetDirectory(d)).ToList();
            }
            catch (Exception) { }
            return folder;
        }
    }
}
