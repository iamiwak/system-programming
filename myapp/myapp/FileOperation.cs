using System;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;
using SysLab1;
using System.IO;
using System.Windows;

namespace myapp
{
    public class FileOperation
    {
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


    }
}
