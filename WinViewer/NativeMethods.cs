using System;
using System.Runtime.InteropServices;
using WhereAreThem.WinViewer.Model;

namespace WhereAreThem.WinViewer {
    internal class NativeMethods {
        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("shell32.dll", CharSet = CharSet.Ansi)]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref Shell32.SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        [DllImport("shell32.dll", CharSet = CharSet.Ansi)]
        public static extern IntPtr ExtractIcon(IntPtr hInst, string lpszExeFileName, int nIconIndex);

        [DllImport("shell32.dll", CharSet = CharSet.Ansi)]
        public static extern int ExtractIconEx(string stExeFileName, int nIconIndex, ref IntPtr phiconLarge, ref IntPtr phiconSmall, int nIcons);

        /// <summary>
        /// Wraps necessary functions imported from User32.dll. Code courtesy of MSDN Cold Rooster Consulting example.
        /// Provides access to function required to delete handle. This method is used internally
        /// and is not required to be called separately.
        /// </summary>
        /// <param name="hIcon">Pointer to icon handle.</param>
        /// <returns>N/A</returns>
        [DllImport("user32.dll")]
        public static extern int DestroyIcon(IntPtr hIcon);
    }
}
