﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using PureLib.Common;
using WhereAreThem.Model;
using WhereAreThem.Model.Models;
using WhereAreThem.Model.Persistences;
using IO = System.IO;

namespace WhereAreThem.Converter {
    class Program {
        static void Main(string[] args) {
            IPersistence source = new TextPersistence();
            IPersistence target = new CompressedPersistence<TextPersistence>();

            string[] lists = IO.Directory.GetFiles(ConfigurationManager.AppSettings["outputPath"], 
                "*.*.wat", IO.SearchOption.AllDirectories);
            foreach (string file in lists) {
                Folder folder = source.Load(file);
                target.Save(folder, file);
            }
        }
    }
}
