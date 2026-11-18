using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Storage.FileSystem;
using Windows.Win32.UI.Shell;

namespace FileSleuth.FileManager
{
    internal class FileIconHandler
    {
        Dictionary<string, BitmapSource> extensionIconSrcs;
        public FileIconHandler()
        {
            extensionIconSrcs = new Dictionary<string, BitmapSource>();
        }

        public void TestIcon(FileData file)
        {
            string filePath = file.FilePath;

            string pathExtension = Path.GetExtension(filePath);

            if (extensionIconSrcs.ContainsKey(pathExtension))
            {
                file.FileIcon = extensionIconSrcs[pathExtension];
                return;
            }

            SHFILEINFOW shFileInfo = new SHFILEINFOW();

            FILE_FLAGS_AND_ATTRIBUTES shgi_attributes;

            if (file.IsDirectory)
                shgi_attributes = FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_DIRECTORY;
            else
                shgi_attributes = FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_NORMAL;

            nuint success = PInvoke.SHGetFileInfo(filePath,
                shgi_attributes,
                ref shFileInfo,
                SHGFI_FLAGS.SHGFI_ICON | SHGFI_FLAGS.SHGFI_SMALLICON | SHGFI_FLAGS.SHGFI_USEFILEATTRIBUTES);

            if (success != 0)
            {
                BitmapSource iconSrc = Imaging.CreateBitmapSourceFromHIcon(shFileInfo.hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                iconSrc.Freeze();

                file.FileIcon = iconSrc;
                extensionIconSrcs[pathExtension] = iconSrc;
                PInvoke.DestroyIcon(shFileInfo.hIcon);
            }
        }
    }
}


