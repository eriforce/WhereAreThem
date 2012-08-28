using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PureLib.Common;
using WhereAreThem.Model;
using WhereAreThem.Model.Models;
using WhereAreThem.Model.Persistences;
using IO = System.IO;

namespace WhereAreThem.Scanner {
    class Program {
        [STAThread]
        static void Main(string[] args) {
            IPersistence persistence = Constant.Persistence;
            string outputPath = ConfigurationManager.AppSettings["outputPath"].WrapPath();
            Model.Scanner scanner = new Model.Scanner(outputPath, Constant.Persistence);
            scanner.PrintLine += new StringEventHandler((s, e) => { Console.WriteLine(e.String); });

            const string updateArgumentName = "u";
            Arguments arguments = new Arguments(args);
            if (arguments.ContainsKey(updateArgumentName)) {
                string path = null;
                while (true) {
                    path = ChooseDirectory(path);
                    if (path.IsNullOrEmpty())
                        break;
                    Console.WriteLine();
                    scanner.ScanUpdate(path);
                    Console.WriteLine();
                    Console.WriteLine("List saved.");
                    Console.WriteLine("Press X to exit.");
                }
            }
            else {
                foreach (string letter in ConfigurationManager.AppSettings["drives"].ToUpper().Split(',')) {
                    scanner.Scan(letter);
                    Console.WriteLine();
                    Console.WriteLine("List saved.");
                }
                Console.ReadLine();
            }
        }

        private static string ChooseDirectory(string selectedPath) {
            using (FolderBrowserDialog fbdDes = new FolderBrowserDialog()) {
                fbdDes.RootFolder = Environment.SpecialFolder.MyComputer;
                fbdDes.SelectedPath = selectedPath;
                fbdDes.ShowNewFolderButton = false;
                fbdDes.Description = "Choose a folder to perform updating.";
                if (fbdDes.ShowDialog() == DialogResult.OK)
                    return fbdDes.SelectedPath;
                else
                    return null;
            }
        }
    }
}
