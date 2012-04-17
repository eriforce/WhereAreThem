using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using PureLib.Common;
using WhereAreThem.Model;

namespace WhereAreThem.PersistenceConverter {
    class Program {
        static void Main(string[] args) {
            IPersistence source = new StringPersistence();
            IPersistence target = new CompressedPersistence<StringPersistence>();

            string[] lists = Directory.GetFiles(ConfigurationManager.AppSettings["outputPath"], 
                "*.{0}".FormatWith(Constant.ListExt), SearchOption.AllDirectories);
            foreach (string file in lists) {
                Folder folder = source.Load(file);
                target.Save(folder, file);
            }
        }
    }
}
