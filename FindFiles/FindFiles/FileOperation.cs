using System;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using SysLab1;
using System.Collections.Generic;

namespace FindFiles
{
    public class FileOperation
    {
        public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
        public static readonly int FILE_ATTRIBUTE_READONLY = 0x1;
        public static readonly int FILE_ATTRIBUTE_HIDDEN = 0x2;
        public static readonly int FILE_ATTRIBUTE_SYSTEM = 0x4;
        public static readonly int FILE_ATTRIBUTE_DIRECTORY = 0x10;
        public static readonly int FILE_ATTRIBUTE_ARCHIVE = 0x20;
        public static readonly int FILE_ATTRIBUTE_DEVICE = 0x40;
        public static readonly int FILE_ATTRIBUTE_NORMAL = 0x80;
        public static readonly int FILE_ATTRIBUTE_TEMPORARY = 0x100;
        public static readonly int FILE_ATTRIBUTE_SPARSE_FILE = 0x200;
        public static readonly int FILE_ATTRIBUTE_REPARSE_POINT = 0x400;
        public static readonly int FILE_ATTRIBUTE_COMPRESSED = 0x800;
        public static readonly int FILE_ATTRIBUTE_OFFLINE = 0x1000;
        public static readonly int FILE_ATTRIBUTE_NOT_CONTENT_INDEXED = 0x2000;
        public static readonly int FILE_ATTRIBUTE_ENCRYPTED = 0x4000;
        public static readonly int FILE_ATTRIBUTE_VIRTUAL = 0x10000;

        [Serializable,
            StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto),
            BestFitMapping(false)]
        private struct WIN32_FIND_DATA
        {
            public int dwFileAttributes;
            public int ftCreationTime_dwLowDateTime;
            public int ftCreationTime_dwHighDateTime;
            public int ftLastAccessTime_dwLowDateTime;
            public int ftLastAccessTime_dwHighDateTime;
            public int ftLastWriteTime_dwLowDateTime;
            public int ftLastWriteTime_dwHighDateTime;
            public int nFileSizeHigh;
            public int nFileSizeLow;
            public int dwReserved0;
            public int dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
        }

        private static bool MakeOperation(uint operation, string source, string destination = "")
        {
            ShellApi.SHFILEOPSTRUCT fileStruct = new ShellApi.SHFILEOPSTRUCT();

            fileStruct.hwnd = IntPtr.Zero;
            fileStruct.wFunc = operation;
            fileStruct.pFrom = Marshal.StringToHGlobalUni(source);
            fileStruct.pTo = Marshal.StringToHGlobalUni(destination);
            fileStruct.fFlags = (ushort)ShellFileOperation.ShellFileOperationFlags.FOF_ALLOWUNDO;
            fileStruct.fAnyOperationsAborted = 0;
            fileStruct.hNameMappings = IntPtr.Zero;
            fileStruct.lpszProgressTitle = "";

            int error = ShellApi.SHFileOperation(ref fileStruct);

            ShellApi.SHChangeNotify(
                (uint)ShellFileOperation.ShellChangeNotificationEvents.SHCNE_ALLEVENTS,
                (uint)ShellFileOperation.ShellChangeNotificationFlags.SHCNF_DWORD,
                IntPtr.Zero,
                IntPtr.Zero);

            if (error != 0)
                return false;

            if (fileStruct.fAnyOperationsAborted != 0)
                return false;

            return true;
        }

        private static string StringsToMultiString(ObservableCollection<string> strings)
        {
            string str = "";
            // if null or if count == 0 then return empty string
            if ((strings?.Count ?? 0) == 0)
                return str;

            for (int i = 0; i < strings.Count; ++i)
                str += strings[i] + '\0';
            str += '\0';

            return str;
        }

        private static string MasksToMultiMask(string sourceFolder, string mask)
        {
            ObservableCollection<string> temp = new ObservableCollection<string>();
            string[] masks;
            char[] separator = new char[] { '|' };

            masks = mask.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < masks.Length; ++i)
            {
                string[] filesWithMask = Directory.GetFiles(sourceFolder, masks[i]);
                int filesWithMaskCount = filesWithMask.Length;

                for (int j = 0; j < filesWithMaskCount; ++j)
                    if (!(temp.Contains(filesWithMask[j])))
                        temp.Add(filesWithMask[j]);
            }

            return StringsToMultiString(temp);
        }

        private static bool IsValid(string sourceFolder, string checkedProperty)
        {
            if (checkedProperty == null)
            {
                MessageBox.Show("Отсутствует маска, либо название файла.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!Directory.Exists(sourceFolder))
            {
                MessageBox.Show("Начальная директория отсутствует.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private static bool IsValid(ObservableCollection<string> list)
        {
            if ((list?.Count ?? 0) == 0)
            {
                MessageBox.Show("Отсутствуют файлы.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private static bool IsHaveFlag(int value, int flag) => (value & flag) == flag;

        public static void MoveFiles(string sourceFolder, string destFolder, string mask)
        {
            if (!IsValid(sourceFolder, mask))
                return;

            MakeOperation(0x0001, MasksToMultiMask(sourceFolder, mask), destFolder);
        }

        public static void MoveFiles(ObservableCollection<string> files, string destFolder)
        {
            if (!IsValid(files))
                return;

            MakeOperation(0x0001, StringsToMultiString(files), destFolder);
        }

        public static void CopyFiles(string sourceFolder, string destFolder, string mask)
        {
            if (!IsValid(sourceFolder, mask))
                return;

            MakeOperation(0x0002, MasksToMultiMask(sourceFolder, mask), destFolder);
        }

        public static void CopyFiles(ObservableCollection<string> files, string destFolder)
        {
            if (!IsValid(files))
                return;

            MakeOperation(0x0002, StringsToMultiString(files), destFolder);
        }

        public static void DeleteFiles(string sourceFolder, string mask)
        {
            if (!IsValid(sourceFolder, mask))
                return;

            MakeOperation(0x0003, MasksToMultiMask(sourceFolder, mask));
        }

        public static void DeleteFiles(ObservableCollection<string> files)
        {
            if (!IsValid(files))
                return;

            MakeOperation(0x0003, StringsToMultiString(files));
        }

        public static void RenameFile(string source, string newName)
        {
            string dest = source.Substring(0, source.LastIndexOf('\\') + 1) + newName;
            source += '\0';
            MakeOperation(0x0004, source, dest);
        }

        public static List<string> SearchFiles(string folder,
            string mask,
            SearchType searchType,
            bool isHidden,
            out int fullSize,
            int minSize = 0,
            int maxSize = 0)
        {
            List<string> items = new List<string>();

            // if folder is null return empty string else check if folder empty string
            if ((folder ?? "").Equals("")) throw new Exception();
            if ((folder[folder.Length - 1] != '\\') && (folder[folder.Length - 1] != '/')) folder += '\\';
            mask = (mask == "") ? "*" : mask;

            string[] masks;
            char[] separator = new char[] { '|' };
            fullSize = 0;
            masks = mask.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < masks.Length; ++i)
            {
                WIN32_FIND_DATA findData = new WIN32_FIND_DATA();
                string folderWithMask = folder + masks[i];
                IntPtr handle = FindFirstFile(folderWithMask, ref findData);
                if (handle == INVALID_HANDLE_VALUE) continue;

                do
                {
                    string itemName = findData.cFileName;
                    if ((itemName == ".") || (itemName == "..")) continue;

                    int size = (findData.nFileSizeHigh << 0x20) + findData.nFileSizeLow;
                    bool isDirectory = IsHaveFlag(findData.dwFileAttributes, FILE_ATTRIBUTE_DIRECTORY);

                    if (!isDirectory && (minSize != -1 && minSize > size || maxSize != -1 && maxSize < size)) continue;
                    if (isHidden && !IsHaveFlag(findData.dwFileAttributes, FILE_ATTRIBUTE_HIDDEN)) continue;
                    if (searchType == SearchType.DIRICTORIES && !isDirectory) continue;
                    if (searchType == SearchType.FILES && isDirectory) continue;

                    items.Add(folder + itemName + (isDirectory ? '\\' : '\0'));
                    fullSize += size;
                    System.Windows.Forms.Application.DoEvents();
                } while (FindNextFile(handle, ref findData));
                FindClose(handle);
            }

            return items;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr FindFirstFile(string pFileName, ref WIN32_FIND_DATA pFindFileData);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool FindNextFile(IntPtr hndFindFile, ref WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FindClose(IntPtr hndFindFile);
    }
}
