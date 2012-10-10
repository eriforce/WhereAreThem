using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using PureLib.Common;

namespace WhereAreThem.Model.Models {
    public class Drive : Folder {
        public DriveType DriveType { get; set; }

        public string DriveLetter {
            get { return GetDriveLetter(Name); }
        }

        public Folder ToFolder() {
            return new Folder() {
                Name = Name,
                CreatedDateUtc = CreatedDateUtc,
                Folders = Folders,
                Files = Files,
            };
        }

        public static string GetDriveLetter(string path) {
            return path.Substring(0, path.IndexOf(Path.VolumeSeparatorChar)).ToUpper();
        }

        public static string GetDrivePath(string letter) {
            return "{0}{1}{2}".FormatWith(letter.ToUpper(), Path.VolumeSeparatorChar, Path.DirectorySeparatorChar);
        }

        public static Drive FromFolder(Folder folder, DriveType driveType) {
            return new Drive() {
                DriveType = driveType,
                Name = folder.Name,
                CreatedDateUtc = folder.CreatedDateUtc,
                Folders = folder.Folders,
                Files = folder.Files,
            };
        }
    }
}
